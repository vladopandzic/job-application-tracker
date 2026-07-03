using FluentValidation;
using Procoding.ApplicationTracker.Domain.Entities;

namespace Procoding.ApplicationTracker.Application.JobApplications.Commands.ChangeJobApplicationStatus;

public sealed class ChangeJobApplicationStatusCommandValidator : AbstractValidator<ChangeJobApplicationStatusCommand>
{
    public ChangeJobApplicationStatusCommandValidator()
    {
        RuleFor(x => x.JobApplicationId).NotEmpty();

        RuleFor(x => x.NewStatus).NotEmpty();

        RuleFor(x => x.NewStatus)
            .Must(x => Enum.TryParse<JobApplicationStatus>(x, ignoreCase: false, out _))
            .WithMessage("Status is not a valid job application status.");
    }
}
