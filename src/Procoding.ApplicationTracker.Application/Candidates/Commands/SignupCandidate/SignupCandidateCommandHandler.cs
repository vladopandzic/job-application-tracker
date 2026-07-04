using FluentValidation;
using LanguageExt.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Procoding.ApplicationTracker.Application.Core.Abstractions.Emailing;
using Procoding.ApplicationTracker.Application.Core.Abstractions.Messaging;
using Procoding.ApplicationTracker.Domain.Abstractions;
using Procoding.ApplicationTracker.Domain.Entities;
using Procoding.ApplicationTracker.Domain.Repositories;
using Procoding.ApplicationTracker.Domain.ValueObjects;
using Procoding.ApplicationTracker.DTOs.Response.Candidates;

namespace Procoding.ApplicationTracker.Application.Candidates.Commands.SignupCandidate;

internal sealed class SignupCandidateCommandHandler : ICommandHandler<SignupCandidateCommand, CandidateSignupResponseDTO>
{
    private readonly ICandidateRepository _candidateRepository;
    private readonly IPasswordHasher<Candidate> _passwordHasher;
    private readonly UserManager<Candidate> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailSender _emailSender;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SignupCandidateCommandHandler> _logger;

    public SignupCandidateCommandHandler(ICandidateRepository candidateRepository,
                                         IPasswordHasher<Candidate> passwordHasher,
                                         UserManager<Candidate> userManager,
                                         IUnitOfWork unitOfWork,
                                         IEmailSender emailSender,
                                         IConfiguration configuration,
                                         ILogger<SignupCandidateCommandHandler> logger)
    {
        _candidateRepository = candidateRepository;
        _passwordHasher = passwordHasher;
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _emailSender = emailSender;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Result<CandidateSignupResponseDTO>> Handle(SignupCandidateCommand request, CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid();
        var email = new Email(request.Email);

        var candidate = Candidate.Create(id, name: request.Name, surname: request.Surname, email: email, password: request.Password, _passwordHasher);

        var result = await _candidateRepository.InsertAsync(candidate, request.Password, cancellationToken);

        if (!result.Succeeded)
        {
            return new Result<CandidateSignupResponseDTO>(new ValidationException(result.Errors.Select(x => new FluentValidation.Results.ValidationFailure("",
                                                                                                                                                           x.Description))));
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await SendConfirmationEmailAsync(candidate, cancellationToken);

        return new CandidateSignupResponseDTO(true);
    }

    /// <summary>
    /// Sends the email-confirmation link. Best-effort: a mail failure must not fail the registration
    /// (the account exists, unconfirmed — the user can request a resend later). Will move to an
    /// outbox-driven handler eventually.
    /// </summary>
    private async Task SendConfirmationEmailAsync(Candidate candidate, CancellationToken cancellationToken)
    {
        try
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(candidate);
            var confirmBase = _configuration["EmailConfirmation:ApiConfirmUrl"] ?? "https://jobtrek-api.runasp.net/candidates/confirm-email";
            var confirmUrl = $"{confirmBase}?userId={candidate.Id}&token={Uri.EscapeDataString(token)}";

            var fullName = $"{candidate.Name} {candidate.Surname}".Trim();
            var htmlBody = $"""
                <div style="font-family:Inter,Arial,sans-serif;max-width:520px;margin:auto;color:#1a1a2e">
                    <h2 style="color:#6d28d9">Dobrodošli u JobTrek, {candidate.Name}! 🎉</h2>
                    <p>Još samo jedan korak — potvrdi svoju email adresu klikom na gumb ispod pa se možeš prijaviti.</p>
                    <p style="margin:24px 0">
                        <a href="{confirmUrl}"
                           style="background:#6d28d9;color:#fff;text-decoration:none;padding:12px 24px;border-radius:8px">
                           Potvrdi email
                        </a>
                    </p>
                    <p style="color:#6b7280;font-size:13px">Ako gumb ne radi, kopiraj ovaj link u preglednik:<br>{confirmUrl}</p>
                    <p style="color:#6b7280;font-size:13px;margin-top:32px">Ako se nisi ti registrirao, slobodno zanemari ovaj mail. — JobTrek tim</p>
                </div>
                """;

            var plainText = $"Dobrodošli u JobTrek, {candidate.Name}! Potvrdi email: {confirmUrl}";

            await _emailSender.SendAsync(new EmailMessage(candidate.Email.Value, fullName, "Potvrdi svoj JobTrek račun", htmlBody, plainText),
                                         cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send confirmation email to {Recipient} after signup.", candidate.Email.Value);
        }
    }
}
