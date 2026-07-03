using LanguageExt.Common;
using Procoding.ApplicationTracker.Application.Core.Abstractions.Messaging;
using Procoding.ApplicationTracker.Domain.Abstractions;
using Procoding.ApplicationTracker.Domain.Entities;
using Procoding.ApplicationTracker.Domain.Exceptions;
using Procoding.ApplicationTracker.Domain.Repositories;
using Procoding.ApplicationTracker.DTOs.Model;
using Procoding.ApplicationTracker.DTOs.Response.JobApplications;

namespace Procoding.ApplicationTracker.Application.JobApplications.Commands.AddInterviewStep;

internal sealed class AddInterviewStepCommandHandler : ICommandHandler<AddInterviewStepCommand, InterviewStepAddedResponseDTO>
{
    private readonly IJobApplicationRepository _jobApplicationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddInterviewStepCommandHandler(IJobApplicationRepository jobApplicationRepository, IUnitOfWork unitOfWork)
    {
        _jobApplicationRepository = jobApplicationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<InterviewStepAddedResponseDTO>> Handle(AddInterviewStepCommand request, CancellationToken cancellationToken)
    {
        var jobApplication = await _jobApplicationRepository.GetJobApplicationAsync(request.JobApplicationId, cancellationToken);

        if (jobApplication is null)
        {
            throw new JobApplicationDoesNotExistException("Job application does not exist");
        }

        var type = Enum.Parse<InterviewStepType>(request.Type);
        var outcome = Enum.Parse<InterviewStepOutcome>(request.Outcome);

        var interviewStep = jobApplication.AddInterviewStep(Guid.NewGuid(), type, request.OccurredOn, outcome, request.Notes);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new InterviewStepDTO(interviewStep.Id,
                                       interviewStep.Type.ToString(),
                                       interviewStep.OccurredOn,
                                       interviewStep.Outcome.ToString(),
                                       interviewStep.Notes);

        return new InterviewStepAddedResponseDTO(dto);
    }
}
