namespace Procoding.ApplicationTracker.DTOs.Response.JobApplications;

/// <summary>
/// AI-extracted job posting fields, used to pre-fill the new-application form. The user reviews and edits
/// before saving. Enum-like fields may be null when the model can't determine them.
/// </summary>
public sealed class ExtractedJobPostingResponseDTO
{
    public string CompanyName { get; set; } = string.Empty;

    public string PositionTitle { get; set; } = string.Empty;

    public string? JobType { get; set; }

    public string? WorkLocationType { get; set; }

    public string Description { get; set; } = string.Empty;

    public string? CompanyWebsite { get; set; }

    public string? JobAdLink { get; set; }
}
