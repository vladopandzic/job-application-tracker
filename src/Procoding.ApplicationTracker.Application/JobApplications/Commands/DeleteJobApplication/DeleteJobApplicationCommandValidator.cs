using FluentValidation;

namespace Procoding.ApplicationTracker.Application.JobApplications.Commands.DeleteJobApplication;

public sealed class DeleteJobApplicationCommandValidator : AbstractValidator<DeleteJobApplicationCommand>
{
    public DeleteJobApplicationCommandValidator()
    {
        RuleFor(x => x.JobApplicationId).NotEmpty();
    }
}
