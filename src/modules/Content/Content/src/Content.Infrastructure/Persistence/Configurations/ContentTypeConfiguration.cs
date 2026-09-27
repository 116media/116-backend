using _116.Content.Domain.Constants;
using _116.Content.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _116.Content.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the <see cref="ContentTypeEntity" />.
/// Defines the table structure, constraints, and indexes for content types.
/// </summary>
public class ContentTypeConfiguration : IEntityTypeConfiguration<ContentTypeEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ContentTypeEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(ContentConstants.MaxContentTypeNameLength).IsRequired();

        builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);

        builder.HasIndex(x => x.Name).IsUnique();
    }
}
