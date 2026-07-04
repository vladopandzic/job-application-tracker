using LanguageExt.Common;
using Procoding.ApplicationTracker.Application.Core.Abstractions.Messaging;

namespace Procoding.ApplicationTracker.Application.JobApplications.Commands.SetJobApplicationArchived;

public sealed class SetJobApplicationArchivedCommand : ICommand<Result<Guid>>
{
    public SetJobApplicationArchivedCommand(Guid jobApplicationId, bool archived)
    {
        JobApplicationId = jobApplicationId;
        Archived = archived;
    }

    public Guid JobApplicationId { get; }

    /// <summary>True to archive, false to restore (unarchive).</summary>
    public bool Archived { get; }
}
