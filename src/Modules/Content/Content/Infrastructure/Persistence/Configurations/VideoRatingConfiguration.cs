using _116.Content.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _116.Content.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for <see cref="VideoRatingEntity" />.
/// A unique index enforces one rating per user per video.
/// </summary>
public class VideoRatingConfiguration : IEntityTypeConfiguration<VideoRatingEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<VideoRatingEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId).IsRequired();

        builder.Property(x => x.VideoId).IsRequired();

        builder
            .Property(x => x.Stars)
            .IsRequired()
            .HasAnnotation("CheckConstraint:stars_range", "stars >= 1 AND stars <= 5");

        builder.HasIndex(x => new { x.UserId, x.VideoId }).IsUnique().HasDatabaseName("ix_video_ratings_user_video");

        builder.HasOne(x => x.Video).WithMany().HasForeignKey(x => x.VideoId).OnDelete(DeleteBehavior.Cascade);
    }
}
