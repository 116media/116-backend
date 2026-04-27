using _116.Content.Domain.Constants;
using _116.Content.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _116.Content.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the <see cref="PackageEntity" />.
/// Defines the table structure, constraints, and indexes for bundle deal packages.
/// </summary>
public class PackageConfiguration : IEntityTypeConfiguration<PackageEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<PackageEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(ContentConstants.MaxPackageNameLength).IsRequired();

        builder.Property(x => x.Description).HasMaxLength(ContentConstants.MaxPackageDescriptionLength).IsRequired();

        builder.Property(x => x.FlatPriceUsd).HasColumnType("numeric(10,2)").IsRequired();

        builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);
    }
}
