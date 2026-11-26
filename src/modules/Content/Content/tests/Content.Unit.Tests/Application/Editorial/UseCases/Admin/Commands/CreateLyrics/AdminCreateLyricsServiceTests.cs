using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateLyrics;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Mocks.Repositories;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Editorial.UseCases.Admin.Commands.CreateLyrics;

/// <summary>
/// Unit tests for <see cref="AdminCreateLyricsService"/>: the category, slug and video gates and
/// the staged lyrics.
/// </summary>
public class AdminCreateLyricsServiceTests
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock = MockCategoryRepository.Create();
    private readonly Mock<ILyricsRepository> _lyricsRepositoryMock = MockLyricsRepository.Create();
    private readonly Mock<IVideoRepository> _videoRepositoryMock = MockVideoRepository.Create();
    private readonly AdminCreateLyricsService _service;

    public AdminCreateLyricsServiceTests()
    {
        _service = new AdminCreateLyricsService(
            _categoryRepositoryMock.Object,
            _lyricsRepositoryMock.Object,
            _videoRepositoryMock.Object
        );
    }

    private static AdminCreateLyricsCommand Command(Guid categoryId, Guid? customerId = null)
    {
        return new AdminCreateLyricsCommand(
            categoryId,
            "Song",
            "Artist",
            "Lyrics text",
            "en",
            Guid.NewGuid(),
            null,
            customerId,
            customerId is null ? null : Guid.NewGuid()
        );
    }

    [Fact]
    public async Task CreateAsync_WithAFreeSlug_ShouldStageFreeLyrics()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(Guid.NewGuid());
        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _lyricsRepositoryMock.SetupGetBySlug(It.IsAny<string>(), null);

        // Act
        LyricsEntity lyrics = await _service.CreateAsync(Command(category.Id), CancellationToken.None);

        // Assert
        lyrics.Slug.Value.Should().Be("song");
        lyrics.CustomerId.Should().BeNull();
        _lyricsRepositoryMock.VerifyAddCalled();
    }

    [Fact]
    public async Task CreateAsync_WithACustomer_ShouldStagePaidLyrics()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(Guid.NewGuid());
        Guid customerId = Guid.NewGuid();
        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _lyricsRepositoryMock.SetupGetBySlug(It.IsAny<string>(), null);

        // Act
        LyricsEntity lyrics = await _service.CreateAsync(Command(category.Id, customerId), CancellationToken.None);

        // Assert
        lyrics.CustomerId.Should().Be(customerId);
    }

    [Fact]
    public async Task CreateAsync_WhenTheCategoryDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid categoryId = Guid.NewGuid();
        _categoryRepositoryMock.SetupGetByIdOrThrowNotFound(categoryId);

        // Act
        Func<Task> act = async () => await _service.CreateAsync(Command(categoryId), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
