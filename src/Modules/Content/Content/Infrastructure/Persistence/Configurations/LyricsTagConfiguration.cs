using _116.Content.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _116.Content.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the <see cref="LyricsTagEntity" /> junction table.
/// </summary>
public class LyricsTagConfiguration : IEntityTypeConfiguration<LyricsTagEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<LyricsTagEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.LyricsId).IsRequired();

        builder.Property(x => x.TagId).IsRequired();

        builder.HasIndex(x => new { x.LyricsId, x.TagId }).IsUnique();

        builder
            .HasOne(x => x.Lyrics)
            .WithMany(l => l.Tags)
            .HasForeignKey(x => x.LyricsId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Tag).WithMany().HasForeignKey(x => x.TagId).OnDelete(DeleteBehavior.Cascade);
    }
}
