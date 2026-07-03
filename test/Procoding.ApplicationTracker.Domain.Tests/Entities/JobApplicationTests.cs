using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using NUnit.Framework;
using Procoding.ApplicationTracker.Domain.Entities;
using Procoding.ApplicationTracker.Domain.Events;
using Procoding.ApplicationTracker.Domain.Tests.TestData;
using Procoding.ApplicationTracker.Domain.ValueObjects;

namespace Procoding.ApplicationTracker.Domain.Tests.Entities;

[TestFixture]
public class JobApplicationTests
{
    [Test]
    public void Create_ValidParameters_ReturnsJobApplication()
    {
        // Arrange
        var candidate = CandidateTestData.ValidCandidate;
        var id = Guid.NewGuid();
        var jobApplicationSource = JobApplicationSourceTestData.ValidJobApplicationSource;
        var company = CompanyTestData.ValidCompany;
        var timeProvider = Substitute.For<FakeTimeProvider>();
        timeProvider.GetUtcNow().Returns(DateTime.UtcNow);

        // Act
        var jobApplication = JobApplication.Create(candidate: candidate,
                                                   id: id,
                                                   jobApplicationSource: jobApplicationSource,
                                                   company: company,
                                                   timeProvider: timeProvider,
                                                   jobPositionTitle: "Senior .NET sw engineer",
                                                   jobAdLink: new Link("https://www.link2.com"),
                                                   workLocationType: WorkLocationType.Remote,
                                                   jobType: JobType.FullTime,
                                                   description: "desc");

        // Assert
        Assert.That(jobApplication, Is.Not.Null);
        Assert.That(jobApplication.Candidate, Is.EqualTo(candidate));
        Assert.That(jobApplication.AppliedOnUTC, Is.EqualTo(timeProvider.GetUtcNow().DateTime));
        Assert.That(jobApplication.ApplicationSource, Is.EqualTo(jobApplicationSource));
        Assert.That(jobApplication.Company, Is.EqualTo(company));
        Assert.That(jobApplication.JobApplicationStatus, Is.EqualTo(JobApplicationStatus.Applied));
        Assert.That(jobApplication.JobPositionTitle, Is.EqualTo("Senior .NET sw engineer"));
        Assert.That(jobApplication.Description, Is.EqualTo("desc"));
        Assert.That(jobApplication.JobAdLink, Is.EqualTo(new Link("https://www.link2.com")));
        Assert.That(jobApplication.WorkLocationType, Is.EqualTo(WorkLocationType.Remote));
        Assert.That(jobApplication.JobType, Is.EqualTo(JobType.FullTime));





    }

    [Test]
    public void AddInterviewStep_ValidParameters_AddsInterviewStep()
    {
        // Arrange
        var jobApplication = JobApplicationTestData.ValidJobApplication;
        var id = Guid.NewGuid();

        // Act
        var interviewStep = jobApplication.AddInterviewStep(id, InterviewStepType.TechnicalInterview, DateTime.UtcNow, InterviewStepOutcome.Pending, "Interview notes");

        // Assert
        Assert.That(jobApplication.InterviewSteps, Has.Member(interviewStep));
        Assert.That(jobApplication.InterviewSteps, Has.Count.EqualTo(1));
    }

    [Test]
    public void Create_NewJobApplication_DispatchesJobApplicationCreatedEvent()
    {
        // Arrange && Act
        var jobApplication = JobApplicationTestData.ValidJobApplication;

        // Assert
        Assert.That(jobApplication.DomainEvents, Has.Count.EqualTo(1));
        var domainEvent = jobApplication.DomainEvents.FirstOrDefault();
        Assert.That(domainEvent, Is.InstanceOf<AppliedForAJobDomainEvent>());
        var createdEvent = (AppliedForAJobDomainEvent)domainEvent!;
        Assert.That(createdEvent.JobApplication, Is.EqualTo(jobApplication));
    }

    [Test]
    public void ChangeStatus_ValidTransition_ChangesStatusAndDispatchesEvent()
    {
        // Arrange
        var jobApplication = JobApplicationTestData.ValidJobApplication;
        jobApplication.ClearDomainEvents();

        // Act
        jobApplication.ChangeStatus(JobApplicationStatus.InProcess);

        // Assert
        Assert.That(jobApplication.JobApplicationStatus, Is.EqualTo(JobApplicationStatus.InProcess));
        Assert.That(jobApplication.DomainEvents, Has.Count.EqualTo(1));
        var domainEvent = jobApplication.DomainEvents.FirstOrDefault();
        Assert.That(domainEvent, Is.InstanceOf<JobApplicationStatusChangedDomainEvent>());
        var statusChanged = (JobApplicationStatusChangedDomainEvent)domainEvent!;
        Assert.That(statusChanged.OldStatus, Is.EqualTo(JobApplicationStatus.Applied));
        Assert.That(statusChanged.NewStatus, Is.EqualTo(JobApplicationStatus.InProcess));
        Assert.That(statusChanged.JobApplicationId, Is.EqualTo(jobApplication.Id));
    }

    [Test]
    public void ChangeStatus_FullHappyPath_AppliedToAccepted()
    {
        // Arrange
        var jobApplication = JobApplicationTestData.ValidJobApplication;

        // Act - Applied -> InProcess -> Offer -> Accepted
        jobApplication.ChangeStatus(JobApplicationStatus.InProcess);
        jobApplication.ChangeStatus(JobApplicationStatus.Offer);
        jobApplication.ChangeStatus(JobApplicationStatus.Accepted);

        // Assert
        Assert.That(jobApplication.JobApplicationStatus, Is.EqualTo(JobApplicationStatus.Accepted));
    }

    [Test]
    public void ChangeStatus_SkippingStages_AppliedDirectlyToAccepted_IsAllowed()
    {
        // Arrange
        var jobApplication = JobApplicationTestData.ValidJobApplication;

        // Act - any status can move to any other status
        jobApplication.ChangeStatus(JobApplicationStatus.Accepted);

        // Assert
        Assert.That(jobApplication.JobApplicationStatus, Is.EqualTo(JobApplicationStatus.Accepted));
    }

    [Test]
    public void ChangeStatus_SameStatus_IsNoOpAndDispatchesNoEvent()
    {
        // Arrange
        var jobApplication = JobApplicationTestData.ValidJobApplication;
        jobApplication.ClearDomainEvents();

        // Act
        jobApplication.ChangeStatus(JobApplicationStatus.Applied);

        // Assert
        Assert.That(jobApplication.JobApplicationStatus, Is.EqualTo(JobApplicationStatus.Applied));
        Assert.That(jobApplication.DomainEvents, Is.Empty);
    }

    [Test]
    public void ChangeStatus_FromTerminalStatus_CanMoveBack()
    {
        // Arrange
        var jobApplication = JobApplicationTestData.ValidJobApplication;
        jobApplication.ChangeStatus(JobApplicationStatus.Rejected);
        jobApplication.ClearDomainEvents();

        // Act - a rejected application can be moved back (e.g. the recruiter re-engaged)
        jobApplication.ChangeStatus(JobApplicationStatus.InProcess);

        // Assert
        Assert.That(jobApplication.JobApplicationStatus, Is.EqualTo(JobApplicationStatus.InProcess));
        Assert.That(jobApplication.DomainEvents, Has.Count.EqualTo(1));
    }

    [Test]
    public void AddInterviewStep_NewInterviewAdded_DispatchesNewInterviewAddedEvent()
    {
        // Arrange
        var jobApplication = JobApplicationTestData.ValidJobApplication;
        var id = Guid.NewGuid();
        var notes = "Interview notes";

        // Act
        jobApplication.AddInterviewStep(id, InterviewStepType.TechnicalInterview, DateTime.UtcNow, InterviewStepOutcome.Passed, notes);

        // Assert
        Assert.That(jobApplication.DomainEvents, Has.Count.EqualTo(2));
        var domainEventCreation = jobApplication.DomainEvents.FirstOrDefault();
        Assert.That(domainEventCreation, Is.InstanceOf<AppliedForAJobDomainEvent>());
        var domainEventInterviewAdded = jobApplication.DomainEvents.Skip(1).Take(1).FirstOrDefault();
        Assert.That(domainEventInterviewAdded, Is.InstanceOf<NewInterviewAddedDomainEvent>());
        var interviewAddedEvent = (NewInterviewAddedDomainEvent)domainEventInterviewAdded!;
        Assert.That(interviewAddedEvent.InterviewStep.Type, Is.EqualTo(InterviewStepType.TechnicalInterview));
        Assert.That(interviewAddedEvent.InterviewStep.Notes, Is.EqualTo(notes));
    }
}

