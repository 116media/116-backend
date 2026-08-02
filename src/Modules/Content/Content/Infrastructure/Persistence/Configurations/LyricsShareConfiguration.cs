using _116.Content.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _116.Content.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for <see cref="LyricsShareEntity" />.
/// UserId is nullable (anonymous shares allowed); no FK to identity schema by design.
/// </summary>
public class LyricsShareConfiguration : IEntityTypeConfiguration<LyricsShareEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<LyricsShareEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId).IsRequired(false);

        builder.Property(x => x.LyricsId).IsRequired();

        builder.Property(x => x.ShareChannel).HasConversion<string>().HasMaxLength(50).IsRequired(false);

        builder.Property(x => x.CreatedAt).IsRequired();

        builder
            .HasIndex(x => new
            {
                x.UserId,
                x.CreatedAt,
                x.LyricsId,
            })
            .IsDescending(false, true, false)
            .HasFilter("user_id IS NOT NULL")
            .HasDatabaseName("ix_lyrics_shares_user_created_lyrics");

        builder.HasOne(x => x.Lyrics).WithMany().HasForeignKey(x => x.LyricsId).OnDelete(DeleteBehavior.Cascade);
    }
}
