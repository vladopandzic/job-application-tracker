using LanguageExt.Common;
using Procoding.ApplicationTracker.Application.Core.Abstractions.Messaging;
using Procoding.ApplicationTracker.Domain.Abstractions;
using Procoding.ApplicationTracker.Domain.Exceptions;
using Procoding.ApplicationTracker.Domain.Repositories;

namespace Procoding.ApplicationTracker.Application.JobApplications.Commands.SetJobApplicationArchived;

internal sealed class SetJobApplicationArchivedCommandHandler : ICommandHandler<SetJobApplicationArchivedCommand, Guid>
{
    private readonly IJobApplicationRepository _jobApplicationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public SetJobApplicationArchivedCommandHandler(IJobApplicationRepository jobApplicationRepository,
                                                   IUnitOfWork unitOfWork,
                                                   TimeProvider timeProvider)
    {
        _jobApplicationRepository = jobApplicationRepository;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
    }

    public async Task<Result<Guid>> Handle(SetJobApplicationArchivedCommand request, CancellationToken cancellationToken)
    {
        var jobApplication = await _jobApplicationRepository.GetJobApplicationAsync(request.JobApplicationId, cancellationToken);

        if (jobApplication is null)
        {
            throw new JobApplicationDoesNotExistException("Job application does not exist");
        }

        if (request.Archived)
        {
            jobApplication.Archive(_timeProvider);
        }
        else
        {
            jobApplication.Unarchive();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return jobApplication.Id;
    }
}
