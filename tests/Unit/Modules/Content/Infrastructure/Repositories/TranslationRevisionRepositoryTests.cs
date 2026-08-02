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
/// Unit tests for <see cref="TranslationRevisionRepository"/>.
/// </summary>
public class TranslationRevisionRepositoryTests : IDisposable
{
    private const string ProposedText = "Texto de traducción propuesto.";

    private readonly ContentDbContext _context;
    private readonly TranslationRevisionRepository _repository;
    private readonly Guid _translationId = Guid.NewGuid();

    public TranslationRevisionRepositoryTests()
    {
        DbContextOptions<ContentDbContext> options = new DbContextOptionsBuilder<ContentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ContentDbContext(options);
        _repository = new TranslationRevisionRepository(_context);
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
        LyricsTranslationRevisionEntity revision = LyricsTranslationRevisionFactory.Create(_translationId);
        _context.LyricsTranslationRevisions.Add(revision);
        await _context.SaveChangesAsync();

        // Act
        LyricsTranslationRevisionEntity? result = await _repository.GetByIdAsync(revision.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(revision.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenRevisionDoesNotExist_ShouldReturnNull()
    {
        // Act
        LyricsTranslationRevisionEntity? result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetByIdOrThrowAsync Tests

    [Fact]
    public async Task GetByIdOrThrowAsync_WhenRevisionExists_ShouldReturnRevision()
    {
        // Arrange
        LyricsTranslationRevisionEntity revision = LyricsTranslationRevisionFactory.Create(_translationId);
        _context.LyricsTranslationRevisions.Add(revision);
        await _context.SaveChangesAsync();

        // Act
        LyricsTranslationRevisionEntity result = await _repository.GetByIdOrThrowAsync(revision.Id);

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

    #region GetAllByTranslationIdAsync Tests

    [Fact]
    public async Task GetAllByTranslationIdAsync_ShouldReturnOnlyRevisionsOfThatTranslation()
    {
        // Arrange
        LyricsTranslationRevisionEntity pending = LyricsTranslationRevisionFactory.Create(_translationId);
        LyricsTranslationRevisionEntity rejected = LyricsTranslationRevisionFactory.CreateRejected(
            _translationId,
            Guid.NewGuid()
        );
        _context.LyricsTranslationRevisions.AddRange(
            pending,
            rejected,
            LyricsTranslationRevisionFactory.Create(Guid.NewGuid())
        );
        await _context.SaveChangesAsync();

        // Act
        IReadOnlyList<LyricsTranslationRevisionEntity> result = await _repository.GetAllByTranslationIdAsync(
            _translationId
        );

        // Assert
        result.Should().HaveCount(2);
        result.Select(revision => revision.Id).Should().BeEquivalentTo([pending.Id, rejected.Id]);
    }

    [Fact]
    public async Task GetAllByTranslationIdAsync_WhenNoRevisions_ShouldReturnEmptyList()
    {
        // Act
        IReadOnlyList<LyricsTranslationRevisionEntity> result = await _repository.GetAllByTranslationIdAsync(
            Guid.NewGuid()
        );

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetAcceptedButUnappliedAsync Tests

    [Fact]
    public async Task GetAcceptedButUnappliedAsync_ShouldReturnOnlyAcceptedRevisionsNotYetAppliedToTheirTranslation()
    {
        // Arrange — one accepted revision whose text differs from its translation (unapplied),
        // one accepted revision already applied, and one still pending
        var lyricsId = Guid.NewGuid();
        LyricsTranslationEntity unappliedTranslation = LyricsTranslationFactory.CreateWithText(
            lyricsId,
            "es",
            "Texto original."
        );
        LyricsTranslationEntity appliedTranslation = LyricsTranslationFactory.CreateWithText(
            lyricsId,
            "fr",
            ProposedText
        );
        LyricsTranslationEntity pendingTranslation = LyricsTranslationFactory.CreateWithText(
            lyricsId,
            "de",
            "Texto original."
        );
        _context.LyricsTranslations.AddRange(unappliedTranslation, appliedTranslation, pendingTranslation);

        LyricsTranslationRevisionEntity unapplied = LyricsTranslationRevisionFactory.CreateAutoAccepted(
            unappliedTranslation.Id
        );
        _context.LyricsTranslationRevisions.AddRange(
            unapplied,
            LyricsTranslationRevisionFactory.CreateAutoAccepted(appliedTranslation.Id),
            LyricsTranslationRevisionFactory.Create(pendingTranslation.Id)
        );
        await _context.SaveChangesAsync();

        // Act
        IReadOnlyList<LyricsTranslationRevisionEntity> result = await _repository.GetAcceptedButUnappliedAsync();

        // Assert
        result.Should().ContainSingle();
        result.Single().Id.Should().Be(unapplied.Id);
    }

    [Fact]
    public async Task GetAcceptedButUnappliedAsync_WhenNoRevisions_ShouldReturnEmptyList()
    {
        // Act
        IReadOnlyList<LyricsTranslationRevisionEntity> result = await _repository.GetAcceptedButUnappliedAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_ShouldAddRevisionToContext()
    {
        // Arrange
        LyricsTranslationRevisionEntity revision = LyricsTranslationRevisionFactory.Create(_translationId);

        // Act
        await _repository.AddAsync(revision);

        // Assert
        _context.Entry(revision).State.Should().Be(EntityState.Added);

        await _context.SaveChangesAsync();
        LyricsTranslationRevisionEntity? saved = await _context.LyricsTranslationRevisions.FirstOrDefaultAsync(r =>
            r.Id == revision.Id
        );
        saved.Should().NotBeNull();
        saved.ProposedText.Should().Be(revision.ProposedText);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_ShouldMarkRevisionAsModified()
    {
        // Arrange
        LyricsTranslationRevisionEntity revision = LyricsTranslationRevisionFactory.Create(_translationId);
        _context.LyricsTranslationRevisions.Add(revision);
        await _context.SaveChangesAsync();

        // Act
        _repository.Update(revision);

        // Assert
        _context.Entry(revision).State.Should().Be(EntityState.Modified);
    }

    #endregion
}
