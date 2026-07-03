namespace Procoding.ApplicationTracker.DTOs.Request.JobApplications;

/// <summary>
/// Request for deleting an interview step from a job application.
/// </summary>
/// <param name="JobApplicationId">Id of the job application.</param>
/// <param name="InterviewStepId">Id of the interview step to delete.</param>
public record DeleteInterviewStepRequestDTO(Guid JobApplicationId, Guid InterviewStepId);
