using _116.Content.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _116.Content.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the <see cref="ContentOrderItemEntity" />.
/// Defines the table structure, constraints, and relationships for order items.
/// </summary>
public class ContentOrderItemConfiguration : IEntityTypeConfiguration<ContentOrderItemEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ContentOrderItemEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ContentKind).IsRequired();

        builder.Property(x => x.SocialBoost).IsRequired();

        builder.Property(x => x.IsBonus).IsRequired();

        builder.Property(x => x.PromoPriceSnapshotUsd).HasColumnType("numeric(10,2)").IsRequired(false);

        builder
            .HasOne(x => x.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Category).WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.PromotionLevel)
            .WithMany()
            .HasForeignKey(x => x.PromotionLevelId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
