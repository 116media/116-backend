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
/// Unit tests for <see cref="TranslationRepository"/>.
/// </summary>
public class TranslationRepositoryTests : IDisposable
{
    private readonly ContentDbContext _context;
    private readonly TranslationRepository _repository;
    private readonly Guid _lyricsId = Guid.NewGuid();

    public TranslationRepositoryTests()
    {
        DbContextOptions<ContentDbContext> options = new DbContextOptionsBuilder<ContentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ContentDbContext(options);
        _repository = new TranslationRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenTranslationExists_ShouldReturnTranslation()
    {
        // Arrange
        LyricsTranslationEntity translation = LyricsTranslationFactory.Create(_lyricsId);
        _context.LyricsTranslations.Add(translation);
        await _context.SaveChangesAsync();

        // Act
        LyricsTranslationEntity? result = await _repository.GetByIdAsync(translation.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(translation.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenTranslationDoesNotExist_ShouldReturnNull()
    {
        // Act
        LyricsTranslationEntity? result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetByIdOrThrowAsync Tests

    [Fact]
    public async Task GetByIdOrThrowAsync_WhenTranslationExists_ShouldReturnTranslation()
    {
        // Arrange
        LyricsTranslationEntity translation = LyricsTranslationFactory.Create(_lyricsId);
        _context.LyricsTranslations.Add(translation);
        await _context.SaveChangesAsync();

        // Act
        LyricsTranslationEntity result = await _repository.GetByIdOrThrowAsync(translation.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(translation.Id);
    }

    [Fact]
    public async Task GetByIdOrThrowAsync_WhenTranslationDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        Func<Task> act = async () => await _repository.GetByIdOrThrowAsync(id);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region GetByLyricsAndLanguageAsync Tests

    [Fact]
    public async Task GetByLyricsAndLanguageAsync_WhenPairExists_ShouldReturnTranslation()
    {
        // Arrange
        LyricsTranslationEntity spanish = LyricsTranslationFactory.Create(_lyricsId, "es");
        LyricsTranslationEntity french = LyricsTranslationFactory.Create(_lyricsId, "fr");
        _context.LyricsTranslations.AddRange(spanish, french);
        await _context.SaveChangesAsync();

        // Act
        LyricsTranslationEntity? result = await _repository.GetByLyricsAndLanguageAsync(_lyricsId, "fr");

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(french.Id);
    }

    [Fact]
    public async Task GetByLyricsAndLanguageAsync_WhenPairDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        _context.LyricsTranslations.Add(LyricsTranslationFactory.Create(_lyricsId, "es"));
        await _context.SaveChangesAsync();

        // Act
        LyricsTranslationEntity? result = await _repository.GetByLyricsAndLanguageAsync(_lyricsId, "de");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetAllByLyricsIdAsync Tests

    [Fact]
    public async Task GetAllByLyricsIdAsync_ShouldReturnEveryTranslationOfTheLyricsPage()
    {
        // Arrange
        _context.LyricsTranslations.AddRange(
            LyricsTranslationFactory.Create(_lyricsId, "es"),
            LyricsTranslationFactory.Create(_lyricsId, "fr"),
            LyricsTranslationFactory.Create(Guid.NewGuid(), "es")
        );
        await _context.SaveChangesAsync();

        // Act
        IReadOnlyList<LyricsTranslationEntity> result = await _repository.GetAllByLyricsIdAsync(_lyricsId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(translation => translation.LyricsId.Should().Be(_lyricsId));
    }

    [Fact]
    public async Task GetAllByLyricsIdAsync_WhenNoTranslations_ShouldReturnEmptyList()
    {
        // Act
        IReadOnlyList<LyricsTranslationEntity> result = await _repository.GetAllByLyricsIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_ShouldAddTranslationToContext()
    {
        // Arrange
        LyricsTranslationEntity translation = LyricsTranslationFactory.Create(_lyricsId);

        // Act
        await _repository.AddAsync(translation);

        // Assert
        _context.Entry(translation).State.Should().Be(EntityState.Added);

        await _context.SaveChangesAsync();
        LyricsTranslationEntity? saved = await _context.LyricsTranslations.FirstOrDefaultAsync(t =>
            t.Id == translation.Id
        );
        saved.Should().NotBeNull();
        saved.Text.Should().Be(translation.Text);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_ShouldMarkTranslationAsModified()
    {
        // Arrange
        LyricsTranslationEntity translation = LyricsTranslationFactory.Create(_lyricsId);
        _context.LyricsTranslations.Add(translation);
        await _context.SaveChangesAsync();

        // Act
        _repository.Update(translation);

        // Assert
        _context.Entry(translation).State.Should().Be(EntityState.Modified);
    }

    #endregion
}
