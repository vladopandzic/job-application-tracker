using Procoding.ApplicationTracker.Domain.Abstractions;
using Procoding.ApplicationTracker.Domain.Common;

namespace Procoding.ApplicationTracker.Domain.Entities;

/// <summary>
/// Represents one step in the job application / interview process (e.g. a phone screen or a
/// technical interview), with when it happened, its outcome and free-text notes.
/// </summary>
public sealed class InterviewStep : EntityBase, IAuditableEntity, ISoftDeletableEntity
{
    public static readonly int MaxLengthForNotes = 1000;

#pragma warning disable CS8618
    private InterviewStep()
    {
    } //used by EF core
#pragma warning restore CS8618

    /// <summary>
    /// Creates a new interview step.
    /// </summary>
    /// <param name="jobApplication">The job application this step belongs to.</param>
    /// <param name="id">Id of the interview step.</param>
    /// <param name="type">Type of the step.</param>
    /// <param name="occurredOn">When the step took place or is scheduled.</param>
    /// <param name="outcome">Outcome of the step.</param>
    /// <param name="notes">Optional free-text notes / comment.</param>
    public InterviewStep(JobApplication jobApplication,
                         Guid id,
                         InterviewStepType type,
                         DateTime occurredOn,
                         InterviewStepOutcome outcome,
                         string? notes) : base(id)
    {
        if (jobApplication is null)
        {
            throw new ArgumentNullException(nameof(jobApplication));
        }

        if (notes is not null && notes.Length > MaxLengthForNotes)
        {
            throw new ArgumentException($"Notes can not be longer than {MaxLengthForNotes} characters");
        }

        JobApplication = jobApplication;
        Type = type;
        OccurredOn = occurredOn;
        Outcome = outcome;
        Notes = notes;
    }

    /// <summary>
    /// Updates the details of this interview step.
    /// </summary>
    public void Update(InterviewStepType type, DateTime occurredOn, InterviewStepOutcome outcome, string? notes)
    {
        if (notes is not null && notes.Length > MaxLengthForNotes)
        {
            throw new ArgumentException($"Notes can not be longer than {MaxLengthForNotes} characters");
        }

        Type = type;
        OccurredOn = occurredOn;
        Outcome = outcome;
        Notes = notes;
    }

    /// <summary>
    /// Type of the interview step (e.g. phone screen, technical interview).
    /// </summary>
    public InterviewStepType Type { get; private set; }

    /// <summary>
    /// When the step took place or is scheduled.
    /// </summary>
    public DateTime OccurredOn { get; private set; }

    /// <summary>
    /// Outcome of the step.
    /// </summary>
    public InterviewStepOutcome Outcome { get; private set; }

    /// <summary>
    /// Free-text notes / comment about the step.
    /// </summary>
    public string? Notes { get; private set; }

    /// <summary>
    /// Job application this interview step belongs to.
    /// </summary>
    public JobApplication JobApplication { get; private set; }

    /// <inheritdoc/>
    public DateTime? DeletedOnUtc { get; private set; }

    /// <inheritdoc/>
    public DateTime CreatedOnUtc { get; private set; }

    /// <inheritdoc/>
    public DateTime ModifiedOnUtc { get; private set; }
}
