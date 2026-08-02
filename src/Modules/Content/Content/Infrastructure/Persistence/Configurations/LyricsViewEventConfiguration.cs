using _116.Content.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _116.Content.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for <see cref="LyricsViewEventEntity" />.
/// The composite index backs the per-identity dedup-window lookup on every view.
/// </summary>
public class LyricsViewEventConfiguration : IEntityTypeConfiguration<LyricsViewEventEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<LyricsViewEventEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.LyricsId).IsRequired();

        builder.Property(x => x.UserId).IsRequired(false);

        builder.Property(x => x.DedupKey).HasMaxLength(100).IsRequired();

        builder.Property(x => x.IpAddress).HasMaxLength(64).IsRequired(false);

        builder.Property(x => x.UserAgent).HasMaxLength(500).IsRequired(false);

        builder.Property(x => x.IsCounted).IsRequired();

        builder.Property(x => x.DwellMs).IsRequired();

        builder.Property(x => x.ScrollDepthRatio).IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasOne(x => x.Lyrics).WithMany().HasForeignKey(x => x.LyricsId).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new
        {
            x.LyricsId,
            x.DedupKey,
            x.CreatedAt,
        });
    }
}
