using _116.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _116.Identity.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for the UserTokenStateEntity record.
/// Defines the 1:1 mapping with the user (the primary key is the user id) and the marker columns.
/// </summary>
public class UserTokenStateConfiguration : IEntityTypeConfiguration<UserTokenStateEntity>
{
    /// <summary>
    /// Configures the UserTokenStateEntity mapping and its 1:1 relationship with the user.
    /// </summary>
    /// <param name="builder">The entity type builder used to configure the UserTokenStateEntity</param>
    public void Configure(EntityTypeBuilder<UserTokenStateEntity> builder)
    {
        builder.ToTable(name: "user_token_state");

        // Primary key doubles as the FK to the owning user (1:1)
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName(name: "user_id");

        // Properties configuration
        builder.Property(s => s.SecurityStamp).IsRequired();
        builder.Property(s => s.TokenVersion).IsRequired().HasDefaultValue(0L);

        // Relationships
        builder
            .HasOne<UserEntity>()
            .WithOne()
            .HasForeignKey<UserTokenStateEntity>(s => s.Id)
            .OnDelete(deleteBehavior: DeleteBehavior.Cascade);
    }
}
