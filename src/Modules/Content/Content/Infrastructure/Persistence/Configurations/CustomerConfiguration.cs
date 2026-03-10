using _116.Content.Domain.Constants;
using _116.Content.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _116.Content.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the <see cref="CustomerEntity" />.
/// Defines the table structure, constraints, and indexes for B2B customers.
/// </summary>
public class CustomerConfiguration : IEntityTypeConfiguration<CustomerEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<CustomerEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FullName).HasMaxLength(ContentConstants.MaxCustomerFullNameLength).IsRequired();

        builder.Property(x => x.Email).HasMaxLength(ContentConstants.MaxCustomerEmailLength).IsRequired();

        builder.Property(x => x.Phone).HasMaxLength(ContentConstants.MaxCustomerPhoneLength).IsRequired(false);

        builder.Property(x => x.Company).HasMaxLength(ContentConstants.MaxCustomerCompanyLength).IsRequired(false);

        builder.Property(x => x.Notes).HasMaxLength(ContentConstants.MaxCustomerNotesLength).IsRequired(false);

        builder.HasIndex(x => x.Email).IsUnique();
    }
}
