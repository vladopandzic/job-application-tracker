namespace Procoding.ApplicationTracker.Application.Core.Abstractions.Emailing;

/// <summary>
/// Sends transactional emails. Implemented in Infrastructure (SMTP), consumed by command handlers.
/// The concrete sender is provider-agnostic — any SMTP relay (Brevo, Mailjet, Gmail, ...) works via config.
/// </summary>
public interface IEmailSender
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}
