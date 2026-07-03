using Procoding.ApplicationTracker.Application.Core.Abstractions.Messaging;
using Procoding.ApplicationTracker.Domain.Repositories;
using Procoding.ApplicationTracker.DTOs.Model;
using Procoding.ApplicationTracker.DTOs.Response.JobApplications;

namespace Procoding.ApplicationTracker.Application.JobApplications.Queries.GetOneJobApplication;

internal sealed class GetOneJobApplicationQueryHandler : IQueryHandler<GetOneJobApplicationQuery, JobApplicationResponseDTO>
{
    private readonly IJobApplicationRepository _jobApplicationRepository;

    public GetOneJobApplicationQueryHandler(IJobApplicationRepository jobApplicationRepository)
    {
        _jobApplicationRepository = jobApplicationRepository;
    }

    public async Task<JobApplicationResponseDTO> Handle(GetOneJobApplicationQuery request, CancellationToken cancellationToken)
    {
        var jobApplication = await _jobApplicationRepository.GetJobApplicationAsync(request.Id, cancellationToken);

        if (jobApplication is null)
        {
            throw new Domain.Exceptions.JobApplicationDoesNotExistException();
        }
        var candidateDto = new CandidateDTO(jobApplication.Candidate.Id,
                                            jobApplication.Candidate.Name,
                                            jobApplication.Candidate.Surname,
                                            jobApplication.Candidate.Email.Value);
        var jobApplicationSourceDto = new JobApplicationSourceDTO(jobApplication.ApplicationSource.Id, jobApplication.ApplicationSource.Name);
        var companyDto = new CompanyDTO(jobApplication.Company.Id, jobApplication.Company.CompanyName.Value, jobApplication.Company.OfficialWebSiteLink.Value);

        var workLocationDto = new WorkLocationTypeDTO(jobApplication.WorkLocationType.Value);
        var jobType = new JobTypeDTO(jobApplication.JobType.Value);

        var jobApplicationDto = new JobApplicationDTO(id: jobApplication.Id,
                                                      candidate: candidateDto,
                                                      applicationSource: jobApplicationSourceDto,
                                                      company: companyDto,
                                                      jobPositionTitle: jobApplication.JobPositionTitle,
                                                      jobAdLink: jobApplication.JobAdLink.Value,
                                                      workLocationType: workLocationDto,
                                                      jobType: jobType,
                                                      description: jobApplication.Description,
                                                      status: jobApplication.JobApplicationStatus.ToString());

        jobApplicationDto.InterviewSteps = jobApplication.InterviewSteps
            .Where(s => s.DeletedOnUtc == null)
            .OrderBy(s => s.OccurredOn)
            .Select(s => new InterviewStepDTO(s.Id, s.Type.ToString(), s.OccurredOn, s.Outcome.ToString(), s.Notes))
            .ToList();

        return new JobApplicationResponseDTO(jobApplicationDto);
    }
}
