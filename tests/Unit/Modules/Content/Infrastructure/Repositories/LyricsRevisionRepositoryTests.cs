using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Content.Infrastructure.Repositories;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Infrastructure.Repositories;

/// <summary>
/// Unit tests for <see cref="LyricsRevisionRepository"/>.
/// </summary>
public class LyricsRevisionRepositoryTests : IDisposable
{
    private readonly ContentDbContext _context;
    private readonly LyricsRevisionRepository _repository;
    private readonly Guid _lyricsId = Guid.NewGuid();

    public LyricsRevisionRepositoryTests()
    {
        DbContextOptions<ContentDbContext> options = new DbContextOptionsBuilder<ContentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ContentDbContext(options);
        _repository = new LyricsRevisionRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenRevisionExists_ShouldReturnRevision()
    {
        // Arrange
        LyricsRevisionEntity revision = LyricsRevisionFactory.Create(_lyricsId);
        _context.LyricsRevisions.Add(revision);
        await _context.SaveChangesAsync();

        // Act
        LyricsRevisionEntity? result = await _repository.GetByIdAsync(revision.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(revision.Id);
        result.LyricsId.Should().Be(_lyricsId);
    }

    [Fact]
    public async Task GetByIdAsync_WhenRevisionDoesNotExist_ShouldReturnNull()
    {
        // Act
        LyricsRevisionEntity? result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetByIdOrThrowAsync Tests

    [Fact]
    public async Task GetByIdOrThrowAsync_WhenRevisionExists_ShouldReturnRevision()
    {
        // Arrange
        LyricsRevisionEntity revision = LyricsRevisionFactory.Create(_lyricsId);
        _context.LyricsRevisions.Add(revision);
        await _context.SaveChangesAsync();

        // Act
        LyricsRevisionEntity result = await _repository.GetByIdOrThrowAsync(revision.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(revision.Id);
    }

    [Fact]
    public async Task GetByIdOrThrowAsync_WhenRevisionDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        Func<Task> act = async () => await _repository.GetByIdOrThrowAsync(id);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_ShouldAddRevisionToContext()
    {
        // Arrange
        LyricsRevisionEntity revision = LyricsRevisionFactory.Create(_lyricsId);

        // Act
        await _repository.AddAsync(revision);

        // Assert
        _context.Entry(revision).State.Should().Be(EntityState.Added);

        await _context.SaveChangesAsync();
        LyricsRevisionEntity? saved = await _context.LyricsRevisions.FirstOrDefaultAsync(r => r.Id == revision.Id);
        saved.Should().NotBeNull();
        saved.ProposedText.Should().Be(revision.ProposedText);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_ShouldMarkRevisionAsModified()
    {
        // Arrange
        LyricsRevisionEntity revision = LyricsRevisionFactory.Create(_lyricsId);
        _context.LyricsRevisions.Add(revision);
        await _context.SaveChangesAsync();

        // Act
        _repository.Update(revision);

        // Assert
        _context.Entry(revision).State.Should().Be(EntityState.Modified);
    }

    #endregion
}
