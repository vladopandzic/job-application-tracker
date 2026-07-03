using LanguageExt.Common;
using Procoding.ApplicationTracker.Application.Core.Abstractions.Messaging;
using Procoding.ApplicationTracker.Domain.Abstractions;
using Procoding.ApplicationTracker.Domain.Entities;
using Procoding.ApplicationTracker.Domain.Exceptions;
using Procoding.ApplicationTracker.Domain.Repositories;
using Procoding.ApplicationTracker.DTOs.Model;

namespace Procoding.ApplicationTracker.Application.JobApplications.Commands.UpdateInterviewStep;

internal sealed class UpdateInterviewStepCommandHandler : ICommandHandler<UpdateInterviewStepCommand, InterviewStepDTO>
{
    private readonly IJobApplicationRepository _jobApplicationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateInterviewStepCommandHandler(IJobApplicationRepository jobApplicationRepository, IUnitOfWork unitOfWork)
    {
        _jobApplicationRepository = jobApplicationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<InterviewStepDTO>> Handle(UpdateInterviewStepCommand request, CancellationToken cancellationToken)
    {
        var jobApplication = await _jobApplicationRepository.GetJobApplicationAsync(request.JobApplicationId, cancellationToken);

        if (jobApplication is null)
        {
            throw new JobApplicationDoesNotExistException("Job application does not exist");
        }

        var type = Enum.Parse<InterviewStepType>(request.Type);
        var outcome = Enum.Parse<InterviewStepOutcome>(request.Outcome);

        jobApplication.UpdateInterviewStep(request.InterviewStepId, type, request.OccurredOn, outcome, request.Notes);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var step = jobApplication.InterviewSteps.First(x => x.Id == request.InterviewStepId);

        return new InterviewStepDTO(step.Id, step.Type.ToString(), step.OccurredOn, step.Outcome.ToString(), step.Notes);
    }
}
