using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Content.Infrastructure.Repositories;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Infrastructure.Repositories;

/// <summary>
/// Unit tests for <see cref="LyricsSubmissionRepository"/>.
/// </summary>
public class LyricsSubmissionRepositoryTests : IDisposable
{
    private readonly ContentDbContext _context;
    private readonly LyricsSubmissionRepository _repository;

    public LyricsSubmissionRepositoryTests()
    {
        DbContextOptions<ContentDbContext> options = new DbContextOptionsBuilder<ContentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ContentDbContext(options);
        _repository = new LyricsSubmissionRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenSubmissionExists_ShouldReturnSubmission()
    {
        // Arrange
        LyricsSubmissionEntity submission = LyricsSubmissionFactory.Create();
        _context.LyricsSubmissions.Add(submission);
        await _context.SaveChangesAsync();

        // Act
        LyricsSubmissionEntity? result = await _repository.GetByIdAsync(submission.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(submission.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenSubmissionDoesNotExist_ShouldReturnNull()
    {
        // Act
        LyricsSubmissionEntity? result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetByIdOrThrowAsync Tests

    [Fact]
    public async Task GetByIdOrThrowAsync_WhenSubmissionExists_ShouldReturnSubmission()
    {
        // Arrange
        LyricsSubmissionEntity submission = LyricsSubmissionFactory.Create();
        _context.LyricsSubmissions.Add(submission);
        await _context.SaveChangesAsync();

        // Act
        LyricsSubmissionEntity result = await _repository.GetByIdOrThrowAsync(submission.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(submission.Id);
    }

    [Fact]
    public async Task GetByIdOrThrowAsync_WhenSubmissionDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        Func<Task> act = async () => await _repository.GetByIdOrThrowAsync(id);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WithNoStatusFilter_ShouldReturnAllSubmissions()
    {
        // Arrange
        _context.LyricsSubmissions.AddRange(
            LyricsSubmissionFactory.Create(),
            LyricsSubmissionFactory.Create(),
            LyricsSubmissionFactory.CreateRejected(Guid.NewGuid())
        );
        await _context.SaveChangesAsync();

        // Act
        (List<LyricsSubmissionEntity> submissions, int totalCount) = await _repository.GetAllAsync(
            page: 1,
            pageSize: 10,
            status: null
        );

        // Assert
        submissions.Should().HaveCount(3);
        totalCount.Should().Be(3);
    }

    [Fact]
    public async Task GetAllAsync_WithStatusFilter_ShouldReturnOnlyMatchingSubmissions()
    {
        // Arrange
        LyricsSubmissionEntity rejected = LyricsSubmissionFactory.CreateRejected(Guid.NewGuid());
        _context.LyricsSubmissions.AddRange(LyricsSubmissionFactory.Create(), rejected);
        await _context.SaveChangesAsync();

        // Act
        (List<LyricsSubmissionEntity> submissions, int totalCount) = await _repository.GetAllAsync(
            page: 1,
            pageSize: 10,
            status: EnumSubmissionStatus.Rejected
        );

        // Assert
        totalCount.Should().Be(1);
        submissions.Should().ContainSingle();
        submissions.Single().Id.Should().Be(rejected.Id);
    }

    [Fact]
    public async Task GetAllAsync_WithPagination_ShouldReturnRequestedPage()
    {
        // Arrange
        _context.LyricsSubmissions.AddRange(
            LyricsSubmissionFactory.Create(),
            LyricsSubmissionFactory.Create(),
            LyricsSubmissionFactory.Create()
        );
        await _context.SaveChangesAsync();

        // Act
        (List<LyricsSubmissionEntity> submissions, int totalCount) = await _repository.GetAllAsync(
            page: 2,
            pageSize: 2,
            status: null
        );

        // Assert
        submissions.Should().ContainSingle();
        totalCount.Should().Be(3);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoSubmissions_ShouldReturnEmptyList()
    {
        // Act
        (List<LyricsSubmissionEntity> submissions, int totalCount) = await _repository.GetAllAsync(
            page: 1,
            pageSize: 10,
            status: null
        );

        // Assert
        submissions.Should().BeEmpty();
        totalCount.Should().Be(0);
    }

    #endregion

    #region GetPendingWithMatchingLyricsAsync Tests

    [Fact]
    public async Task GetPendingWithMatchingLyricsAsync_ShouldReturnOnlyPendingSubmissionsWithAPublishedMatch()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        LyricsSubmissionEntity matching = LyricsSubmissionFactory.Create("Mario", "Franco");
        LyricsSubmissionEntity unmatched = LyricsSubmissionFactory.Create("Unknown Song", "Unknown Artist");
        LyricsSubmissionEntity decided = LyricsSubmissionFactory.CreateRejected(Guid.NewGuid());

        _context.LyricsSubmissions.AddRange(matching, unmatched, decided);
        _context.Lyrics.Add(LyricsFactory.Create(categoryId, "Mario", "Franco"));
        await _context.SaveChangesAsync();

        // Act
        IReadOnlyList<LyricsSubmissionEntity> result = await _repository.GetPendingWithMatchingLyricsAsync();

        // Assert
        result.Should().ContainSingle();
        result.Single().Id.Should().Be(matching.Id);
    }

    [Fact]
    public async Task GetPendingWithMatchingLyricsAsync_WhenNoLyricsMatch_ShouldReturnEmptyList()
    {
        // Arrange
        _context.LyricsSubmissions.Add(LyricsSubmissionFactory.Create("Mario", "Franco"));
        await _context.SaveChangesAsync();

        // Act
        IReadOnlyList<LyricsSubmissionEntity> result = await _repository.GetPendingWithMatchingLyricsAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_ShouldAddSubmissionToContext()
    {
        // Arrange
        LyricsSubmissionEntity submission = LyricsSubmissionFactory.Create();

        // Act
        await _repository.AddAsync(submission);

        // Assert
        _context.Entry(submission).State.Should().Be(EntityState.Added);

        await _context.SaveChangesAsync();
        LyricsSubmissionEntity? saved = await _context.LyricsSubmissions.FirstOrDefaultAsync(s =>
            s.Id == submission.Id
        );
        saved.Should().NotBeNull();
        saved.SongTitle.Should().Be(submission.SongTitle);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_ShouldMarkSubmissionAsModified()
    {
        // Arrange
        LyricsSubmissionEntity submission = LyricsSubmissionFactory.Create();
        _context.LyricsSubmissions.Add(submission);
        await _context.SaveChangesAsync();

        // Act
        _repository.Update(submission);

        // Assert
        _context.Entry(submission).State.Should().Be(EntityState.Modified);
    }

    #endregion
}
