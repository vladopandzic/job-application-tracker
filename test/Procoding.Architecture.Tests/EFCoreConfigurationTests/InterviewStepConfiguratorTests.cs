using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NUnit.Framework;
using Procoding.ApplicationTracker.Domain.Entities;
using Procoding.ApplicationTracker.Domain.ValueObjects;
using Procoding.ApplicationTracker.Infrastructure.Configurations;
using Procoding.Architecture.Tests.Helpers;
using System.Reflection;

namespace Procoding.Architecture.Tests.EFCoreConfigurationTests
{
    [TestFixture]
    public class InterviewStepConfiguratorTests
    {
        [Test]
        public void InterviewStep_Entity_Configuration_Is_Valid()
        {
            // Arrange
            var modelBuilder = new ModelBuilder(new ConventionSet());
            var entityTypeBuilder = modelBuilder.Entity<InterviewStep>();

            var configuration = new InterviewStepConfiguration();

            // Act
            configuration.Configure(entityTypeBuilder);
            var model = modelBuilder.FinalizeModel();
            var entity = model.FindEntityType(typeof(InterviewStep))!;

            //Assert
            Assert.That(entity, Is.Not.Null, "Entity should not be null");
            Assert.That(entity.GetTableName(), Is.EqualTo("InterviewSteps"));
            Assert.That(EntityTypeConfigurationHelper.PrimaryKey(entity, nameof(InterviewStep.Id)), Is.True);
            Assert.That(EntityTypeConfigurationHelper.Property(entity, nameof(InterviewStep.Notes))!.GetMaxLength, Is.EqualTo(InterviewStep.MaxLengthForNotes));
            Assert.That(EntityTypeConfigurationHelper.Property(entity, nameof(InterviewStep.Type))?.IsNullable, Is.False);
        }

    }
}
