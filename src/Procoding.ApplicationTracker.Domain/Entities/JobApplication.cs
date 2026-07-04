using Procoding.ApplicationTracker.Domain.Abstractions;
using Procoding.ApplicationTracker.Domain.Common;
using Procoding.ApplicationTracker.Domain.Events;
using Procoding.ApplicationTracker.Domain.ValueObjects;

namespace Procoding.ApplicationTracker.Domain.Entities;

/// <summary>
/// Represents job application.
/// </summary>
public sealed class JobApplication : AggregateRoot, ISoftDeletableEntity, IAuditableEntity
{
    private readonly List<InterviewStep> _interviewSteps = new();

    /// <summary>
    /// Creates new instance of <see cref="JobApplication"/>. Required by EF Core.
    /// </summary>
#pragma warning disable CS8618
    private JobApplication()
    {
    }
#pragma warning restore CS8618

    /// <summary>
    /// Creates new instance of <see cref="JobApplication"/>.
    /// </summary>
    /// <param name="candidate">Candidate for this job application.</param>
    /// <param name="id">Id of the job application.</param>
    /// <param name="appliedOnUTC">When the candidate applied.</param>
    /// <param name="applicationSource">Source through candidate applied like LinkedIn, etc.</param>
    /// <param name="company">Company that the candidate applies for.</param>
    /// <param name="jobApplicationStatus">Job application status.</param>
    private JobApplication(Candidate candidate,
                           Guid id,
                           DateTime appliedOnUTC,
                           JobApplicationSource applicationSource,
                           Company company,
                           JobApplicationStatus jobApplicationStatus,
                           JobType jobType,
                           WorkLocationType workLocationType,
                           string jobPositionTitle,
                           Link jobAdLink,
                           string description) : base(id)
    {
        Candidate = candidate;
        AppliedOnUTC = appliedOnUTC;
        ApplicationSource = applicationSource;
        Company = company;
        JobApplicationStatus = jobApplicationStatus;
        JobType = jobType;
        WorkLocationType = workLocationType;
        JobPositionTitle = jobPositionTitle;
        Description = description;
        JobAdLink = jobAdLink;
    }

    /// <summary>
    /// Creates new job application.
    /// </summary>
    /// <param name="candidate"></param>
    /// <param name="id"></param>
    /// <param name="jobApplicationSource"></param>
    /// <param name="company"></param>
    /// <param name="timeProvider"></param>
    /// <returns></returns>
    public static JobApplication Create(Candidate candidate,
                                        Guid id,
                                        JobApplicationSource jobApplicationSource,
                                        Company company,
                                        string jobPositionTitle,
                                        Link jobAdLink,
                                        JobType jobType,
                                        WorkLocationType workLocationType,
                                        string? description,
                                        TimeProvider timeProvider)
    {
        var newJobApplication = new JobApplication(candidate: candidate,
                                                   id: id,
                                                   appliedOnUTC: timeProvider.GetUtcNow().DateTime,
                                                   applicationSource: jobApplicationSource,
                                                   company: company,
                                                   jobPositionTitle: jobPositionTitle,
                                                   jobAdLink: jobAdLink,
                                                   jobType: jobType,
                                                   workLocationType: workLocationType,
                                                   description: description,
                                                   jobApplicationStatus: JobApplicationStatus.Applied);

        newJobApplication.AddDomainEvent(new AppliedForAJobDomainEvent(newJobApplication));

        return newJobApplication;
    }

    /// <summary>
    /// Changes the status of the job application. Any status can move to any other status (the board
    /// lets the candidate freely re-organize their applications). Changing to the current status is a no-op.
    /// </summary>
    /// <param name="newStatus">The status to move to.</param>
    /// <returns>This job application.</returns>
    public JobApplication ChangeStatus(JobApplicationStatus newStatus)
    {
        if (JobApplicationStatus == newStatus)
        {
            return this;
        }

        var oldStatus = JobApplicationStatus;
        JobApplicationStatus = newStatus;

        AddDomainEvent(new JobApplicationStatusChangedDomainEvent(Id, oldStatus, newStatus));

        return this;
    }

    /// <summary>Archives the application — keeps it but hides it from the active board/list.</summary>
    public JobApplication Archive(TimeProvider timeProvider)
    {
        ArchivedOnUtc ??= timeProvider.GetUtcNow().UtcDateTime;
        return this;
    }

    /// <summary>Restores an archived application back to the active board/list.</summary>
    public JobApplication Unarchive()
    {
        ArchivedOnUtc = null;
        return this;
    }

    /// <summary>
    /// Adds a new step to the interview process of this job application (e.g. a phone screen or a
    /// technical interview) with its outcome and optional notes.
    /// </summary>
    /// <param name="id">Id of the interview step.</param>
    /// <param name="type">Type of the step.</param>
    /// <param name="occurredOn">When the step took place or is scheduled.</param>
    /// <param name="outcome">Outcome of the step.</param>
    /// <param name="notes">Optional free-text notes / comment.</param>
    /// <returns>The created interview step.</returns>
    public InterviewStep AddInterviewStep(Guid id,
                                          InterviewStepType type,
                                          DateTime occurredOn,
                                          InterviewStepOutcome outcome,
                                          string? notes)
    {
        var interviewStep = new InterviewStep(jobApplication: this, id: id, type: type, occurredOn: occurredOn, outcome: outcome, notes: notes);

        _interviewSteps.Add(interviewStep);

        AddDomainEvent(new NewInterviewAddedDomainEvent(interviewStep));

        return interviewStep;
    }

    /// <summary>
    /// Updates an existing interview step. No-op if the step does not exist.
    /// </summary>
    public void UpdateInterviewStep(Guid interviewStepId,
                                    InterviewStepType type,
                                    DateTime occurredOn,
                                    InterviewStepOutcome outcome,
                                    string? notes)
    {
        var interviewStep = _interviewSteps.FirstOrDefault(x => x.Id == interviewStepId);

        interviewStep?.Update(type, occurredOn, outcome, notes);
    }

    /// <summary>
    /// Removes an interview step from this job application. No-op if the step does not exist.
    /// </summary>
    /// <param name="interviewStepId">Id of the interview step to remove.</param>
    public void RemoveInterviewStep(Guid interviewStepId)
    {
        var interviewStep = _interviewSteps.FirstOrDefault(x => x.Id == interviewStepId);

        if (interviewStep is not null)
        {
            _interviewSteps.Remove(interviewStep);
        }
    }

    public JobApplication Update(Company company,
                                 JobApplicationSource jobApplicationSource,
                                 Candidate candidate,
                                 string jobPositionTitle,
                                 WorkLocationType workLocationType,
                                 JobType jobType,
                                 Link jobAdLink,
                                 string? description)
    {
        Company = company;
        ApplicationSource = jobApplicationSource;
        Candidate = candidate;
        JobPositionTitle = jobPositionTitle;
        JobType = jobType;
        WorkLocationType = workLocationType;
        JobAdLink = jobAdLink;
        Description = description;
        this.AddDomainEvent(new JobApplicationUpdatedDomainEvent(id: Id,
                                                                 company: company,
                                                                 jobApplicationSource: jobApplicationSource,
                                                                 candidate: candidate,
                                                                 jobPositionTitle: jobPositionTitle,
                                                                 workLocationType: workLocationType,
                                                                 jobType: jobType,
                                                                 jobAdLink: jobAdLink,
                                                                 description: description));

        return this;
    }

    /// <summary>
    /// Represents time when candidate applied for the job.
    /// </summary>
    public DateTime AppliedOnUTC { get; }

    /// <summary>
    /// Application source through candidate applied.
    /// </summary>
    public JobApplicationSource ApplicationSource { get; private set; }

    /// <summary>
    /// Current job application status.
    /// </summary>
    public JobApplicationStatus JobApplicationStatus { get; private set; }

    /// <summary>
    /// Job type like <see cref="JobType.FullTime"/> or <see cref="JobType.PartTime"/>.
    /// </summary>
    public JobType JobType { get; private set; }

    /// <summary>
    /// Work location type like <see cref="WorkLocationType.Remote"/> or <see cref="WorkLocationType.Hybrid"/>.
    /// </summary>
    public WorkLocationType WorkLocationType { get; private set; }

    //TODO: make it as separate table?
    /// <summary>
    /// Title for the position like Senior .NET software engineer.
    /// </summary>
    public string JobPositionTitle { get; private set; }

    public string? Description { get; private set; }

    public Link JobAdLink { get; private set; }

    /// <summary>
    /// Company the candidate applies for.
    /// </summary>
    public Company Company { get; private set; }

    /// <summary>
    /// The candidate for the job.
    /// </summary>
    public Candidate Candidate { get; private set; }

    /// <summary>
    /// Interview steps each job application has. Using AsReadOnly() will create a read only wrapper around the private
    /// list so is protected against "external updates". It's much cheaper than .ToList() because it will not have to
    /// copy all items in a new collection. (Just one heap alloc for the wrapper instance)
    /// https://msdn.microsoft.com/en-us/library/e78dcd75(v=vs.110).aspx
    /// </summary>
    public IReadOnlyList<InterviewStep> InterviewSteps => _interviewSteps.AsReadOnly();

    /// <inheritdoc/>
    public DateTime? DeletedOnUtc { get; private set; }

    /// <summary>When set, the application is archived — hidden from the board/list but not deleted.</summary>
    public DateTime? ArchivedOnUtc { get; private set; }

    /// <inheritdoc/>
    public DateTime CreatedOnUtc { get; private set; }

    /// <inheritdoc/>
    public DateTime ModifiedOnUtc { get; private set; }
}
