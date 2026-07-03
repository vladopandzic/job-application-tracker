using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Procoding.ApplicationTracker.Domain.Entities;

namespace Procoding.ApplicationTracker.Infrastructure.Configurations;

/// <summary>
/// Configures the <see cref="Translation"/> entity mapping.
/// </summary>
public sealed class TranslationConfiguration : IEntityTypeConfiguration<Translation>
{
    public void Configure(EntityTypeBuilder<Translation> builder)
    {
        builder.ToTable("Translations");
        builder.HasKey(x => x.Id);

        // Ids are assigned by the domain (client-generated Guids).
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Key)
               .HasMaxLength(Translation.MaxLengthForKey)
               .IsRequired();

        builder.Property(x => x.LanguageCode)
               .HasMaxLength(Translation.MaxLengthForLanguageCode)
               .IsRequired();

        builder.Property(x => x.Value)
               .HasMaxLength(Translation.MaxLengthForValue);

        builder.HasIndex(x => new { x.Key, x.LanguageCode }).IsUnique();
    }
}
