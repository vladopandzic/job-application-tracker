namespace Procoding.ApplicationTracker.DTOs.Request.Candidates;

/// <summary>
/// Query parameters for the email-confirmation link sent after signup.
/// </summary>
public sealed class CandidateConfirmEmailRequestDTO
{
    public Guid UserId { get; set; }

    public string Token { get; set; } = string.Empty;
}
