using FluentValidation;
using Procoding.ApplicationTracker.Domain.Entities;

namespace Procoding.ApplicationTracker.Application.JobApplications.Commands.AddInterviewStep;

public sealed class AddInterviewStepCommandValidator : AbstractValidator<AddInterviewStepCommand>
{
    public AddInterviewStepCommandValidator()
    {
        RuleFor(x => x.JobApplicationId).NotEmpty();

        RuleFor(x => x.Type).NotEmpty();

        RuleFor(x => x.Type)
            .Must(x => Enum.TryParse<InterviewStepType>(x, ignoreCase: false, out _))
            .WithMessage("Type is not a valid interview step type.");

        RuleFor(x => x.Outcome)
            .Must(x => Enum.TryParse<InterviewStepOutcome>(x, ignoreCase: false, out _))
            .WithMessage("Outcome is not a valid interview step outcome.");
    }
}
