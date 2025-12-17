using _116.BuildingBlocks.Constants;
using _116.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _116.Identity.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for the UserEntity.
/// Defines database mapping, relationships, constraints, and indexes for user accounts.
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<UserEntity>
{
    /// <summary>
    /// Configures the UserEntity mapping and relationships.
    /// </summary>
    /// <param name="builder">The entity type builder used to configure the UserEntity</param>
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        // Primary key
        builder.HasKey(u => u.Id);

        // Properties configuration
        builder.Property(u => u.Email)
            .HasMaxLength(UserConstants.MaxEmailLength)
            .IsRequired(false); // Can be null for external auth providers

        builder.Property(u => u.UserName)
            .HasMaxLength(UserConstants.MaxUserNameLength)
            .IsRequired();

        builder.Property(u => u.PasswordHash)
            .IsRequired(false); // Can be null for external auth providers

        builder.Property(u => u.AuthProvider)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(u => u.IsVerified)
            .HasDefaultValue(UserConstants.DefaultIsVerified);

        builder.Property(u => u.IsActive)
            .HasDefaultValue(true);

        builder.Property(u => u.IsLoggedIn)
            .HasDefaultValue(false);

        builder.Property(u => u.LastLoginAt)
            .IsRequired(false);

        builder.Property(u => u.CountryName)
            .HasMaxLength(UserConstants.MaxCountryNameLength)
            .IsRequired(false);

        builder.Property(u => u.CountryFlagUrl)
            .HasMaxLength(UserConstants.MaxCountryFlagUrlLength)
            .IsRequired(false);

        builder.Property(u => u.CountryIsoCode)
            .HasMaxLength(UserConstants.MaxCountryIsoCodeLength)
            .IsRequired(false);

        builder.Property(u => u.CountryDialCode)
            .HasMaxLength(UserConstants.MaxCountryDialCodeLength)
            .IsRequired(false);

        builder.Property(u => u.PartialPhoneNumber)
            .HasMaxLength(UserConstants.MaxPartialPhoneNumberLength)
            .IsRequired(false);

        builder.Property(u => u.FullPhoneNumber)
            .HasMaxLength(UserConstants.MaxFullPhoneNumberLength)
            .IsRequired(false);

        builder.Property(u => u.AvatarFileId)
            .IsRequired(false);

        builder.Property(u => u.AvatarSource)
            .HasConversion<string>()
            .IsRequired();

        // Indexes
        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.HasIndex(u => u.UserName)
            .IsUnique();

        // Relationships
        builder.HasMany(u => u.UserRoles)
            .WithOne(ur => ur.User)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
