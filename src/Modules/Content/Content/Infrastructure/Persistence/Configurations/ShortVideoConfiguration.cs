using _116.Content.Domain.Constants;
using _116.Content.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _116.Content.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the <see cref="ShortVideoEntity" />.
/// Defines the table structure, constraints, and relationships for short video clips.
/// </summary>
public class ShortVideoConfiguration : IEntityTypeConfiguration<ShortVideoEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ShortVideoEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.AuthorId).IsRequired();

        builder.Property(x => x.Title).HasMaxLength(ContentConstants.MaxShortVideoTitleLength).IsRequired();

        builder.Property(x => x.Slug).HasMaxLength(ContentConstants.MaxSlugLength).IsRequired();

        builder.Property(x => x.VideoUrl).HasMaxLength(ContentConstants.MaxShortVideoUrlLength).IsRequired();

        builder.Property(x => x.VideoStorageKey).IsRequired();

        builder.Property(x => x.ThumbnailUrl).HasMaxLength(ContentConstants.MaxThumbnailUrlLength).IsRequired(false);

        builder.Property(x => x.ThumbnailStorageKey).IsRequired(false);

        builder.Property(x => x.HasFullVideo).HasDefaultValue(false).IsRequired();

        builder.Property(x => x.IsActive).HasDefaultValue(true).IsRequired();

        builder.Property(x => x.ViewCount).HasDefaultValue(0).IsRequired();

        builder.Property(x => x.LikeCount).HasDefaultValue(0).IsRequired();

        builder.Property(x => x.ShareCount).HasDefaultValue(0).IsRequired();

        builder.Property(x => x.BookmarkCount).HasDefaultValue(0).IsRequired();

        builder.HasIndex(x => x.Slug).IsUnique();
        builder.HasIndex(x => x.Title).IsUnique();

        builder
            .HasOne(x => x.ParentVideo)
            .WithMany(v => v.Shorts)
            .HasForeignKey(x => x.VideoId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
