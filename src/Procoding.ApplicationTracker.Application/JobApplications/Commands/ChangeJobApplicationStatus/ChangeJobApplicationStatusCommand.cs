using LanguageExt.Common;
using Procoding.ApplicationTracker.Application.Core.Abstractions.Messaging;
using Procoding.ApplicationTracker.DTOs.Response.JobApplications;

namespace Procoding.ApplicationTracker.Application.JobApplications.Commands.ChangeJobApplicationStatus;

public sealed class ChangeJobApplicationStatusCommand : ICommand<Result<JobApplicationStatusChangedResponseDTO>>
{
    public ChangeJobApplicationStatusCommand(Guid jobApplicationId, string newStatus)
    {
        JobApplicationId = jobApplicationId;
        NewStatus = newStatus;
    }

    public Guid JobApplicationId { get; }

    public string NewStatus { get; }
}
