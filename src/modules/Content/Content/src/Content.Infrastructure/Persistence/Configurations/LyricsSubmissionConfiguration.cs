using _116.Content.Domain.Constants;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _116.Content.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the <see cref="LyricsSubmissionEntity" />.
/// Defines the table structure and constraints for community song submissions.
/// </summary>
public class LyricsSubmissionConfiguration : IEntityTypeConfiguration<LyricsSubmissionEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<LyricsSubmissionEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.SongTitle).HasMaxLength(ContentConstants.MaxSongTitleLength).IsRequired();

        builder.Property(x => x.ArtistName).HasMaxLength(ContentConstants.MaxArtistNameLength).IsRequired();

        builder.Property(x => x.LyricsText).IsRequired();

        builder.Property(x => x.Language).HasMaxLength(ContentConstants.MaxLyricsLanguageLength).IsRequired();

        builder.Property(x => x.SubmittedByUserId).IsRequired();

        builder
            .Property(x => x.Status)
            .HasConversion<string>()
            .HasDefaultValue(EnumSubmissionStatus.Pending)
            .IsRequired();

        builder.Property(x => x.ReviewedByUserId).IsRequired(false);

        builder.Property(x => x.ReviewNote).IsRequired(false);

        // PublishedLyricsId is nullable and set only after approval. No FK/navigation is added —
        // it is a one-directional, informational cross-reference recorded for audit purposes
        // once the real LyricsEntity exists, not a relationship this aggregate needs to traverse.
        builder.Property(x => x.PublishedLyricsId).IsRequired(false);
    }
}
