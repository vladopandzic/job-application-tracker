using Procoding.ApplicationTracker.Domain.Entities;

namespace Procoding.ApplicationTracker.Domain.Events;

/// <summary>
/// Raised when a <see cref="JobApplication"/> transitions from one status to another.
/// </summary>
public sealed class JobApplicationStatusChangedDomainEvent : IDomainEvent
{
    public JobApplicationStatusChangedDomainEvent(Guid jobApplicationId,
                                                  JobApplicationStatus oldStatus,
                                                  JobApplicationStatus newStatus)
    {
        JobApplicationId = jobApplicationId;
        OldStatus = oldStatus;
        NewStatus = newStatus;
    }

    /// <summary>
    /// Id of the job application whose status changed.
    /// </summary>
    public Guid JobApplicationId { get; }

    /// <summary>
    /// Status before the change.
    /// </summary>
    public JobApplicationStatus OldStatus { get; }

    /// <summary>
    /// Status after the change.
    /// </summary>
    public JobApplicationStatus NewStatus { get; }
}
