using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Procoding.ApplicationTracker.DTOs.Request.Candidates;
using Procoding.ApplicationTracker.DTOs.Request.Employees;
using Procoding.ApplicationTracker.Web.Auth;
using Procoding.ApplicationTracker.Web.Services.Interfaces;
using System.Security.Claims;

namespace Procoding.ApplicationTracker.Web.Controllers;

public class LoginController : Controller
{
    readonly IAuthService _authService;
    public LoginController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }


    [HttpGet]
    [Route("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return Redirect("/login");

    }

    [HttpPost]
    public async Task<IActionResult> Login(CandidateLoginRequestDTO model, CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            var result = await _authService.LoginCandidate(model, cancellationToken);
            if (result.IsSuccess)
            {
                var claims = ClaimsCreator.GetClaimsFromToken(result.Value.AccessToken, result.Value.RefreshToken);

                var identity = new ClaimsIdentity(claims, authenticationType: "CookieAuth");
                var user = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(user);
                return Redirect("/home");
            }
            // Surface the API's message (e.g. "email not confirmed") instead of a generic one.
            var message = result.Errors.FirstOrDefault()?.Message;
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(message) ? "Invalid login attempt." : message);
        }
        return View("Index", model);
    }
}
