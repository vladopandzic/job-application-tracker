namespace Procoding.ApplicationTracker.DTOs.Request.JobApplications;

/// <summary>
/// Request to extract structured job-application fields from pasted job posting text.
/// </summary>
public sealed class ExtractJobPostingRequestDTO
{
    public string Content { get; set; } = string.Empty;
}
