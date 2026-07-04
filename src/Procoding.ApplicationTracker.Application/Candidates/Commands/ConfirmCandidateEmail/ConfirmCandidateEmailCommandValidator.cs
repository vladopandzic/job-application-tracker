using FluentValidation;

namespace Procoding.ApplicationTracker.Application.Candidates.Commands.ConfirmCandidateEmail;

public sealed class ConfirmCandidateEmailCommandValidator : AbstractValidator<ConfirmCandidateEmailCommand>
{
    public ConfirmCandidateEmailCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();

        RuleFor(x => x.Token).NotEmpty();
    }
}
