using System;
using System.IO;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SFServer.UI;
using SFServer.UI.Filters;
using SFServer.Shared.Server.Project;
using System.Collections.Generic;

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

// var port = Environment.GetEnvironmentVariable("UI_PORT");
// builder.WebHost.UseUrls($"http://*:{port}");

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/var/app-keys"))
    .SetApplicationName("SecretGameBackend");

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

builder.Services.AddAuthorization();

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AuthorizeFilter());
    options.Filters.Add<ProjectContextFilter>();
});
builder.Services.AddRazorPages();

builder.Services.AddHttpClient("api", c =>
{
    c.BaseAddress = new Uri(builder.Configuration["API_BASE_URL"]);
});
builder.Services.AddSingleton<ProjectSettingsService>();
builder.Services.AddSingleton<GlobalSettingsService>();
builder.Services.AddSingleton<ProjectContext>();
builder.Services.AddScoped<ProjectContextFilter>();
builder.Services.AddSingleton<DashboardMetricsService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", async (HttpContext context, IConfiguration config, ProjectContext proj) =>
{
    if (context.User.Identity?.IsAuthenticated != true)
    {
        context.Response.Redirect("/Account/Login");
        return;
    }

    if (proj.CurrentProjectId == Guid.Empty)
    {
        using var client = context.User.CreateApiClient(config);
        var projects = await client.GetFromMessagePackAsync<List<ProjectInfo>>("Projects") ?? new();
        if (projects.Count > 0)
        {
            proj.CurrentProjectId = projects[0].Id;
            proj.CurrentProjectName = projects[0].Name;
        }
    }

    if (proj.CurrentProjectId == Guid.Empty)
    {
        context.Response.Redirect("/Account/Login");
        return;
    }

    context.Response.Redirect($"/{proj.CurrentProjectId}/");
});

app.MapControllerRoute(name: "project", pattern: "{projectId:guid}/{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(name: "default", pattern: "{controller=Account}/{action=Login}/{id?}");
app.MapRazorPages();

app.Run();