using _116.Content.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _116.Content.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for <see cref="ShortVideoViewEventEntity" />.
/// The composite index backs the per-identity dedup-window lookup on every view.
/// </summary>
public class ShortVideoViewEventConfiguration : IEntityTypeConfiguration<ShortVideoViewEventEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ShortVideoViewEventEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ShortVideoId).IsRequired();

        builder.Property(x => x.UserId).IsRequired(false);

        builder.Property(x => x.DedupKey).HasMaxLength(100).IsRequired();

        builder.Property(x => x.IpAddress).HasMaxLength(64).IsRequired(false);

        builder.Property(x => x.UserAgent).HasMaxLength(500).IsRequired(false);

        builder.Property(x => x.IsCounted).IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired();

        builder
            .HasOne<ShortVideoEntity>()
            .WithMany()
            .HasForeignKey(x => x.ShortVideoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new
        {
            x.ShortVideoId,
            x.DedupKey,
            x.CreatedAt,
        });

        // The nightly cleanup deletes uncounted rows older than the cutoff; the filtered index
        // keeps that scan off the fastest-growing table's full heap.
        builder
            .HasIndex(x => x.CreatedAt)
            .HasDatabaseName("ix_short_video_view_events_uncounted_created_at")
            .HasFilter("is_counted = false");
    }
}
