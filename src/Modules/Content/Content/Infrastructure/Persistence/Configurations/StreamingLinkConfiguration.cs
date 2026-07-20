using _116.Content.Domain.Constants;
using _116.Content.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _116.Content.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the <see cref="StreamingLinkEntity" />.
/// Defines the table structure, constraints, and relationships for streaming platform links.
/// </summary>
public class StreamingLinkConfiguration : IEntityTypeConfiguration<StreamingLinkEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<StreamingLinkEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Url).HasMaxLength(ContentConstants.MaxStreamingLinkUrlLength).IsRequired();

        builder.HasIndex(x => new { x.AlbumId, x.Platform }).IsUnique();

        builder.HasIndex(x => new { x.LyricsId, x.Platform }).IsUnique();

        // A streaming link has no meaning without its parent release, so deleting the album or
        // single cascades to delete its own streaming links — unlike the SetNull FKs used
        // elsewhere in this module for artist/category references.
        builder.ToTable(t =>
            t.HasCheckConstraint(
                "ck_streaming_links_exactly_one_target",
                "(album_id IS NOT NULL AND lyrics_id IS NULL) OR (album_id IS NULL AND lyrics_id IS NOT NULL)"
            )
        );

        builder.HasOne(x => x.Album).WithMany().HasForeignKey(x => x.AlbumId).OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Lyrics).WithMany().HasForeignKey(x => x.LyricsId).OnDelete(DeleteBehavior.Cascade);
    }
}
