using _116.Mailer.Domain.Constants;
using _116.Mailer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _116.Mailer.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the <see cref="OutboxEmailEntity" />.
/// Defines the table structure and the dispatcher's scan index.
/// </summary>
public class OutboxEmailConfiguration : IEntityTypeConfiguration<OutboxEmailEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<OutboxEmailEntity> builder)
    {
        builder.ToTable("outbox_emails");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.RecipientAddress).HasMaxLength(320).IsRequired();
        builder.Property(x => x.RecipientName).HasMaxLength(200);
        builder.Property(x => x.Subject).HasMaxLength(500).IsRequired();
        builder.Property(x => x.HtmlBody).IsRequired();
        builder.Property(x => x.TextBody).IsRequired();
        builder.Property(x => x.Template).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.LastError).HasMaxLength(MailerConstants.MaxLastErrorLength);

        // The dispatcher scans "pending rows due by now, oldest first" on every
        // run; this composite index is that scan.
        builder.HasIndex(x => new { x.Status, x.NextAttemptAt });
    }
}
