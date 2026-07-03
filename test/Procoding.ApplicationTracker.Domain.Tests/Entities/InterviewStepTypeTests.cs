using NUnit.Framework;
using Procoding.ApplicationTracker.Domain.Entities;
using System;
using Procoding.ApplicationTracker.Domain.Tests.TestData;

namespace Procoding.ApplicationTracker.Domain.Tests.Entities;

[TestFixture]
public class InterviewStepTypeTests
{
    [Test]
    public void Constructor_ValidParameters_ObjectCreated()
    {
        // Arrange
        var jobApplication = JobApplicationTestData.ValidJobApplication;
        var id = Guid.NewGuid();
        var occurredOn = DateTime.UtcNow;
        var notes = "Went well, they asked about EF Core.";

        // Act
        var interviewStep = new InterviewStep(jobApplication, id, InterviewStepType.TechnicalInterview, occurredOn, InterviewStepOutcome.Passed, notes);

        // Assert
        Assert.That(interviewStep, Is.Not.Null);
        Assert.That(interviewStep.Type, Is.EqualTo(InterviewStepType.TechnicalInterview));
        Assert.That(interviewStep.OccurredOn, Is.EqualTo(occurredOn));
        Assert.That(interviewStep.Outcome, Is.EqualTo(InterviewStepOutcome.Passed));
        Assert.That(interviewStep.Notes, Is.EqualTo(notes));
        Assert.That(interviewStep.JobApplication, Is.EqualTo(jobApplication));
    }

    [Test]
    public void Constructor_NullNotes_IsAllowed()
    {
        // Arrange
        var jobApplication = JobApplicationTestData.ValidJobApplication;

        // Act
        var interviewStep = new InterviewStep(jobApplication, Guid.NewGuid(), InterviewStepType.PhoneScreen, DateTime.UtcNow, InterviewStepOutcome.Pending, null);

        // Assert
        Assert.That(interviewStep.Notes, Is.Null);
    }

    [Test]
    public void Constructor_NotesTooLong_ArgumentExceptionThrown()
    {
        // Arrange
        var jobApplication = JobApplicationTestData.ValidJobApplication;
        var notes = new string('x', InterviewStep.MaxLengthForNotes + 1);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new InterviewStep(jobApplication, Guid.NewGuid(), InterviewStepType.TechnicalInterview, DateTime.UtcNow, InterviewStepOutcome.Pending, notes));
    }

    [Test]
    public void Constructor_NullJobApplication_ArgumentNullExceptionThrown()
    {
        // Arrange
        JobApplication? jobApplication = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new InterviewStep(jobApplication, Guid.NewGuid(), InterviewStepType.TechnicalInterview, DateTime.UtcNow, InterviewStepOutcome.Pending, "notes"));
    }
}
