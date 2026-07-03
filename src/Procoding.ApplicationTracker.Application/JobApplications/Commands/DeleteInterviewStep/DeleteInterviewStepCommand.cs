using LanguageExt.Common;
using Procoding.ApplicationTracker.Application.Core.Abstractions.Messaging;

namespace Procoding.ApplicationTracker.Application.JobApplications.Commands.DeleteInterviewStep;

public sealed class DeleteInterviewStepCommand : ICommand<Result<Guid>>
{
    public DeleteInterviewStepCommand(Guid jobApplicationId, Guid interviewStepId)
    {
        JobApplicationId = jobApplicationId;
        InterviewStepId = interviewStepId;
    }

    public Guid JobApplicationId { get; }

    public Guid InterviewStepId { get; }
}
