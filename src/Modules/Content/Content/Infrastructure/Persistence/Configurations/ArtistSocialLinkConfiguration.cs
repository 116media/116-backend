using _116.Content.Domain.Constants;
using _116.Content.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _116.Content.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the <see cref="ArtistSocialLinkEntity" />.
/// Defines the table structure, constraints, and relationships for artist social links.
/// </summary>
public class ArtistSocialLinkConfiguration : IEntityTypeConfiguration<ArtistSocialLinkEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ArtistSocialLinkEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Url).HasMaxLength(ContentConstants.MaxStreamingLinkUrlLength).IsRequired();

        builder.HasIndex(x => new { x.ArtistId, x.Platform }).IsUnique();

        // A social link has no meaning without its artist, so deleting the profile cascades
        // to its links — the same call StreamingLinkConfiguration makes for the same reason.
        builder.HasOne(x => x.Artist).WithMany().HasForeignKey(x => x.ArtistId).OnDelete(DeleteBehavior.Cascade);
    }
}
