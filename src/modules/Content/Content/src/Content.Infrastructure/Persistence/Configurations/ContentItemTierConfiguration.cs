using _116.Content.Domain.Entities;
using _116.Content.Domain.ValueObjects;
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

        builder
            .Property(x => x.PriceSnapshotUsd)
            .HasConversion(money => money.Amount, value => new Money(value))
            .HasColumnType("numeric(10,2)")
            .IsRequired();

        builder
            .HasOne<ContentOrderItemEntity>()
            .WithMany(i => i.Tiers)
            .HasForeignKey(x => x.OrderItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne<PricingTierEntity>()
            .WithMany()
            .HasForeignKey(x => x.PricingTierId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
