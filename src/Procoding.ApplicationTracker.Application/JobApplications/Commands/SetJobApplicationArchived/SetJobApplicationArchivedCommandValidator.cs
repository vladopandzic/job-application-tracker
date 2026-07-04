using FluentValidation;

namespace Procoding.ApplicationTracker.Application.JobApplications.Commands.SetJobApplicationArchived;

public sealed class SetJobApplicationArchivedCommandValidator : AbstractValidator<SetJobApplicationArchivedCommand>
{
    public SetJobApplicationArchivedCommandValidator()
    {
        RuleFor(x => x.JobApplicationId).NotEmpty();
    }
}
