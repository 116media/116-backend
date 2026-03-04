using _116.Content.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _116.Content.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the <see cref="PackageSlotEntity" />.
/// Defines the table structure, constraints, and relationships for package slots.
/// </summary>
public class PackageSlotConfiguration : IEntityTypeConfiguration<PackageSlotEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<PackageSlotEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.IsRequired).IsRequired();

        builder.Property(x => x.Quantity).IsRequired();

        builder.Property(x => x.CategoryId).IsRequired(false);

        builder
            .HasOne(x => x.Package)
            .WithMany(p => p.Slots)
            .HasForeignKey(x => x.PackageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.Category)
            .WithMany(c => c.PackageSlots)
            .HasForeignKey(x => x.CategoryId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
