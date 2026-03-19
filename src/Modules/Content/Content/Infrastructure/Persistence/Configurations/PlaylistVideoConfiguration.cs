using _116.Content.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _116.Content.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for <see cref="PlaylistVideoEntity" />.
/// </summary>
public class PlaylistVideoConfiguration : IEntityTypeConfiguration<PlaylistVideoEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<PlaylistVideoEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PlaylistId).IsRequired();

        builder.Property(x => x.VideoId).IsRequired();

        builder.Property(x => x.SortOrder).IsRequired();

        builder.HasIndex(x => new { x.PlaylistId, x.VideoId }).IsUnique();

        builder
            .HasOne(x => x.Playlist)
            .WithMany(p => p.Videos)
            .HasForeignKey(x => x.PlaylistId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Video).WithMany().HasForeignKey(x => x.VideoId).OnDelete(DeleteBehavior.Cascade);
    }
}
