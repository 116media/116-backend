using _116.Content.Application.Editorial.UseCases.Public.Queries.GetArtistArticles;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Pagination;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetArtistArticles;

/// <summary>
/// Unit tests for <see cref="PublicGetArtistArticlesHandler"/>.
/// </summary>
public class PublicGetArtistArticlesHandlerTests : BaseContentHandlerTest
{
    private static readonly Guid CategoryId = Guid.NewGuid();

    private readonly Mock<IArtistRepository> _artistRepositoryMock;
    private readonly Mock<IArticleRepository> _articleRepositoryMock;
    private readonly PublicGetArtistArticlesHandler _handler;

    public PublicGetArtistArticlesHandlerTests()
    {
        _artistRepositoryMock = MockArtistRepository.Create();
        _articleRepositoryMock = MockArticleRepository.Create();
        Mock<IFileRepository> fileRepositoryMock = MockFileRepository.Create();
        _handler = new PublicGetArtistArticlesHandler(
            _artistRepositoryMock.Object,
            _articleRepositoryMock.Object,
            Mapper,
            fileRepositoryMock.Object,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    [Fact]
    public async Task Handle_WhenArtistNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        _artistRepositoryMock.SetupGetBySlug("missing-artist", null);
        var query = new PublicGetArtistArticlesQuery("missing-artist", new PaginatedRequest(0, 10));

        // Act
        Func<Task> act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _articleRepositoryMock.Verify(
            r =>
                r.GetPublishedByArtistAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_WhenArtistFound_ShouldReturnItsArticlesInThePageEnvelope()
    {
        // Arrange
        ArtistEntity artist = ArtistFactory.CreateWithSlug("fally-ipupa");
        List<ArticleEntity> articles = ArticleFactory.CreateMany(CategoryId, 2);

        _artistRepositoryMock.SetupGetBySlug("fally-ipupa", artist);
        _articleRepositoryMock
            .Setup(r => r.GetPublishedByArtistAsync(artist.Id, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((articles, 12));

        var query = new PublicGetArtistArticlesQuery("fally-ipupa", new PaginatedRequest(0, 10));

        // Act
        PublicGetArtistArticlesResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Articles.PageIndex.Should().Be(0);
        result.Articles.PageSize.Should().Be(10);
        result.Articles.Count.Should().Be(12);
        result.Articles.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ShouldTranslateTheZeroBasedPageToTheRepositoryOneBasedPage()
    {
        // Arrange
        ArtistEntity artist = ArtistFactory.CreateWithSlug("fally-ipupa");
        _artistRepositoryMock.SetupGetBySlug("fally-ipupa", artist);
        _articleRepositoryMock
            .Setup(r =>
                r.GetPublishedByArtistAsync(artist.Id, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(([], 0));

        var query = new PublicGetArtistArticlesQuery("fally-ipupa", new PaginatedRequest(2, 5));

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _articleRepositoryMock.Verify(
            r => r.GetPublishedByArtistAsync(artist.Id, 3, 5, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
