using _116.Content.Domain.Constants;
using _116.Content.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _116.Content.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the <see cref="CategoryEntity" />.
/// Defines the table structure, constraints, relationships, and indexes for categories.
/// </summary>
public class CategoryConfiguration : IEntityTypeConfiguration<CategoryEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<CategoryEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(ContentConstants.MaxCategoryNameLength).IsRequired();

        builder.Property(x => x.Slug).HasMaxLength(ContentConstants.MaxCategorySlugLength).IsRequired();

        builder
            .Property(x => x.Description)
            .HasMaxLength(ContentConstants.MaxCategoryDescriptionLength)
            .IsRequired(false);

        builder.Property(x => x.IsFree).IsRequired().HasDefaultValue(false);

        builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);

        builder.HasIndex(x => x.Slug).IsUnique();

        builder
            .HasOne(x => x.ContentType)
            .WithMany()
            .HasForeignKey(x => x.ContentTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
