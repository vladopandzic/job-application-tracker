namespace Procoding.ApplicationTracker.Application.Core.Abstractions.Emailing;

/// <summary>
/// A single outgoing email. <see cref="HtmlBody"/> is the rich content; <see cref="PlainTextBody"/> is an
/// optional text fallback for clients that don't render HTML.
/// </summary>
public sealed record EmailMessage(
    string ToEmail,
    string ToName,
    string Subject,
    string HtmlBody,
    string? PlainTextBody = null);
