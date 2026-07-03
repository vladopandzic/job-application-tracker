using LanguageExt.Common;
using Procoding.ApplicationTracker.Application.Core.Abstractions.Messaging;
using Procoding.ApplicationTracker.Domain.Abstractions;
using Procoding.ApplicationTracker.Domain.Exceptions;
using Procoding.ApplicationTracker.Domain.Repositories;

namespace Procoding.ApplicationTracker.Application.JobApplications.Commands.DeleteInterviewStep;

internal sealed class DeleteInterviewStepCommandHandler : ICommandHandler<DeleteInterviewStepCommand, Guid>
{
    private readonly IJobApplicationRepository _jobApplicationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteInterviewStepCommandHandler(IJobApplicationRepository jobApplicationRepository, IUnitOfWork unitOfWork)
    {
        _jobApplicationRepository = jobApplicationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(DeleteInterviewStepCommand request, CancellationToken cancellationToken)
    {
        var jobApplication = await _jobApplicationRepository.GetJobApplicationAsync(request.JobApplicationId, cancellationToken);

        if (jobApplication is null)
        {
            throw new JobApplicationDoesNotExistException("Job application does not exist");
        }

        jobApplication.RemoveInterviewStep(request.InterviewStepId);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return request.InterviewStepId;
    }
}
