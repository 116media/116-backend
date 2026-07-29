using _116.Mailer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _116.Mailer.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the <see cref="NewsletterSubscriberEntity" />.
/// Defines the table structure and the unique email and token lookups.
/// </summary>
public class NewsletterSubscriberConfiguration : IEntityTypeConfiguration<NewsletterSubscriberEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<NewsletterSubscriberEntity> builder)
    {
        builder.ToTable("newsletter_subscribers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email).HasMaxLength(320).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(x => x.ConfirmationToken).HasMaxLength(64).IsRequired();
        builder.Property(x => x.UnsubscribeToken).HasMaxLength(64).IsRequired();

        builder.HasIndex(x => x.Email).IsUnique();
        builder.HasIndex(x => x.ConfirmationToken).IsUnique();
        builder.HasIndex(x => x.UnsubscribeToken).IsUnique();
    }
}
