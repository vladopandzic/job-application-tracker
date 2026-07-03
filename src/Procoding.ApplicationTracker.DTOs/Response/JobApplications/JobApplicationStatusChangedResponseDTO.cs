using Procoding.ApplicationTracker.DTOs.Model;

namespace Procoding.ApplicationTracker.DTOs.Response.JobApplications;

/// <summary>
/// Response for a job application status change.
/// </summary>
public sealed class JobApplicationStatusChangedResponseDTO
{
    public JobApplicationStatusChangedResponseDTO(JobApplicationDTO jobApplicationDto)
    {
        JobApplication = jobApplicationDto;
    }

    public JobApplicationStatusChangedResponseDTO()
    {
    }

    /// <summary>
    /// Job application with its updated status.
    /// </summary>
    public JobApplicationDTO JobApplication { get; set; }
}
