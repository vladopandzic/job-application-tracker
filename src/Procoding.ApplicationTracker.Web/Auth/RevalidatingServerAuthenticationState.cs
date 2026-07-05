using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Procoding.ApplicationTracker.DTOs.Request.Employees;
using Procoding.ApplicationTracker.Web.Services.Interfaces;

namespace Procoding.ApplicationTracker.Web.Auth;

public class RevalidatingServerAuthenticationState : RevalidatingServerAuthenticationStateProvider
{
    private readonly IAuthService _authService;

    public RevalidatingServerAuthenticationState(ILoggerFactory loggerFactory, IAuthService authService) : base(loggerFactory)
    {
        _authService = authService;
    }

    // The access token now lives as long as the auth cookie (30 days), so it won't expire mid-session and
    // we don't need to churn it. Revalidate rarely — the old 8s interval rotated the refresh token
    // constantly (invalidating the cookie's token), which is exactly what left the board empty after ~1h.
    protected override TimeSpan RevalidationInterval => TimeSpan.FromHours(12);

    protected override async Task<bool> ValidateAuthenticationStateAsync(AuthenticationState authenticationState, CancellationToken cancellationToken)
    {
        var user = authenticationState.User;

        var currentAccessToken = user.Claims.FirstOrDefault(x => x.Type == "access_token")?.Value;
        var currentRefreshToken = user.Claims.FirstOrDefault(x => x.Type == "refresh_token")?.Value;

        // Not a JWT-backed session (or missing tokens) — nothing to refresh, keep the state as-is.
        if (string.IsNullOrEmpty(currentAccessToken) || string.IsNullOrEmpty(currentRefreshToken))
        {
            return true;
        }

        var tokenRequest = new TokenRequestDTO { AccessToken = currentAccessToken, RefreshToken = currentRefreshToken };

        try
        {
            string newAccessToken;
            string newRefreshToken;

            if (user.IsInRole("Employee"))
            {
                var result = await _authService.RefreshLoginTokenForEmployee(tokenRequest, cancellationToken);
                // On a transient refresh failure keep the current (still-valid) tokens rather than tearing
                // the session down — otherwise a single hiccup logs the user out or empties their data.
                if (!result.IsSuccess)
                {
                    return true;
                }
                newAccessToken = result.Value.AccessToken;
                newRefreshToken = result.Value.RefreshToken;
            }
            else
            {
                var result = await _authService.RefreshLoginTokenForCandidate(tokenRequest, cancellationToken);
                if (!result.IsSuccess)
                {
                    return true;
                }
                newAccessToken = result.Value.AccessToken;
                newRefreshToken = result.Value.RefreshToken;
            }

            if (user.Identity is ClaimsIdentity identity)
            {
                // Atomically swap ONLY the token claims so there's never a window with a missing
                // access_token (name/role/etc. don't change on refresh and are left untouched).
                ReplaceClaim(identity, "access_token", newAccessToken);
                ReplaceClaim(identity, "refresh_token", newRefreshToken);

                SetAuthenticationState(Task.FromResult(new AuthenticationState(user)));
            }
        }
        catch
        {
            // Network/serialization hiccup — keep the current session and try again next interval.
        }

        return true;
    }

    private static void ReplaceClaim(ClaimsIdentity identity, string type, string value)
    {
        var existing = identity.FindFirst(type);
        if (existing is not null)
        {
            identity.RemoveClaim(existing);
        }

        identity.AddClaim(new Claim(type, value));
    }
}
