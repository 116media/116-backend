using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Editorial.UseCases.Public.Commands.SubmitLyrics;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Mocks.Repositories;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Editorial.UseCases.Public.Commands.SubmitLyrics;

/// <summary>
/// Unit tests for <see cref="PublicSubmitLyricsService"/>: the owned-artist lookup and the direct
/// publication with its slug and category gates.
/// </summary>
public class PublicSubmitLyricsServiceTests
{
    private readonly Mock<IArtistRepository> _artistRepositoryMock = MockArtistRepository.Create();
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock = MockCategoryRepository.Create();
    private readonly Mock<ILyricsRepository> _lyricsRepositoryMock = MockLyricsRepository.Create();
    private readonly PublicSubmitLyricsService _service;

    public PublicSubmitLyricsServiceTests()
    {
        _service = new PublicSubmitLyricsService(
            _artistRepositoryMock.Object,
            _categoryRepositoryMock.Object,
            _lyricsRepositoryMock.Object,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    private static PublicSubmitLyricsCommand Command(Guid userId, string? slug = "song-slug")
    {
        return new PublicSubmitLyricsCommand("Song", null, "Text", "en", slug, userId);
    }

    [Fact]
    public async Task FindOwnedArtistAsync_ShouldLookTheArtistUpByUserIdOnly()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        ArtistEntity artist = ArtistFactory.Create();
        _artistRepositoryMock.SetupGetByUserId(userId, artist);

        // Act
        ArtistEntity? result = await _service.FindOwnedArtistAsync(userId, CancellationToken.None);

        // Assert
        result.Should().BeSameAs(artist);
    }

    [Fact]
    public async Task CreateForArtistAsync_ShouldStagePublishedLyricsLinkedToTheArtist()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        ArtistEntity artist = ArtistFactory.Create();
        CategoryEntity category = CategoryFactory.CreateDefaultForLyrics(Guid.NewGuid());
        _categoryRepositoryMock.SetupGetDefaultLyricsCategory(category);
        _lyricsRepositoryMock.SetupGetBySlug("song-slug", null);

        // Act
        LyricsEntity lyrics = await _service.CreateForArtistAsync(Command(userId), artist, CancellationToken.None);

        // Assert
        lyrics.ArtistId.Should().Be(artist.Id);
        lyrics.ArtistName.Should().Be(artist.Name);
        lyrics.AuthorId.Should().Be(userId);
        _lyricsRepositoryMock.VerifyAddCalled();
    }

    [Fact]
    public async Task CreateForArtistAsync_WithoutASlug_ShouldThrowBadRequestException()
    {
        // Act
        Func<Task> act = async () =>
            await _service.CreateForArtistAsync(
                Command(Guid.NewGuid(), slug: null),
                ArtistFactory.Create(),
                CancellationToken.None
            );

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task CreateForArtistAsync_WhenTheSlugIsTaken_ShouldThrowConflictException()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.CreateDefaultForLyrics(Guid.NewGuid());
        _categoryRepositoryMock.SetupGetDefaultLyricsCategory(category);
        _lyricsRepositoryMock.SetupGetBySlug("song-slug", LyricsFactory.Create(category.Id));

        // Act
        Func<Task> act = async () =>
            await _service.CreateForArtistAsync(
                Command(Guid.NewGuid()),
                ArtistFactory.Create(),
                CancellationToken.None
            );

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }
}
