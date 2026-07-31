using _116.Content.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _116.Content.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the <see cref="ArtistClaimRequestEntity" />.
/// Defines the table structure for durable artist ownership claim requests.
/// </summary>
public class ArtistClaimRequestConfiguration : IEntityTypeConfiguration<ArtistClaimRequestEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ArtistClaimRequestEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ArtistId).IsRequired();

        builder.Property(x => x.UserId).IsRequired();

        // The future claim-review queue lists requests per artist; the user
        // index lists an account's own requests.
        builder.HasIndex(x => x.ArtistId);
        builder.HasIndex(x => x.UserId);

        // One request per account per profile: the handler's duplicate guard
        // reads this pair, and the unique constraint holds the invariant under
        // concurrent submissions.
        builder.HasIndex(x => new { x.ArtistId, x.UserId }).IsUnique();

        // No FK to ArtistEntity by design: this module's cross-aggregate
        // references stay unconstrained id columns, matching every sibling
        // configuration.
    }
}
