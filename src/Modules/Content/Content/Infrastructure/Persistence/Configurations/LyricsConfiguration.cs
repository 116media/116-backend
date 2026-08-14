using _116.Content.Domain.Constants;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
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

        builder.Property(x => x.AuthorId).IsRequired();

        builder.Property(x => x.Slug).HasMaxLength(ContentConstants.MaxSlugLength).IsRequired();

        builder.Property(x => x.Status).HasConversion<string>().HasDefaultValue(EnumContentStatus.Draft).IsRequired();

        builder
            .Property(x => x.RejectionReason)
            .HasMaxLength(ContentConstants.MaxRejectionReasonLength)
            .IsRequired(false);

        builder.Property(x => x.PublishedAt).IsRequired(false);

        builder.Property(x => x.OrderItemId).IsRequired(false);

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

        // StructuredData is stored as JSONB in PostgreSQL for schema.org JSON-LD
        builder.Property(x => x.StructuredData).HasColumnType("jsonb").IsRequired(false);

        builder.Property(x => x.Album).HasMaxLength(ContentConstants.MaxAlbumNameLength).IsRequired(false);

        builder.Property(x => x.Label).HasMaxLength(ContentConstants.MaxLabelNameLength).IsRequired(false);

        builder.Property(x => x.Songwriter).HasMaxLength(ContentConstants.MaxCreditNameLength).IsRequired(false);

        builder.Property(x => x.Producer).HasMaxLength(ContentConstants.MaxCreditNameLength).IsRequired(false);

        builder.Property(x => x.ReleaseYear).IsRequired(false);

        builder.Property(x => x.IsPromoted).HasDefaultValue(false).IsRequired();

        builder.Property(x => x.PromotedUntil).IsRequired(false);

        builder.Property(x => x.UnpromotedAt).IsRequired(false);

        builder.Property(x => x.UnpromotedBy).IsRequired(false);

        builder.Property(x => x.UnpromotedReason).HasMaxLength(500).IsRequired(false);

        builder
            .HasOne(x => x.Video)
            .WithMany()
            .HasForeignKey(x => x.VideoId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.Slug).IsUnique();

        // Serves the artist profile's song surface and the artist content predicate — both
        // filter on this exact pair per artist row.
        builder.HasIndex(x => new { x.ArtistId, x.Status });

        builder.HasOne(x => x.Category).WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.Customer)
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasOne<ArtistEntity>()
            .WithMany()
            .HasForeignKey(x => x.ArtistId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasOne<AlbumEntity>()
            .WithMany()
            .HasForeignKey(x => x.AlbumId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
