using FluentValidation;
using LanguageExt.Common;
using Microsoft.AspNetCore.Identity;
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
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<SignupCandidateCommandHandler> _logger;

    public SignupCandidateCommandHandler(ICandidateRepository candidateRepository,
                                         IPasswordHasher<Candidate> passwordHasher,
                                         IUnitOfWork unitOfWork,
                                         IEmailSender emailSender,
                                         ILogger<SignupCandidateCommandHandler> logger)
    {
        _candidateRepository = candidateRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task<Result<CandidateSignupResponseDTO>> Handle(SignupCandidateCommand request, CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid();
        var email = new Email(request.Email);


        var employee = Candidate.Create(id, name: request.Name, surname: request.Surname, email: email, password: request.Password, _passwordHasher);

        //TODO: prevent duplicates
        var result = await _candidateRepository.InsertAsync(employee, request.Password, cancellationToken);

        if (!result.Succeeded)
        {
            return new Result<CandidateSignupResponseDTO>(new ValidationException(result.Errors.Select(x => new FluentValidation.Results.ValidationFailure("",
                                                                                                                                                           x.Description))));
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await SendWelcomeEmailAsync(request, cancellationToken);

        return new CandidateSignupResponseDTO(true);
    }

    /// <summary>
    /// Sends a welcome email after signup. Deliberately best-effort: a mail failure must not fail the
    /// registration (the account is already saved). Will move to an outbox-driven handler later.
    /// </summary>
    private async Task SendWelcomeEmailAsync(SignupCandidateCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var fullName = $"{request.Name} {request.Surname}".Trim();
            var htmlBody = $"""
                <div style="font-family:Inter,Arial,sans-serif;max-width:520px;margin:auto;color:#1a1a2e">
                    <h2 style="color:#6d28d9">Dobrodošli u JobTrek, {request.Name}! 🎉</h2>
                    <p>Tvoj račun je uspješno kreiran. Od sada možeš pratiti svoje prijave za posao
                       na jednom mjestu — od prijave do ponude.</p>
                    <p style="margin-top:24px">
                        <a href="https://jobtrek.runasp.net/Login"
                           style="background:#6d28d9;color:#fff;text-decoration:none;padding:12px 24px;border-radius:8px">
                           Prijavi se
                        </a>
                    </p>
                    <p style="color:#6b7280;font-size:13px;margin-top:32px">Sretno u potrazi! — JobTrek tim</p>
                </div>
                """;

            var plainText = $"Dobrodošli u JobTrek, {request.Name}! Tvoj račun je kreiran. Prijava: https://jobtrek.runasp.net/Login";

            await _emailSender.SendAsync(new EmailMessage(request.Email, fullName, "Dobrodošli u JobTrek 🎉", htmlBody, plainText),
                                         cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send welcome email to {Recipient} after signup.", request.Email);
        }
    }
}
