using _116.Content.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _116.Content.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for <see cref="ShortVideoBookmarkEntity" />.
/// </summary>
public class ShortVideoBookmarkConfiguration : IEntityTypeConfiguration<ShortVideoBookmarkEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ShortVideoBookmarkEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId).IsRequired();

        builder.Property(x => x.ShortVideoId).IsRequired();

        builder.HasIndex(x => new { x.UserId, x.ShortVideoId }).IsUnique();

        builder
            .HasOne(x => x.ShortVideo)
            .WithMany()
            .HasForeignKey(x => x.ShortVideoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
