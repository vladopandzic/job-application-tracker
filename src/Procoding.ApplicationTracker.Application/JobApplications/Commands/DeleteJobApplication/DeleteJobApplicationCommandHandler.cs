using LanguageExt.Common;
using Procoding.ApplicationTracker.Application.Core.Abstractions.Messaging;
using Procoding.ApplicationTracker.Domain.Abstractions;
using Procoding.ApplicationTracker.Domain.Exceptions;
using Procoding.ApplicationTracker.Domain.Repositories;

namespace Procoding.ApplicationTracker.Application.JobApplications.Commands.DeleteJobApplication;

internal sealed class DeleteJobApplicationCommandHandler : ICommandHandler<DeleteJobApplicationCommand, Guid>
{
    private readonly IJobApplicationRepository _jobApplicationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteJobApplicationCommandHandler(IJobApplicationRepository jobApplicationRepository, IUnitOfWork unitOfWork)
    {
        _jobApplicationRepository = jobApplicationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(DeleteJobApplicationCommand request, CancellationToken cancellationToken)
    {
        var jobApplication = await _jobApplicationRepository.GetJobApplicationAsync(request.JobApplicationId, cancellationToken);

        if (jobApplication is null)
        {
            throw new JobApplicationDoesNotExistException("Job application does not exist");
        }

        _jobApplicationRepository.Delete(jobApplication);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return jobApplication.Id;
    }
}
