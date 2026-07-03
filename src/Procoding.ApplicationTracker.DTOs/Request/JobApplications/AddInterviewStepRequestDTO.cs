namespace Procoding.ApplicationTracker.DTOs.Request.JobApplications;

/// <summary>
/// Request for adding an interview step to a job application.
/// </summary>
/// <param name="JobApplicationId">Id of the job application.</param>
/// <param name="Type">Step type name (e.g. PhoneScreen, TechnicalInterview).</param>
/// <param name="OccurredOn">When the step took place or is scheduled.</param>
/// <param name="Outcome">Outcome name (Pending, Passed, Failed).</param>
/// <param name="Notes">Optional free-text notes / comment.</param>
public record AddInterviewStepRequestDTO(Guid JobApplicationId, string Type, DateTime OccurredOn, string Outcome, string? Notes);
