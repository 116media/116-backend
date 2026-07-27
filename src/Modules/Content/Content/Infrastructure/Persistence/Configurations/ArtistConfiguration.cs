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

        builder.Property(x => x.NameFolded).HasMaxLength(ContentConstants.MaxArtistNameLength).IsRequired();

        builder.Property(x => x.InitialLetter).HasMaxLength(1).IsRequired();

        builder.Property(x => x.RealName).HasMaxLength(ContentConstants.MaxArtistRealNameLength).IsRequired(false);

        // Postgres text[] via Npgsql. The list is display-only and never queried, so a join
        // table would add an entity and a migration to express "render these strings".
        // The default is a SQL literal, never HasDefaultValue(new List<string>()) — a fresh
        // list instance makes the model differ from its snapshot on every build, which trips
        // EF's PendingModelChangesWarning and blocks migration on startup.
        builder
            .Property<List<string>>("_aliases")
            .HasColumnName("aliases")
            .HasDefaultValueSql("'{}'::text[]")
            .IsRequired();

        builder.Ignore(x => x.Aliases);

        builder.Property(x => x.Birthdate).HasColumnType("date").IsRequired(false);

        builder.Property(x => x.Hometown).HasMaxLength(ContentConstants.MaxArtistHometownLength).IsRequired(false);

        builder.Property(x => x.AvatarFileId).IsRequired(false);

        builder.Property(x => x.UserId).IsRequired(false);

        builder.Property(x => x.VerifiedAt).IsRequired(false);

        builder.HasIndex(x => x.Slug).IsUnique();

        // Drives the directory's alphabetical ordering and its public name search.
        builder.HasIndex(x => x.NameFolded);

        // Drives the letter filter and the available-letters rail, in that order.
        builder.HasIndex(x => new { x.InitialLetter, x.NameFolded });

        // Partial unique index: a claimed profile's UserId must be unique, but many
        // unclaimed profiles can all have UserId = null without conflicting.
        builder.HasIndex(x => x.UserId).IsUnique().HasFilter("user_id IS NOT NULL");
    }
}
