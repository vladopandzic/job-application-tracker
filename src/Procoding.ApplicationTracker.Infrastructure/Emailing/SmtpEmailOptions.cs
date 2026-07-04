namespace Procoding.ApplicationTracker.Infrastructure.Emailing;

/// <summary>
/// SMTP settings, bound from the "Email" configuration section. Values come from secrets at deploy time —
/// never hardcoded. Provider-agnostic: works with Brevo, Mailjet, Gmail, etc.
/// </summary>
public sealed class SmtpEmailOptions
{
    public const string SectionName = "Email";

    /// <summary>When false (or Host empty), the sender is a no-op — safe default when not configured.</summary>
    public bool Enabled { get; set; }

    public string Host { get; set; } = string.Empty;

    public int Port { get; set; } = 587;

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string FromEmail { get; set; } = string.Empty;

    public string FromName { get; set; } = "JobTrek";

    /// <summary>STARTTLS on port 587 (typical). Set false to use implicit SSL on connect (port 465).</summary>
    public bool UseStartTls { get; set; } = true;
}
