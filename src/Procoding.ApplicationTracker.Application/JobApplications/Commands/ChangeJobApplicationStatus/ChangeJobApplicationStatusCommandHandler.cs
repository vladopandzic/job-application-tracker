using LanguageExt.Common;
using Procoding.ApplicationTracker.Application.Core.Abstractions.Messaging;
using Procoding.ApplicationTracker.Domain.Abstractions;
using Procoding.ApplicationTracker.Domain.Entities;
using Procoding.ApplicationTracker.Domain.Exceptions;
using Procoding.ApplicationTracker.Domain.Repositories;
using Procoding.ApplicationTracker.DTOs.Model;
using Procoding.ApplicationTracker.DTOs.Response.JobApplications;

namespace Procoding.ApplicationTracker.Application.JobApplications.Commands.ChangeJobApplicationStatus;

internal sealed class ChangeJobApplicationStatusCommandHandler : ICommandHandler<ChangeJobApplicationStatusCommand, JobApplicationStatusChangedResponseDTO>
{
    private readonly IJobApplicationRepository _jobApplicationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ChangeJobApplicationStatusCommandHandler(IJobApplicationRepository jobApplicationRepository, IUnitOfWork unitOfWork)
    {
        _jobApplicationRepository = jobApplicationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<JobApplicationStatusChangedResponseDTO>> Handle(ChangeJobApplicationStatusCommand request, CancellationToken cancellationToken)
    {
        var jobApplication = await _jobApplicationRepository.GetJobApplicationAsync(request.JobApplicationId, cancellationToken);

        if (jobApplication is null)
        {
            throw new JobApplicationDoesNotExistException("Job application does not exist");
        }

        var newStatus = Enum.Parse<JobApplicationStatus>(request.NewStatus);

        jobApplication.ChangeStatus(newStatus);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var candidateDto = new CandidateDTO(jobApplication.Candidate.Id,
                                            jobApplication.Candidate.Name,
                                            jobApplication.Candidate.Surname,
                                            jobApplication.Candidate.Email.Value);
        var jobApplicationSourceDto = new JobApplicationSourceDTO(jobApplication.ApplicationSource.Id, jobApplication.ApplicationSource.Name);
        var companyDto = new CompanyDTO(jobApplication.Company.Id, jobApplication.Company.CompanyName.Value, jobApplication.Company.OfficialWebSiteLink.Value);
        var workLocationDto = new WorkLocationTypeDTO(jobApplication.WorkLocationType.Value);
        var jobTypeDto = new JobTypeDTO(jobApplication.JobType.Value);

        return new JobApplicationStatusChangedResponseDTO(new JobApplicationDTO(id: jobApplication.Id,
                                                                                candidate: candidateDto,
                                                                                applicationSource: jobApplicationSourceDto,
                                                                                company: companyDto,
                                                                                jobPositionTitle: jobApplication.JobPositionTitle,
                                                                                jobAdLink: jobApplication.JobAdLink.Value,
                                                                                workLocationType: workLocationDto,
                                                                                jobType: jobTypeDto,
                                                                                description: jobApplication.Description,
                                                                                status: jobApplication.JobApplicationStatus.ToString()));
    }
}
