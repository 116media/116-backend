using _116.Content.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _116.Content.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the <see cref="ContentItemTierEntity" />.
/// Defines the table structure, constraints, and relationships for item pricing tier snapshots.
/// </summary>
public class ContentItemTierConfiguration : IEntityTypeConfiguration<ContentItemTierEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ContentItemTierEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PriceSnapshotUsd).HasColumnType("numeric(10,2)").IsRequired();

        builder
            .HasOne(x => x.OrderItem)
            .WithMany(i => i.Tiers)
            .HasForeignKey(x => x.OrderItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.PricingTier)
            .WithMany()
            .HasForeignKey(x => x.PricingTierId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
