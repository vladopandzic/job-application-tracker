namespace Procoding.ApplicationTracker.Application.Core.Abstractions.AiExtraction;

/// <summary>
/// Structured result of extracting a job posting. <see cref="JobType"/> is one of
/// FullTime/PartTime/Contract/Temporary/Volunteer; <see cref="WorkLocationType"/> is one of
/// Remote/OnSite/Hybrid (or null when the model can't tell). The user reviews these before saving.
/// </summary>
public sealed record ExtractedJobPosting(
    string CompanyName,
    string PositionTitle,
    string? JobType,
    string? WorkLocationType,
    string Description,
    string? CompanyWebsite);
