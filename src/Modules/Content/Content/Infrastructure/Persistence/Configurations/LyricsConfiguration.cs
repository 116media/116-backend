using _116.Content.Domain.Constants;
using _116.Content.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _116.Content.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the <see cref="LyricsEntity" />.
/// Defines the table structure, constraints, and relationships for lyrics pages.
/// </summary>
public class LyricsConfiguration : IEntityTypeConfiguration<LyricsEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<LyricsEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.SongTitle).HasMaxLength(ContentConstants.MaxSongTitleLength).IsRequired();

        builder.Property(x => x.ArtistName).HasMaxLength(ContentConstants.MaxArtistNameLength).IsRequired();

        builder.Property(x => x.LyricsText).IsRequired();

        builder
            .Property(x => x.Language)
            .HasMaxLength(ContentConstants.MaxLyricsLanguageLength)
            .HasDefaultValue(ContentConstants.DefaultLyricsLanguage)
            .IsRequired();

        builder.Property(x => x.MetaTitle).HasMaxLength(ContentConstants.MaxMetaTitleLength).IsRequired(false);

        builder
            .Property(x => x.MetaDescription)
            .HasMaxLength(ContentConstants.MaxMetaDescriptionLength)
            .IsRequired(false);

        builder.Property(x => x.MetaKeywords).HasMaxLength(ContentConstants.MaxMetaKeywordsLength).IsRequired(false);

        // StructuredData is stored as JSONB in PostgreSQL for schema.org JSON-LD
        builder.Property(x => x.StructuredData).HasColumnType("jsonb").IsRequired(false);

        builder
            .HasOne(x => x.Video)
            .WithMany()
            .HasForeignKey(x => x.VideoId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasOne(x => x.Article)
            .WithMany()
            .HasForeignKey(x => x.ArticleId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
