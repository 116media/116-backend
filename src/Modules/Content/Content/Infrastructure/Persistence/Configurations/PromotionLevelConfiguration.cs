using _116.Content.Domain.Constants;
using _116.Content.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _116.Content.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the <see cref="PromotionLevelEntity" />.
/// Defines the table structure, constraints, and indexes for promotion levels.
/// </summary>
public class PromotionLevelConfiguration : IEntityTypeConfiguration<PromotionLevelEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<PromotionLevelEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(ContentConstants.MaxPromotionLevelNameLength).IsRequired();

        builder.Property(x => x.DurationDays).IsRequired();

        builder.Property(x => x.PriceUsd).HasPrecision(10, 2).IsRequired();

        builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);

        builder.HasIndex(x => x.Name).IsUnique();
    }
}
