using _116.Mailer.Domain.Entities;
using _116.Mailer.Infrastructure.Persistence;
using _116.Mailer.Infrastructure.Repositories;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace _116.Unit.Tests.Modules.Mailer.Infrastructure.Repositories;

/// <summary>
/// Unit tests for <see cref="OutboxEmailRepository" />. The skip-locked batch
/// claim is raw SQL and only runs against a relational provider, so it is
/// covered by the integration suite; unit owns the enqueue path.
/// </summary>
public class OutboxEmailRepositoryTests
{
    private readonly MailerDbContext _context;
    private readonly OutboxEmailRepository _repository;

    public OutboxEmailRepositoryTests()
    {
        DbContextOptions<MailerDbContext> options = new DbContextOptionsBuilder<MailerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new MailerDbContext(options);
        _repository = new OutboxEmailRepository(_context);
    }

    [Fact]
    public async Task AddAsync_ShouldPersistThePendingEmail()
    {
        // Arrange
        var email = OutboxEmailEntity.Enqueue(
            Guid.NewGuid(),
            recipientAddress: "fan@example.com",
            recipientName: "Fan",
            subject: "subject",
            htmlBody: "<p>body</p>",
            textBody: "body",
            template: "NewsletterWelcome",
            now: DateTime.UtcNow
        );

        // Act
        await _repository.AddAsync(email, CancellationToken.None);
        await _context.SaveChangesAsync();

        // Assert
        (await _context.OutboxEmails.FindAsync(email.Id))
            .Should()
            .NotBeNull();
    }
}
