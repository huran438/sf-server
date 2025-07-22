using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SFServer.API;
using SFServer.API.Data;
using SFServer.API.Utils;
using SFServer.API.Services;
using SFServer.Shared.Server.UserProfile;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

DotNetEnv.Env.Load(); 

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables() // includes .env if loaded
    .AddCommandLine(args);

// var port = Environment.GetEnvironmentVariable("API_PORT");
// builder.WebHost.UseUrls($"http://*:{port}");

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddScoped<IPasswordHasher<UserProfile>, PasswordHasher<UserProfile>>();
builder.Services.AddScoped<IPasswordHasher<SFServer.Shared.Server.Admin.Administrator>, PasswordHasher<SFServer.Shared.Server.Admin.Administrator>>();
builder.Services.AddScoped<InventoryService>();
builder.Services.AddSingleton<IAnalyticsService>(sp => new ClickHouseAnalyticsService(builder.Configuration.GetConnectionString("ClickHouse")!));

builder.Services.AddControllers()
    .AddMvcOptions(options =>
    {
        options.InputFormatters.Insert(0, new MemoryPackInputFormatter());
        options.OutputFormatters.Insert(0, new MemoryPackOutputFormatter());
        options.Conventions.Add(new GlobalMessagePackConvention("application/x-memorypack"));
    });


builder.Services.AddDbContext<DatabseContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/var/app-keys"))
    .SetApplicationName("SecretGameBackend");

// Configure JWT authentication.
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT_SECRET"]))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SecretGameBackend API",
        Version = "v1",
        Description = "API documentation for the SecretGameBackend service"
    });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter 'Bearer' [space] and then your valid token in the text input below.\r\n\r\nExample: \"Bearer abc123\"",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };

    options.AddSecurityDefinition("Bearer", securityScheme);

    // Require the token in all operations.
    var securityRequirement = new OpenApiSecurityRequirement
    {
        { securityScheme, Array.Empty<string>() }
    };
    options.AddSecurityRequirement(securityRequirement);
});

var app = builder.Build();

// Enable middleware to serve generated Swagger as a JSON endpoint.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "SecretGameBackend API v1");
        // Optional: set Swagger UI at the root, e.g. options.RoutePrefix = "";
    });
}

// Apply migrations & seed admin
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DatabseContext>();
    context.Database.Migrate(); // apply migrations
    CleanupOrphanProjectData(context);

    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

    var adminUsername = config["ADMIN_USERNAME"];
    var adminEmail = config["ADMIN_EMAIL"];

    if (!context.Administrators.Any())
    {
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<SFServer.Shared.Server.Admin.Administrator>>();

        var adminUser = new SFServer.Shared.Server.Admin.Administrator
        {
            Id = Guid.CreateVersion7(),
            Username = adminUsername,
            Email = adminEmail,
            CreatedAt = DateTime.UtcNow
        };

        adminUser.PasswordHash = hasher.HashPassword(adminUser, config["ADMIN_PASSWORD"] ?? "password");

        context.Administrators.Add(adminUser);
        context.SaveChanges();
        Console.WriteLine($"✅ Root administrator '{adminUsername}' created.");
    }
    else
    {
        Console.WriteLine("ℹ️ Administrators already configured.");
    }

    // Seed projects
    var project = context.Projects.FirstOrDefault();
    if (project == null)
    {
        project = new SFServer.Shared.Server.Project.ProjectInfo
        {
            Id = Guid.CreateVersion7(),
            Name = config["DEFAULT_PROJECT_NAME"] ?? "Default"
        };
        context.Projects.Add(project);
        context.SaveChanges();
        Console.WriteLine("✅ Default project created.");
    }

    // Seed global settings
    if (!context.GlobalSettings.Any())
    {
        var gs = new SFServer.Shared.Server.Settings.GlobalSettings
        {
            Id = Guid.CreateVersion7(),
            ServerTitle = config["SERVER_TITLE"] ?? string.Empty,
            ServerCopyright = config["SERVER_COPYRIGHT"] ?? string.Empty
        };
        context.GlobalSettings.Add(gs);
        context.SaveChanges();
        Console.WriteLine("✅ Global settings created from environment.");
    }

    if (!context.ProjectSettings.Any())
    {
        var ps = new SFServer.Shared.Server.Settings.ProjectSettings
        {
            Id = Guid.CreateVersion7(),
            ProjectId = project.Id,
            ServerTitle = project.Name,
            ServerCopyright = string.Empty,
            GoogleClientId = string.Empty,
            ClickHouseConnection = string.Empty,
            GoogleClientSecret = string.Empty,
            GoogleServiceAccountJson = string.Empty,
            BundleId = string.Empty
        };
        context.ProjectSettings.Add(ps);
        context.SaveChanges();
        Console.WriteLine("✅ Default project settings created.");
    }
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseMiddleware<AuditLogMiddleware>();
app.UseAuthorization();
app.MapControllers();app.Run();

static void CleanupOrphanProjectData(DatabseContext db)
{
    var validIds = db.Projects.Select(p => p.Id).ToHashSet();
    if (validIds.Count == 0)
        return;

    var orphanProfiles = db.UserProfiles.Where(p => !validIds.Contains(p.ProjectId)).ToList();
    db.UserProfiles.RemoveRange(orphanProfiles);

    var orphanIds = orphanProfiles.Select(p => p.Id).ToList();

    var devices = db.UserDevices.Where(d => orphanIds.Contains(d.UserId));
    db.UserDevices.RemoveRange(devices);

    var currencies = db.Currencies.Where(c => !validIds.Contains(c.ProjectId));
    db.Currencies.RemoveRange(currencies);

    var wallets = db.WalletItems.Where(w => orphanIds.Contains(w.UserId));
    db.WalletItems.RemoveRange(wallets);

    var items = db.InventoryItems.Where(i => !validIds.Contains(i.ProjectId));
    db.InventoryItems.RemoveRange(items);

    var playerInv = db.PlayerInventoryItems.Where(pi => orphanIds.Contains(pi.UserId));
    db.PlayerInventoryItems.RemoveRange(playerInv);

    var settings = db.ProjectSettings.Where(s => !validIds.Contains(s.ProjectId));
    db.ProjectSettings.RemoveRange(settings);

    var logs = db.AuditLogs.Where(l => !validIds.Contains(l.ProjectId));
    db.AuditLogs.RemoveRange(logs);

    db.SaveChanges();
}