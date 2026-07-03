namespace Procoding.ApplicationTracker.DTOs.Request.JobApplications;

/// <summary>
/// Request for changing the status of a job application.
/// </summary>
/// <param name="JobApplicationId">Id of the job application whose status should change.</param>
/// <param name="Status">Target status name (e.g. Applied, InProcess, Rejected, Withdrawed).</param>
public record JobApplicationChangeStatusRequestDTO(Guid JobApplicationId, string Status);
