using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Procoding.ApplicationTracker.DTOs.Request.Candidates;
using Procoding.ApplicationTracker.Web.Services.Interfaces;

namespace Procoding.ApplicationTracker.Web.Controllers;

public class SignupController : Controller
{
    private readonly IAuthService _authService;

    public SignupController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet]

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Signup(CandidateSignupRequestDTO model, CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            var result = await _authService.SignupCandidate(model, cancellationToken);
            if (result.IsSuccess)
            {
                // Account created but not yet usable — the user must confirm their email first.
                return Redirect("/Login?confirmEmail=1");
            }

            ModelState.AddModelError(string.Empty, string.Join(Environment.NewLine, result.Errors.Select(x => x.Message)));
        }
        return View("Index", model);
    }
}
