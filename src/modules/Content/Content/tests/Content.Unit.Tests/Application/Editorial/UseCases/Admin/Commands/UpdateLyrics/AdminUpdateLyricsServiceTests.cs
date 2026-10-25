using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyrics;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Mocks.Repositories;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Editorial.UseCases.Admin.Commands.UpdateLyrics;

/// <summary>
/// Unit tests for <see cref="AdminUpdateLyricsService"/>: the category and slug gates and the verbs applied.
/// </summary>
public class AdminUpdateLyricsServiceTests
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock = MockCategoryRepository.Create();
    private readonly Mock<ILyricsRepository> _lyricsRepositoryMock = MockLyricsRepository.Create();
    private readonly Mock<IVideoRepository> _videoRepositoryMock = MockVideoRepository.Create();
    private readonly AdminUpdateLyricsService _service;

    public AdminUpdateLyricsServiceTests()
    {
        _service = new AdminUpdateLyricsService(
            _categoryRepositoryMock.Object,
            _lyricsRepositoryMock.Object,
            _videoRepositoryMock.Object
        );
    }

    private static AdminUpdateLyricsCommand Command(LyricsEntity lyrics)
    {
        return new AdminUpdateLyricsCommand(
            lyrics.Id.ToString(),
            lyrics.CategoryId,
            "New title",
            "New artist",
            "New text",
            "fr",
            null,
            null,
            null
        );
    }

    [Fact]
    public async Task UpdateAsync_ShouldApplyTheVerbs()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(Guid.NewGuid());
        LyricsEntity lyrics = LyricsFactory.CreateWithSlug(category.Id, "old-slug");
        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);
        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _lyricsRepositoryMock.SetupGetBySlug(It.IsAny<string>(), null);

        // Act
        LyricsEntity result = await _service.UpdateAsync(Command(lyrics), CancellationToken.None);

        // Assert
        result.Should().BeSameAs(lyrics);
        lyrics.SongTitle.Should().Be("New title");
        lyrics.Slug.Value.Should().Be("old-slug");
    }

    [Fact]
    public async Task UpdateAsync_WhenTheSlugIsUnchanged_ShouldNotCheckForAConflict()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(Guid.NewGuid());
        LyricsEntity lyrics = LyricsFactory.CreateWithSlug(category.Id, "same-slug");
        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);
        _categoryRepositoryMock.SetupGetByIdOrThrow(category);

        // Act
        await _service.UpdateAsync(Command(lyrics), CancellationToken.None);

        // Assert
        _lyricsRepositoryMock.Verify(
            x => x.GetBySlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }
}
