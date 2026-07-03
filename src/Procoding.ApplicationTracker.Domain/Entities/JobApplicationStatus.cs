namespace Procoding.ApplicationTracker.Domain.Entities;

/// <summary>
/// Represents all possible job application statuses.
/// </summary>
public enum JobApplicationStatus
{
    Applied,
    InProcess,
    Offer,
    Accepted,
    Rejected,
    Withdrawed
}
