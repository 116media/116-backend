using _116.Content.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _116.Content.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the <see cref="ContentPaymentEntity" />.
/// Defines the table structure, constraints, and relationships for order payment records.
/// </summary>
public class ContentPaymentConfiguration : IEntityTypeConfiguration<ContentPaymentEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ContentPaymentEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.AmountUsd).HasColumnType("numeric(10,2)").IsRequired();

        builder.Property(x => x.Status).IsRequired();

        builder.Property(x => x.PaymentMethod).IsRequired(false);

        builder.Property(x => x.PaymentProofFileId).IsRequired(false);

        builder.Property(x => x.VerifiedById).IsRequired(false);

        builder.Property(x => x.VerifiedAt).IsRequired(false);

        builder.Property(x => x.ReceiptUrl).HasMaxLength(500).IsRequired(false);

        builder.Property(x => x.Notes).HasMaxLength(1000).IsRequired(false);

        builder
            .HasOne(x => x.Order)
            .WithOne(o => o.Payment)
            .HasForeignKey<ContentPaymentEntity>(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
