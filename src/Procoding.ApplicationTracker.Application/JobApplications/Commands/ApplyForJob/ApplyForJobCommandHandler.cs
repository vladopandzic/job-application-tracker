using LanguageExt.Common;
using Procoding.ApplicationTracker.Application.Core.Abstractions.Messaging;
using Procoding.ApplicationTracker.Domain.Abstractions;
using Procoding.ApplicationTracker.Domain.Exceptions;
using Procoding.ApplicationTracker.Domain.Repositories;
using Procoding.ApplicationTracker.Domain.ValueObjects;
using Procoding.ApplicationTracker.DTOs.Model;
using Procoding.ApplicationTracker.DTOs.Response.JobApplications;

namespace Procoding.ApplicationTracker.Application.JobApplications.Commands.ApplyForJob;

internal sealed class ApplyForJobCommandHandler : ICommandHandler<ApplyForJobCommand, JobApplicationInsertedResponseDTO>
{
    private readonly ICompanyRepository _companyRepository;
    private readonly ICandidateRepository _candidateRepository;
    private readonly IJobApplicationSourceRepository _jobApplicationSourceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJobApplicationRepository _jobApplicationRepository;
    private readonly TimeProvider _timeProvider;

    public ApplyForJobCommandHandler(ICompanyRepository companyRepository,
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

    public async Task<Result<JobApplicationInsertedResponseDTO>> Handle(ApplyForJobCommand request, CancellationToken cancellationToken)
    {
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

        var jobApplication = candidate.ApplyForAJob(company: company,
                                                    jobApplicationSource: jobApplicationSource,
                                                    timeProvider: _timeProvider,
                                                    jobPositionTitle: request.JobPositionTitle,
                                                    jobAdLink: new Link(request.JobAdLink),
                                                    workLocationType: new WorkLocationType(request.WorkLocationType),
                                                    jobType: new JobType(request.JobType),
                                                    description: request.Description);

        await _jobApplicationRepository.InsertAsync(jobApplication, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var candidateDto = new CandidateDTO(jobApplication.Candidate.Id,
                                            jobApplication.Candidate.Name,
                                            jobApplication.Candidate.Surname,
                                            jobApplication.Candidate.Email.Value);
        var jobApplicationSourceDto = new JobApplicationSourceDTO(jobApplication.ApplicationSource.Id, jobApplication.ApplicationSource.Name);
        var companyDto = new CompanyDTO(jobApplication.Company.Id, jobApplication.Company.CompanyName.Value, jobApplication.Company.OfficialWebSiteLink.Value);

        var workLocationDto = new WorkLocationTypeDTO(jobApplication.WorkLocationType.Value);
        var jobType = new JobTypeDTO(jobApplication.JobType.Value);
        return new JobApplicationInsertedResponseDTO(new JobApplicationDTO(id: jobApplication.Id,
                                                                           candidate: candidateDto,
                                                                           applicationSource: jobApplicationSourceDto,
                                                                           company: companyDto,
                                                                           jobPositionTitle: jobApplication.JobPositionTitle,
                                                                           jobAdLink: jobApplication.JobAdLink.Value,
                                                                           workLocationType: workLocationDto,
                                                                           jobType: jobType,
                                                                           description: jobApplication.Description,
                                                                           status: jobApplication.JobApplicationStatus.ToString()));
    }
}
