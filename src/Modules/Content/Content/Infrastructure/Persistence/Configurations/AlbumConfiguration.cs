using _116.Content.Domain.Constants;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _116.Content.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the <see cref="AlbumEntity" />.
/// Defines the table structure, constraints, and relationships for albums.
/// </summary>
public class AlbumConfiguration : IEntityTypeConfiguration<AlbumEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AlbumEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(ContentConstants.MaxAlbumNameLength).IsRequired();

        builder.Property(x => x.CoverImageFileId).IsRequired(false);

        builder.Property(x => x.ReleaseYear).IsRequired(false);

        builder.Property(x => x.Label).HasMaxLength(ContentConstants.MaxLabelNameLength).IsRequired(false);

        // Rows that predate the discriminator backfill to Album via the column default;
        // editors correct mis-filed mixtapes from there rather than blocking the migration
        // on a full catalog audit.
        builder.Property(x => x.ReleaseType).HasDefaultValue(EnumReleaseType.Album).IsRequired();

        // Serves the artist-scoped release query and the album term of the artist content
        // count — one index, both readers.
        builder.HasIndex(x => new { x.ArtistId, x.ReleaseType });

        builder
            .HasOne<ArtistEntity>()
            .WithMany()
            .HasForeignKey(x => x.ArtistId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
