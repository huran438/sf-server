using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SFServer.Shared.Server.Auth;
using SFServer.UI.Models.UserProfiles;

namespace SFServer.UI.Controllers
{
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
            return View(new LoginViewModel { ReturnUrl = returnUrl ?? string.Empty });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            var httpClient = User.CreateApiClient(_config);

            try
            {
                var loginResponse = await httpClient.PostAsMessagePackAsync<LoginDashboardRequest, DashboardLoginResponse>("Auth/login-dashboard", new LoginDashboardRequest
                {
                    Credential = model.Credential,
                    Password = model.Password
                });

                var claims = new List<Claim>
                {
                    new(ClaimTypes.Name, loginResponse.Username),
                    new(ClaimTypes.Role, loginResponse.Role.ToString()),
                    new("JwtToken", loginResponse.JwtToken),
                    new("UserId", loginResponse.UserId.ToString())
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
            catch (HttpRequestException)
            {
                ModelState.AddModelError("", "Invalid credentials or server error.");
                return View(model);
            }
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
}