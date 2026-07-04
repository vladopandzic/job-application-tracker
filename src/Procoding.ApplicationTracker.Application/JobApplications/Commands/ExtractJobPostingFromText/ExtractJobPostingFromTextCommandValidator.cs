using FluentValidation;

namespace Procoding.ApplicationTracker.Application.JobApplications.Commands.ExtractJobPostingFromText;

public sealed class ExtractJobPostingFromTextCommandValidator : AbstractValidator<ExtractJobPostingFromTextCommand>
{
    public ExtractJobPostingFromTextCommandValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty()
            .MinimumLength(30).WithMessage("Zalijepi barem malo više teksta oglasa da AI ima što izvući.");
    }
}
