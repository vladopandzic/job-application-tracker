using LanguageExt.Common;
using Procoding.ApplicationTracker.Application.Core.Abstractions.Messaging;

namespace Procoding.ApplicationTracker.Application.JobApplications.Commands.DeleteJobApplication;

public sealed class DeleteJobApplicationCommand : ICommand<Result<Guid>>
{
    public DeleteJobApplicationCommand(Guid jobApplicationId)
    {
        JobApplicationId = jobApplicationId;
    }

    public Guid JobApplicationId { get; }
}
