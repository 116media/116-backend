using System.Reflection;
using _116.Mailer.Domain.Constants;
using _116.Mailer.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace _116.Mailer.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core database context for the Mailer module.
/// Manages the outbox and newsletter entities under the "mailer" schema.
/// </summary>
/// <param name="options">The options to configure this database context.</param>
public class MailerDbContext(DbContextOptions<MailerDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Gets the DbSet for outbox email entities.
    /// </summary>
    public DbSet<OutboxEmailEntity> OutboxEmails => Set<OutboxEmailEntity>();

    /// <summary>
    /// Gets the DbSet for newsletter subscriber entities.
    /// </summary>
    public DbSet<NewsletterSubscriberEntity> NewsletterSubscribers => Set<NewsletterSubscriberEntity>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(MailerConstants.SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
