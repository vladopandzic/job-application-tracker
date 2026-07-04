using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Procoding.ApplicationTracker.DTOs.Request.Employees;
using Procoding.ApplicationTracker.Web.Auth;
using Procoding.ApplicationTracker.Web.Services.Interfaces;
using System.Security.Claims;

namespace Procoding.ApplicationTracker.Web.Controllers;

public class AdminController : Controller
{
    readonly IAuthService _authService;
    public AdminController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet]
    [Route("admin/login")]
    public IActionResult Login()
    {
        return View();
    }


    [HttpGet]
    [Route("admin/logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return Redirect("/admin/login");

    }

    [HttpPost]
    [Route("admin/login")]
    public async Task<IActionResult> Login(EmployeeLoginRequestDTO model, CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            var result = await _authService.LoginEmployee(model, cancellationToken);
            if (result.IsSuccess)
            {
                var claims = ClaimsCreator.GetClaimsFromToken(result.Value.AccessToken, result.Value.RefreshToken);

                var identity = new ClaimsIdentity(claims, "AdminCookieAuth");
                var user = new ClaimsPrincipal(identity);


                var authenticationProperties = new AuthenticationProperties();
                authenticationProperties.StoreTokens([new AuthenticationToken() { Name = "access_token", Value = result.Value.AccessToken },
                                                      new AuthenticationToken() { Name = "refresh_token", Value = result.Value.RefreshToken }]);

                await HttpContext.SignInAsync(user, new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30)
                });


                // Land the admin inside the app shell (authenticated home), not the public landing page.
                return Redirect("/home");
            }
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        }
        return View("Login", model);
    }
}
