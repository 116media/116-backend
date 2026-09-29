using _116.Mailer.Infrastructure.Persistence;
using _116.Mailer.Infrastructure.Repositories;
using _116.Mailer.TestData.Builders.Entities;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace _116.Mailer.Unit.Tests.Infrastructure.Repositories;

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
        var email = new OutboxEmailBuilder()
            .WithId(Guid.NewGuid())
            .WithRecipient("fan@example.com", "Fan")
            .WithContent("subject", "<p>body</p>", "body")
            .WithTemplate("NewsletterWelcome")
            .At(DateTime.UtcNow)
            .Build();

        // Act
        await _repository.AddAsync(email, CancellationToken.None);
        await _context.SaveChangesAsync();

        // Assert
        (await _context.OutboxEmails.FindAsync(email.Id))
            .Should()
            .NotBeNull();
    }
}
