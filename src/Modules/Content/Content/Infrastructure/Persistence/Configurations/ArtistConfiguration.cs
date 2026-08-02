using _116.Content.Domain.Constants;
using _116.Content.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _116.Content.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the <see cref="ArtistEntity" />.
/// Defines the table structure, constraints, and indexes for artist profiles.
/// </summary>
public class ArtistConfiguration : IEntityTypeConfiguration<ArtistEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ArtistEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(ContentConstants.MaxArtistNameLength).IsRequired();

        builder.Property(x => x.Slug).HasMaxLength(ContentConstants.MaxSlugLength).IsRequired();

        builder.Property(x => x.Bio).IsRequired(false);

        builder.Property(x => x.AvatarFileId).IsRequired(false);

        builder.Property(x => x.UserId).IsRequired(false);

        builder.Property(x => x.VerifiedAt).IsRequired(false);

        builder.HasIndex(x => x.Slug).IsUnique();

        // Partial unique index: a claimed profile's UserId must be unique, but many
        // unclaimed profiles can all have UserId = null without conflicting.
        builder.HasIndex(x => x.UserId).IsUnique().HasFilter("user_id IS NOT NULL");
    }
}
