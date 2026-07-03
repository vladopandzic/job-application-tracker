using LanguageExt.Common;
using Procoding.ApplicationTracker.Application.Core.Abstractions.Messaging;
using Procoding.ApplicationTracker.DTOs.Model;

namespace Procoding.ApplicationTracker.Application.JobApplications.Commands.UpdateInterviewStep;

public sealed class UpdateInterviewStepCommand : ICommand<Result<InterviewStepDTO>>
{
    public UpdateInterviewStepCommand(Guid jobApplicationId, Guid interviewStepId, string type, DateTime occurredOn, string outcome, string? notes)
    {
        JobApplicationId = jobApplicationId;
        InterviewStepId = interviewStepId;
        Type = type;
        OccurredOn = occurredOn;
        Outcome = outcome;
        Notes = notes;
    }

    public Guid JobApplicationId { get; }

    public Guid InterviewStepId { get; }

    public string Type { get; }

    public DateTime OccurredOn { get; }

    public string Outcome { get; }

    public string? Notes { get; }
}
