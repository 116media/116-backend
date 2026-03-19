using _116.Content.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _116.Content.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the <see cref="VideoTagEntity" /> junction table.
/// </summary>
public class VideoTagConfiguration : IEntityTypeConfiguration<VideoTagEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<VideoTagEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.VideoId).IsRequired();

        builder.Property(x => x.TagId).IsRequired();

        builder.HasIndex(x => new { x.VideoId, x.TagId }).IsUnique();

        builder
            .HasOne(x => x.Video)
            .WithMany(v => v.Tags)
            .HasForeignKey(x => x.VideoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Tag).WithMany().HasForeignKey(x => x.TagId).OnDelete(DeleteBehavior.Cascade);
    }
}
