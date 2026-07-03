using LanguageExt.Common;
using Procoding.ApplicationTracker.Application.Core.Abstractions.Messaging;
using Procoding.ApplicationTracker.DTOs.Response.JobApplications;

namespace Procoding.ApplicationTracker.Application.JobApplications.Commands.AddInterviewStep;

public sealed class AddInterviewStepCommand : ICommand<Result<InterviewStepAddedResponseDTO>>
{
    public AddInterviewStepCommand(Guid jobApplicationId, string type, DateTime occurredOn, string outcome, string? notes)
    {
        JobApplicationId = jobApplicationId;
        Type = type;
        OccurredOn = occurredOn;
        Outcome = outcome;
        Notes = notes;
    }

    public Guid JobApplicationId { get; }

    public string Type { get; }

    public DateTime OccurredOn { get; }

    public string Outcome { get; }

    public string? Notes { get; }
}
