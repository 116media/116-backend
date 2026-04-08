using _116.Content.Domain.Constants;
using _116.Content.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _116.Content.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the <see cref="PricingTierEntity" />.
/// Defines the table structure, constraints, and indexes for pricing tiers.
/// </summary>
public class PricingTierConfiguration : IEntityTypeConfiguration<PricingTierEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<PricingTierEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(ContentConstants.MaxPricingTierNameLength).IsRequired();

        builder
            .Property(x => x.Description)
            .HasMaxLength(ContentConstants.MaxPricingTierDescriptionLength)
            .IsRequired();

        builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);

        builder.HasIndex(x => x.Name).IsUnique();
    }
}
