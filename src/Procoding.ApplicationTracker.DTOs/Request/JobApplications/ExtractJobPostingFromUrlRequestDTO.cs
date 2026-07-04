namespace Procoding.ApplicationTracker.DTOs.Request.JobApplications;

/// <summary>Request to extract job-application fields from a job posting URL.</summary>
public sealed class ExtractJobPostingFromUrlRequestDTO
{
    public string Url { get; set; } = string.Empty;
}
