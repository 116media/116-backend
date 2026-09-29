using _116.Content.Domain.Constants;
using _116.Content.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _116.Content.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for <see cref="PlaylistEntity" />.
/// </summary>
public class PlaylistConfiguration : IEntityTypeConfiguration<PlaylistEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<PlaylistEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId).IsRequired();

        builder.Property(x => x.Name).HasMaxLength(ContentConstants.MaxPlaylistNameLength).IsRequired();

        builder.HasIndex(x => x.UserId).HasDatabaseName("ix_playlists_user");
    }
}
