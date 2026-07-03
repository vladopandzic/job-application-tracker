using LanguageExt.Common;
using Procoding.ApplicationTracker.Application.Core.Abstractions.Messaging;
using Procoding.ApplicationTracker.Domain.Abstractions;
using Procoding.ApplicationTracker.Domain.Exceptions;
using Procoding.ApplicationTracker.Domain.Repositories;
using Procoding.ApplicationTracker.Domain.ValueObjects;
using Procoding.ApplicationTracker.DTOs.Model;
using Procoding.ApplicationTracker.DTOs.Response.JobApplications;

namespace Procoding.ApplicationTracker.Application.JobApplications.Commands.UpdateJobApplication;

internal sealed class UpdateJobApplicationCommandHandler : ICommandHandler<UpdateJobApplicationCommand, JobApplicationUpdatedResponseDTO>
{
    private readonly ICompanyRepository _companyRepository;
    private readonly ICandidateRepository _candidateRepository;
    private readonly IJobApplicationSourceRepository _jobApplicationSourceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJobApplicationRepository _jobApplicationRepository;
    private readonly TimeProvider _timeProvider;

    public UpdateJobApplicationCommandHandler(ICompanyRepository companyRepository,
                                              ICandidateRepository candidateRepository,
                                              IJobApplicationSourceRepository jobApplicationSourceRepository,
                                              IUnitOfWork unitOfWork,
                                              IJobApplicationRepository jobApplicationRepository,
                                              TimeProvider timeProvider)
    {
        _companyRepository = companyRepository;
        _candidateRepository = candidateRepository;
        _jobApplicationSourceRepository = jobApplicationSourceRepository;
        _unitOfWork = unitOfWork;
        _jobApplicationRepository = jobApplicationRepository;
        _timeProvider = timeProvider;
    }

    public async Task<Result<JobApplicationUpdatedResponseDTO>> Handle(UpdateJobApplicationCommand request, CancellationToken cancellationToken)
    {
        var jobApplication = await _jobApplicationRepository.GetJobApplicationAsync(request.Id, cancellationToken);

        var workLocationType = new WorkLocationType(request.WorkLocationType);
        var jobType = new JobType(request.JobType);
        var jobAdLink = new Link(request.JobAdLink);

        if (jobApplication is null)
        {
            throw new JobApplicationDoesNotExistException("Job application does not exist");
        }

        var company = await _companyRepository.GetCompanyAsync(request.CompanyId, cancellationToken);

        if (company is null)
        {
            throw new CompanyDoesNotExistException("Company does not exist");
        }

        var candidate = await _candidateRepository.GetCandidateAsync(request.CandidateId, cancellationToken);

        if (candidate is null)
        {
            throw new CandidateDoesNotExistException("Candidate does not exist");
        }

        var jobApplicationSource = await _jobApplicationSourceRepository.GetJobApplicationSourceAsync(request.JobApplicationSourceId, cancellationToken);

        if (jobApplicationSource is null)
        {
            throw new JobApplicationSourceDoesNotExistException("Candidate does not exist");
        }

        jobApplication.Update(company: company,
                              jobApplicationSource: jobApplicationSource,
                              candidate: candidate,
                              jobPositionTitle: request.JobPositionTitle,
                              workLocationType: workLocationType,
                              jobType: jobType,
                              jobAdLink: jobAdLink,
                              description: request.Description);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var candidateDto = new CandidateDTO(jobApplication.Candidate.Id,
                                            jobApplication.Candidate.Name,
                                            jobApplication.Candidate.Surname,
                                            jobApplication.Candidate.Email.Value);

        var jobApplicationSourceDto = new JobApplicationSourceDTO(jobApplication.ApplicationSource.Id, jobApplication.ApplicationSource.Name);
        var companyDto = new CompanyDTO(jobApplication.Company.Id, jobApplication.Company.CompanyName.Value, jobApplication.Company.OfficialWebSiteLink.Value);

        var workLocationDto = new WorkLocationTypeDTO(jobApplication.WorkLocationType.Value);
        var jobTypeDto = new JobTypeDTO(jobApplication.JobType.Value);

        return new JobApplicationUpdatedResponseDTO(new JobApplicationDTO(id: jobApplication.Id,
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
