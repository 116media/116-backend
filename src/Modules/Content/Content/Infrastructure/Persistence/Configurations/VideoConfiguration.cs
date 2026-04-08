using _116.Content.Domain.Constants;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _116.Content.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the <see cref="VideoEntity" />.
/// Defines the table structure, constraints, relationships, and indexes for videos.
/// </summary>
public class VideoConfiguration : IEntityTypeConfiguration<VideoEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<VideoEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.AuthorId).IsRequired();

        builder.Property(x => x.Title).HasMaxLength(ContentConstants.MaxTitleLength).IsRequired();

        builder.Property(x => x.Slug).HasMaxLength(ContentConstants.MaxSlugLength).IsRequired();

        builder.Property(x => x.Description).IsRequired();

        builder.Property(x => x.ThumbnailUrl).HasMaxLength(ContentConstants.MaxThumbnailUrlLength).IsRequired(false);

        builder.Property(x => x.ThumbnailStorageKey).IsRequired(false);

        builder
            .Property(x => x.YoutubeVideoId)
            .HasMaxLength(ContentConstants.MaxYoutubeVideoIdLength)
            .IsRequired(false);

        builder.Property(x => x.Status).HasConversion<string>().HasDefaultValue(EnumContentStatus.Draft).IsRequired();

        builder
            .Property(x => x.RejectionReason)
            .HasMaxLength(ContentConstants.MaxRejectionReasonLength)
            .IsRequired(false);

        builder.Property(x => x.MetaTitle).HasMaxLength(ContentConstants.MaxMetaTitleLength).IsRequired(false);

        builder
            .Property(x => x.MetaDescription)
            .HasMaxLength(ContentConstants.MaxMetaDescriptionLength)
            .IsRequired(false);

        builder.Property(x => x.SocialBoost).HasDefaultValue(false).IsRequired();

        builder.Property(x => x.IsFeatured).HasDefaultValue(false).IsRequired();

        builder.Property(x => x.HasLyrics).HasDefaultValue(false).IsRequired();

        builder.Property(x => x.RatingAverage).HasPrecision(3, 2).HasDefaultValue(0m).IsRequired();

        builder.Property(x => x.RatingCount).HasDefaultValue(0).IsRequired();

        builder.Property(x => x.ShareCount).HasDefaultValue(0).IsRequired();

        builder.HasIndex(x => x.Slug).IsUnique();
        builder.HasIndex(x => x.Title).IsUnique();

        builder.HasOne(x => x.Category).WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.Customer)
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
