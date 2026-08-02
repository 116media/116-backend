using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _116.Content.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the <see cref="LyricsTranslationRevisionEntity" />.
/// Defines the table structure, constraints, and relationships for proposed translation corrections.
/// </summary>
public class LyricsTranslationRevisionConfiguration : IEntityTypeConfiguration<LyricsTranslationRevisionEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<LyricsTranslationRevisionEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TranslationId).IsRequired();

        builder.Property(x => x.ProposedText).IsRequired();

        builder.Property(x => x.EditSummary).IsRequired(false);

        builder.Property(x => x.ProposedByUserId).IsRequired();

        builder
            .Property(x => x.Status)
            .HasConversion<string>()
            .HasDefaultValue(EnumRevisionStatus.Pending)
            .IsRequired();

        builder.Property(x => x.DecidedByUserId).IsRequired(false);

        // A translation revision has no meaning without its translation, so deleting the
        // translation cascades to delete its own pending/decided revisions — matching
        // StreamingLinkEntity's reasoning for parent-dependent rows in this module.
        builder
            .HasOne<LyricsTranslationEntity>()
            .WithMany()
            .HasForeignKey(x => x.TranslationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
