using _116.BuildingBlocks.Constants;
using _116.Core.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _116.Core.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for the FileEntity.
/// Defines database mapping, constraints, and indexes for file management.
/// </summary>
public class FileConfiguration : IEntityTypeConfiguration<FileEntity>
{
    /// <summary>
    /// Configures the FileEntity mapping and constraints.
    /// </summary>
    /// <param name="builder">The entity type builder used to configure the FileEntity</param>
    public void Configure(EntityTypeBuilder<FileEntity> builder)
    {
        // Primary key
        builder.HasKey(f => f.Id);

        // Properties configuration
        builder.Property(f => f.FileName)
            .HasMaxLength(FileConstants.MaxFileNameLength)
            .IsRequired();

        builder.Property(f => f.OriginalFileName)
            .HasMaxLength(FileConstants.MaxOriginalFileNameLength)
            .IsRequired();

        builder.Property(f => f.MimeType)
            .HasMaxLength(FileConstants.MaxMimeTypeLength)
            .IsRequired();

        builder.Property(f => f.StorageUrl)
            .HasMaxLength(FileConstants.MaxStorageUrlLength)
            .IsRequired();

        builder.Property(f => f.SizeInBytes).IsRequired();

        builder.Property(f => f.IsDeleted)
            .HasDefaultValue(FileConstants.DefaultIsDeleted);

        builder.Property(f => f.DeletedAt).IsRequired(false);

        // Indexes
        builder.HasIndex(f => f.FileName).IsUnique();

        builder.HasIndex(f => f.IsDeleted);
    }
}
