using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _116.Content.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the <see cref="LyricsRevisionEntity" />.
/// Defines the table structure, constraints, and relationships for proposed lyrics-text corrections.
/// </summary>
public class LyricsRevisionConfiguration : IEntityTypeConfiguration<LyricsRevisionEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<LyricsRevisionEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.LyricsId).IsRequired();

        builder.Property(x => x.ProposedText).IsRequired();

        builder.Property(x => x.EditSummary).IsRequired(false);

        builder.Property(x => x.ProposedByUserId).IsRequired();

        builder
            .Property(x => x.Status)
            .HasConversion<string>()
            .HasDefaultValue(EnumRevisionStatus.Pending)
            .IsRequired();

        builder.Property(x => x.DecidedByUserId).IsRequired(false);

        // A correction proposal has no meaning if the underlying lyrics record is deleted, so
        // deleting the lyrics page cascades to delete its own pending/decided revisions.
        builder.HasOne<LyricsEntity>().WithMany().HasForeignKey(x => x.LyricsId).OnDelete(DeleteBehavior.Cascade);
    }
}
