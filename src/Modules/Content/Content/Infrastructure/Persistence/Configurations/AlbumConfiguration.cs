using _116.Content.Domain.Constants;
using _116.Content.Domain.Entities;
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

        builder
            .HasOne<ArtistEntity>()
            .WithMany()
            .HasForeignKey(x => x.ArtistId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
