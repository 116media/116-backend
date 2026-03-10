using _116.Content.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _116.Content.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the <see cref="CategoryPricingEntity" />.
/// Defines the table structure, constraints, and relationships for category pricing rows.
/// </summary>
public class CategoryPricingConfiguration : IEntityTypeConfiguration<CategoryPricingEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<CategoryPricingEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PriceUsd).HasColumnType("numeric(10,2)").IsRequired();

        builder
            .HasIndex(new[] { nameof(CategoryPricingEntity.CategoryId), nameof(CategoryPricingEntity.PricingTierId) })
            .IsUnique()
            .HasDatabaseName("uq_category_pricing_category_tier");

        builder
            .HasOne(x => x.Category)
            .WithMany(c => c.Pricing)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.PricingTier)
            .WithMany()
            .HasForeignKey(x => x.PricingTierId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
