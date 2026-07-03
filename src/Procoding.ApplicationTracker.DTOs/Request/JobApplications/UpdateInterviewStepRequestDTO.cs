namespace Procoding.ApplicationTracker.DTOs.Request.JobApplications;

/// <summary>
/// Request for updating an existing interview step.
/// </summary>
public record UpdateInterviewStepRequestDTO(Guid JobApplicationId, Guid InterviewStepId, string Type, DateTime OccurredOn, string Outcome, string? Notes);
