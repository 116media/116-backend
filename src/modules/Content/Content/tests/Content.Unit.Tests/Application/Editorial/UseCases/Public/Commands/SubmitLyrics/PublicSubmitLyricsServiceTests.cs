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

    private static PublicSubmitLyricsCommand Command(Guid userId)
    {
        return new PublicSubmitLyricsCommand("Song", null, "Text", "en", userId);
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
        _lyricsRepositoryMock.SetupGetBySlug(It.IsAny<string>(), null);

        // Act
        LyricsEntity lyrics = await _service.CreateForArtistAsync(Command(userId), artist, CancellationToken.None);

        // Assert
        lyrics.ArtistId.Should().Be(artist.Id);
        lyrics.ArtistName.Should().Be(artist.Name);
        lyrics.AuthorId.Should().Be(userId);
        _lyricsRepositoryMock.VerifyAddCalled();
    }
}
