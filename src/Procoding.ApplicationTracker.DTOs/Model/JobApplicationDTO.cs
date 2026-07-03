namespace Procoding.ApplicationTracker.DTOs.Model;

public class JobApplicationDTO
{
    public JobApplicationDTO()
    {
    }

    public JobApplicationDTO(Guid id,
                             CandidateDTO candidate,
                             JobApplicationSourceDTO applicationSource,
                             CompanyDTO company,
                             string jobPositionTitle,
                             string jobAdLink,
                             WorkLocationTypeDTO workLocationType,
                             JobTypeDTO jobType,
                             string? description,
                             string status = "")
    {
        Id = id;
        Candidate = candidate;
        ApplicationSource = applicationSource;
        Company = company;
        JobPositionTitle = jobPositionTitle;
        JobAdLink = jobAdLink;
        WorkLocationType = workLocationType;
        JobType = jobType;
        Description = description;
        Status = status;
    }

    public Guid Id { get; set; }

    public CandidateDTO Candidate { get; set; }

    public JobApplicationSourceDTO ApplicationSource { get; set; }

    public CompanyDTO Company { get; set; }

    public string JobPositionTitle { get; set; }

    public string JobAdLink { get; set; }

    public WorkLocationTypeDTO WorkLocationType { get; set; }

    public JobTypeDTO JobType { get; set; }

    public string? Description { get; set; }

    /// <summary>
    /// Current status of the job application (e.g. Applied, InProcess, Rejected, Withdrawed).
    /// </summary>
    public string Status { get; set; } = "";

    /// <summary>
    /// Steps in the interview process, ordered chronologically. Populated on the detail view.
    /// </summary>
    public List<InterviewStepDTO> InterviewSteps { get; set; } = new();
}
