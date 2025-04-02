using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFServer.Shared.Models.Auth;
using SFServer.UI.Models.UserProfiles;

namespace SFServer.UI.Controllers;

[AllowAnonymous]
public class AccountController : Controller
{
    private readonly IConfiguration _config;

    public AccountController(IConfiguration config)
    {
        _config = config;
    }

    [HttpGet]
    public IActionResult Login(string returnUrl = null)
    {
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        var httpClient = new HttpClient { BaseAddress = new Uri(_config["API_BASE_URL"]) };

        var response = await httpClient.PostAsJsonAsync("Auth/login", model);
        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", "Invalid credentials");
            return View(model);
        }

        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, loginResponse.Username),
            new(ClaimTypes.Email, loginResponse.Email),
            new(ClaimTypes.Role, loginResponse.Role),
            new("JwtToken", loginResponse.JwtToken),
            new("UserId", loginResponse.UserId.ToString())  // NEW claim for user id
        };


        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties { IsPersistent = true }
        );

        return RedirectToAction("Index", "UserProfiles");
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return RedirectToAction("Login");
    }
    
    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }
}