using Procoding.ApplicationTracker.DTOs.Model;

namespace Procoding.ApplicationTracker.DTOs.Response.JobApplications;

/// <summary>
/// Response for adding an interview step.
/// </summary>
public sealed class InterviewStepAddedResponseDTO
{
    public InterviewStepAddedResponseDTO(InterviewStepDTO interviewStep)
    {
        InterviewStep = interviewStep;
    }

    public InterviewStepAddedResponseDTO()
    {
    }

    public InterviewStepDTO InterviewStep { get; set; }
}
