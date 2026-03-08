using _116.Content.Domain.Constants;
using _116.Content.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _116.Content.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the <see cref="TagEntity" />.
/// Defines the table structure, constraints, and indexes for tags.
/// </summary>
public class TagConfiguration : IEntityTypeConfiguration<TagEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<TagEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(ContentConstants.MaxTagNameLength).IsRequired();

        builder.Property(x => x.Slug).HasMaxLength(ContentConstants.MaxTagSlugLength).IsRequired();

        builder.HasIndex(x => x.Slug).IsUnique();
        builder.HasIndex(x => x.Name).IsUnique();
    }
}
