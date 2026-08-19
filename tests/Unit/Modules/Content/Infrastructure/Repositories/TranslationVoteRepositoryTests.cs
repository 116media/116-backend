using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Content.Infrastructure.Repositories;
using _116.Tests.Fixtures.Factories.Content;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Infrastructure.Repositories;

/// <summary>
/// Unit tests for <see cref="TranslationVoteRepository" /> covering the
/// one-vote-per-user lookup (TranslationVoteByRevisionAndUserSpecification)
/// and the net approval tally (TranslationVoteByRevisionIdSpecification).
/// </summary>
public class TranslationVoteRepositoryTests : IDisposable
{
    private readonly ContentDbContext _context;
    private readonly TranslationVoteRepository _repository;

    public TranslationVoteRepositoryTests()
    {
        DbContextOptions<ContentDbContext> options = new DbContextOptionsBuilder<ContentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ContentDbContext(options);
        _repository = new TranslationVoteRepository(_context);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task AddAsync_ShouldPersistTheVote()
    {
        // Arrange
        LyricsTranslationVoteEntity vote = LyricsTranslationVoteFactory.CreateApprove(Guid.NewGuid());

        // Act
        await _repository.AddAsync(vote);
        await _context.SaveChangesAsync();

        // Assert
        (await _context.LyricsTranslationVotes.FindAsync(vote.Id))
            .Should()
            .NotBeNull();
    }

    [Fact]
    public async Task HasVotedAsync_ShouldOnlyMatchTheExactRevisionAndUserPair()
    {
        // Arrange
        var revisionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _context.LyricsTranslationVotes.Add(LyricsTranslationVoteFactory.CreateApprove(revisionId, userId));
        await _context.SaveChangesAsync();

        // Act
        bool samePair = await _repository.HasVotedAsync(revisionId, userId);
        bool otherUser = await _repository.HasVotedAsync(revisionId, Guid.NewGuid());
        bool otherRevision = await _repository.HasVotedAsync(Guid.NewGuid(), userId);

        // Assert
        samePair.Should().BeTrue();
        otherUser.Should().BeFalse();
        otherRevision.Should().BeFalse();
    }

    [Fact]
    public async Task GetNetApprovalsAsync_ShouldCountApprovalsMinusRejections()
    {
        // Arrange
        var revisionId = Guid.NewGuid();
        _context.LyricsTranslationVotes.Add(LyricsTranslationVoteFactory.CreateApprove(revisionId));
        _context.LyricsTranslationVotes.Add(LyricsTranslationVoteFactory.CreateApprove(revisionId));
        _context.LyricsTranslationVotes.Add(LyricsTranslationVoteFactory.CreateReject(revisionId));
        _context.LyricsTranslationVotes.Add(LyricsTranslationVoteFactory.CreateApprove(Guid.NewGuid()));
        await _context.SaveChangesAsync();

        // Act
        int net = await _repository.GetNetApprovalsAsync(revisionId);

        // Assert — 2 approvals - 1 rejection, blind to the other revision's vote
        net.Should().Be(1);
    }

    [Fact]
    public async Task GetNetApprovalsAsync_WithNoVotes_ShouldReturnZero()
    {
        // Act
        int net = await _repository.GetNetApprovalsAsync(Guid.NewGuid());

        // Assert
        net.Should().Be(0);
    }
}
