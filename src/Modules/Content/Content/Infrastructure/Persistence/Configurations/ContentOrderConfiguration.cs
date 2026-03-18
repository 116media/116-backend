using _116.Content.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _116.Content.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the <see cref="ContentOrderEntity" />.
/// Defines the table structure, constraints, and relationships for content orders.
/// </summary>
public class ContentOrderConfiguration : IEntityTypeConfiguration<ContentOrderEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ContentOrderEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TotalAmountUsd).HasColumnType("numeric(10,2)").IsRequired();

        builder.Property(x => x.Status).IsRequired();

        builder.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.Package)
            .WithMany()
            .HasForeignKey(x => x.PackageId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
