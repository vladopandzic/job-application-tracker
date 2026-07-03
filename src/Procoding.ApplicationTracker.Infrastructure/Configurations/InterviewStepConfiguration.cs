using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Procoding.ApplicationTracker.Domain.Entities;

namespace Procoding.ApplicationTracker.Infrastructure.Configurations;

/// <summary>
/// This class is used to configure the <see cref="InterviewStep"/> entity. It is used to map the <see
/// cref="InterviewStep"/> entity. to a database table.
/// </summary>
public class InterviewStepConfiguration : IEntityTypeConfiguration<InterviewStep>
{
    public void Configure(EntityTypeBuilder<InterviewStep> builder)
    {
        builder.ToTable(nameof(JobApplication.InterviewSteps));
        builder.HasKey(x => x.Id);

        // Ids are assigned by the domain (client-generated Guids). Without this, EF treats a nav-added
        // step with a set key as already existing and issues an UPDATE (0 rows) instead of an INSERT.
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Type)
               .HasConversion<string>()
               .HasMaxLength(64)
               .IsRequired();

        builder.Property(x => x.Outcome)
               .HasConversion<string>()
               .HasMaxLength(32)
               .IsRequired();

        builder.Property(x => x.OccurredOn);

        builder.Property(x => x.Notes)
               .HasMaxLength(InterviewStep.MaxLengthForNotes);

        builder.HasOne(x => x.JobApplication)
               .WithMany(x => x.InterviewSteps)
               .IsRequired();
    }
}
