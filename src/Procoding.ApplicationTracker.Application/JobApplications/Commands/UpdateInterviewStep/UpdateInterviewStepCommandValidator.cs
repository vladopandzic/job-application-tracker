using FluentValidation;
using Procoding.ApplicationTracker.Domain.Entities;

namespace Procoding.ApplicationTracker.Application.JobApplications.Commands.UpdateInterviewStep;

public sealed class UpdateInterviewStepCommandValidator : AbstractValidator<UpdateInterviewStepCommand>
{
    public UpdateInterviewStepCommandValidator()
    {
        RuleFor(x => x.JobApplicationId).NotEmpty();

        RuleFor(x => x.InterviewStepId).NotEmpty();

        RuleFor(x => x.Type)
            .Must(x => Enum.TryParse<InterviewStepType>(x, ignoreCase: false, out _))
            .WithMessage("Type is not a valid interview step type.");

        RuleFor(x => x.Outcome)
            .Must(x => Enum.TryParse<InterviewStepOutcome>(x, ignoreCase: false, out _))
            .WithMessage("Outcome is not a valid interview step outcome.");
    }
}
