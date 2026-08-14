using _116.Content.Application.Editorial.UseCases.Public.Queries.GetArtistReleases;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Core.Application.Shared.Repositories;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Pagination;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetArtistReleases;

/// <summary>
/// Unit tests for <see cref="PublicGetArtistReleasesHandler"/>.
/// </summary>
public class PublicGetArtistReleasesHandlerTests
{
    private readonly Mock<IArtistRepository> _artistRepositoryMock;
    private readonly Mock<IAlbumRepository> _albumRepositoryMock;
    private readonly PublicGetArtistReleasesHandler _handler;

    public PublicGetArtistReleasesHandlerTests()
    {
        _artistRepositoryMock = MockArtistRepository.Create();
        _albumRepositoryMock = MockAlbumRepository.Create();
        Mock<IFileRepository> fileRepositoryMock = MockFileRepository.Create();
        _handler = new PublicGetArtistReleasesHandler(
            _artistRepositoryMock.Object,
            _albumRepositoryMock.Object,
            fileRepositoryMock.Object,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    [Fact]
    public async Task Handle_WithUnknownSlug_ShouldThrowNotFound()
    {
        // Arrange
        var query = new PublicGetArtistReleasesQuery("nobody", EnumReleaseType.Album, new PaginatedRequest(0, 12));

        // Act
        Func<Task> act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_ShouldQueryByResolvedArtistIdNeverBySlug()
    {
        // Arrange
        ArtistEntity artist = ArtistFactory.CreateWithSlug("fally-ipupa");
        _artistRepositoryMock.SetupGetBySlug("fally-ipupa", artist);
        _albumRepositoryMock
            .Setup(x => x.GetByArtistAsync(artist.Id, EnumReleaseType.Mixtape, 1, 12, It.IsAny<CancellationToken>()))
            .ReturnsAsync(([], 0));

        var query = new PublicGetArtistReleasesQuery(
            "fally-ipupa",
            EnumReleaseType.Mixtape,
            new PaginatedRequest(0, 12)
        );

        // Act
        PublicGetArtistReleasesResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert — the resolved id reached the repository with the requested type.
        result.Releases.Count.Should().Be(0);
        _albumRepositoryMock.Verify(
            x => x.GetByArtistAsync(artist.Id, EnumReleaseType.Mixtape, 1, 12, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithReleases_ShouldReturnPaginatedDtos()
    {
        // Arrange
        ArtistEntity artist = ArtistFactory.CreateWithSlug("koffi");
        _artistRepositoryMock.SetupGetBySlug("koffi", artist);
        List<AlbumEntity> albums = [AlbumFactory.Create(), AlbumFactory.Create()];
        _albumRepositoryMock
            .Setup(x => x.GetByArtistAsync(artist.Id, EnumReleaseType.Album, 1, 12, It.IsAny<CancellationToken>()))
            .ReturnsAsync((albums, 5));

        var query = new PublicGetArtistReleasesQuery("koffi", EnumReleaseType.Album, new PaginatedRequest(0, 12));

        // Act
        PublicGetArtistReleasesResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Releases.Items.Should().HaveCount(2);
        result.Releases.Count.Should().Be(5);
    }
}
