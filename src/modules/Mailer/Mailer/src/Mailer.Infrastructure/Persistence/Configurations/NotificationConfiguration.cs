using _116.Mailer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _116.Mailer.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the <see cref="NotificationEntity" />.
/// Defines the table structure, the unread-count index, and the feed index.
/// </summary>
public class NotificationConfiguration : IEntityTypeConfiguration<NotificationEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<NotificationEntity> builder)
    {
        builder.ToTable("notifications");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Body).HasMaxLength(500).IsRequired();
        builder.Property(x => x.LinkPath).HasMaxLength(300);

        // The badge counts "unread rows for this user" on every poll; this
        // composite index is that count.
        builder.HasIndex(x => new { x.UserId, x.ReadAt });

        // The feed pages "this user's rows, newest first"; this composite
        // index is that page.
        builder.HasIndex(x => new { x.UserId, x.CreatedAt }).IsDescending(false, true);
    }
}
