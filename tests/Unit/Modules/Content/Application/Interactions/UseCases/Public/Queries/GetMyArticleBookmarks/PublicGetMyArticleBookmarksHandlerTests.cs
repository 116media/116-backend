using _116.Content.Application.Interactions.UseCases.Public.Queries.GetMyArticleBookmarks;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Pagination;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Interactions.UseCases.Public.Queries.GetMyArticleBookmarks;

/// <summary>
/// Unit tests for <see cref="PublicGetMyArticleBookmarksHandler"/>.
/// </summary>
public class PublicGetMyArticleBookmarksHandlerTests : BaseContentHandlerTest
{
    private static readonly Guid CategoryId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();

    private readonly Mock<IArticleRepository> _articleRepositoryMock;
    private readonly PublicGetMyArticleBookmarksHandler _handler;

    public PublicGetMyArticleBookmarksHandlerTests()
    {
        _articleRepositoryMock = MockArticleRepository.Create();
        _handler = new PublicGetMyArticleBookmarksHandler(_articleRepositoryMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenUserHasBookmarks_ShouldReturnMappedPaginatedArticles()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.CreatePublished(CategoryId);
        _articleRepositoryMock.SetupGetBookmarkedArticlesAsync(new List<ArticleEntity> { article }, totalCount: 1);

        var query = new PublicGetMyArticleBookmarksQuery(
            UserId: UserId,
            PaginatedRequest: new PaginatedRequest(PageIndex: 0, PageSize: 10)
        );

        // Act
        PublicGetMyArticleBookmarksResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Articles.Items.Count().Should().Be(1);
    }

    [Fact]
    public async Task Handle_WhenUserHasNoBookmarks_ShouldReturnEmptyPage()
    {
        // Arrange
        _articleRepositoryMock.SetupGetBookmarkedArticlesAsync(new List<ArticleEntity>(), totalCount: 0);

        var query = new PublicGetMyArticleBookmarksQuery(
            UserId: UserId,
            PaginatedRequest: new PaginatedRequest(PageIndex: 0, PageSize: 10)
        );

        // Act
        PublicGetMyArticleBookmarksResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Articles.Items.Should().BeEmpty();
    }

    #endregion
}
