using _116.Content.Application.Editorial.UseCases.Admin.Queries.GetAllArtists;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Shared.Application.Pagination;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Queries.GetAllArtists;

/// <summary>
/// Unit tests for <see cref="AdminGetAllArtistsHandler"/>.
/// </summary>
public class AdminGetAllArtistsHandlerTests
{
    private readonly Mock<IArtistRepository> _artistRepositoryMock;
    private readonly AdminGetAllArtistsHandler _handler;

    public AdminGetAllArtistsHandlerTests()
    {
        _artistRepositoryMock = MockArtistRepository.Create();
        Mock<IFileRepository> fileRepositoryMock = MockFileRepository.Create();
        _handler = new AdminGetAllArtistsHandler(_artistRepositoryMock.Object, fileRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenArtistsExist_ShouldReturnPaginatedResult()
    {
        // Arrange
        List<ArtistEntity> artists = ArtistFactory.CreateMany(3);
        _artistRepositoryMock.SetupGetAllAsync(artists, 3);
        var query = new AdminGetAllArtistsQuery(new PaginatedRequest(0, 10), null);

        // Act
        AdminGetAllArtistsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Artists.Items.Should().HaveCount(3);
        result.Artists.Count.Should().Be(3);
    }

    [Fact]
    public async Task Handle_WhenNoArtistsExist_ShouldReturnEmptyResult()
    {
        // Arrange
        var query = new AdminGetAllArtistsQuery(new PaginatedRequest(0, 10), null);

        // Act
        AdminGetAllArtistsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Artists.Items.Should().BeEmpty();
        result.Artists.Count.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldPassSearchTermThrough()
    {
        // Arrange
        var query = new AdminGetAllArtistsQuery(new PaginatedRequest(0, 10), "Fally");

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _artistRepositoryMock.Verify(
            x => x.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), "Fally", It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
