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
builder.Services.AddScoped<SFServer.API.Services.InventoryService>();
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

    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

    var adminUsername = config["ADMIN_USERNAME"];
    var adminEmail = config["ADMIN_EMAIL"];
    var adminRole = Enum.GetName(UserRole.Admin);

    // Only create if admin doesn't exist
    if (!context.UserProfiles.Any(u => u.Email == adminEmail))
    {
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<UserProfile>>();

        var adminUser = new UserProfile
        {
            Id = Guid.CreateVersion7(),
            Username = adminUsername,
            Email = adminEmail,
            Role = Enum.TryParse<UserRole>(adminRole, out var role) ? role : UserRole.Admin,
            CreatedAt = DateTime.UtcNow
        };


        adminUser.PasswordHash = hasher.HashPassword(adminUser, config["ADMIN_PASSWORD"] ?? "password");

        context.UserProfiles.Add(adminUser);

        context.SaveChanges();
        Console.WriteLine($"✅ Admin user '{adminUsername}' created.");
    }
    else
    {
        Console.WriteLine($"ℹ️ Admin user '{adminUsername}' already exists.");
    }

    // Seed server settings from environment variables if not present
    if (!context.ServerSettings.Any())
    {
        var settings = new SFServer.Shared.Server.Settings.ServerSettings
        {
            Id = Guid.NewGuid(),
            ServerCopyright = config["SERVER_COPYRIGHT"] ?? string.Empty,
            GoogleClientId = config["GOOGLE_CLIENT_ID"] ?? string.Empty,
            ClickHouseConnection = config["CLICKHOUSE_CONNECTION"] ?? string.Empty,
            GoogleClientSecret = config["GOOGLE_CLIENT_SECRET"] ?? string.Empty,
            GoogleServiceAccountJson = config["GOOGLE_SERVICE_ACCOUNT_JSON"] ?? string.Empty
        };
        context.ServerSettings.Add(settings);
        context.SaveChanges();
        Console.WriteLine("✅ Server settings created from environment.");
    }
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();