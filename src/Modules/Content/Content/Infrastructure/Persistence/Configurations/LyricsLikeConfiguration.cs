using _116.Content.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _116.Content.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for <see cref="LyricsLikeEntity" />.
/// </summary>
public class LyricsLikeConfiguration : IEntityTypeConfiguration<LyricsLikeEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<LyricsLikeEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId).IsRequired();

        builder.Property(x => x.LyricsId).IsRequired();

        builder.HasIndex(x => new { x.UserId, x.LyricsId }).IsUnique();

        builder.HasOne(x => x.Lyrics).WithMany().HasForeignKey(x => x.LyricsId).OnDelete(DeleteBehavior.Cascade);
    }
}
