using _116.Content.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _116.Content.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for <see cref="VideoShareEntity" />.
/// UserId is nullable (anonymous shares allowed); no FK to identity schema by design.
/// </summary>
public class VideoShareConfiguration : IEntityTypeConfiguration<VideoShareEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<VideoShareEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId).IsRequired(false);

        builder.Property(x => x.VideoId).IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasOne(x => x.Video).WithMany().HasForeignKey(x => x.VideoId).OnDelete(DeleteBehavior.Cascade);
    }
}
