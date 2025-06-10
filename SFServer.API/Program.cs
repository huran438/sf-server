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
using SFServer.Shared.Server.UserProfile;
using SFServer.API.Services;
using Amazon.S3;
using Amazon.Extensions.NETCore.Setup;

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
builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
builder.Services.AddAWSService<IAmazonS3>();
builder.Services.AddSingleton<S3Service>();

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
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();