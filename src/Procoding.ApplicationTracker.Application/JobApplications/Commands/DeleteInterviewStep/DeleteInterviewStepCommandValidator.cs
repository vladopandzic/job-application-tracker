using FluentValidation;

namespace Procoding.ApplicationTracker.Application.JobApplications.Commands.DeleteInterviewStep;

public sealed class DeleteInterviewStepCommandValidator : AbstractValidator<DeleteInterviewStepCommand>
{
    public DeleteInterviewStepCommandValidator()
    {
        RuleFor(x => x.JobApplicationId).NotEmpty();

        RuleFor(x => x.InterviewStepId).NotEmpty();
    }
}
