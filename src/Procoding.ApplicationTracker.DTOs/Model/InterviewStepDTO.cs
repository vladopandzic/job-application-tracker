namespace Procoding.ApplicationTracker.DTOs.Model;

/// <summary>
/// One step in the interview process of a job application.
/// </summary>
public class InterviewStepDTO
{
    public InterviewStepDTO()
    {
    }

    public InterviewStepDTO(Guid id, string type, DateTime occurredOn, string outcome, string? notes)
    {
        Id = id;
        Type = type;
        OccurredOn = occurredOn;
        Outcome = outcome;
        Notes = notes;
    }

    public Guid Id { get; set; }

    /// <summary>Step type (e.g. PhoneScreen, TechnicalInterview).</summary>
    public string Type { get; set; } = "";

    /// <summary>When the step took place or is scheduled.</summary>
    public DateTime OccurredOn { get; set; }

    /// <summary>Outcome (Pending, Passed, Failed).</summary>
    public string Outcome { get; set; } = "";

    /// <summary>Free-text notes / comment.</summary>
    public string? Notes { get; set; }
}
