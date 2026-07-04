namespace Procoding.ApplicationTracker.DTOs.Request.JobApplications;

/// <summary>Route binding for endpoints that act on a single job application (archive / unarchive / delete).</summary>
public sealed class JobApplicationIdRequestDTO
{
    public Guid JobApplicationId { get; set; }
}
