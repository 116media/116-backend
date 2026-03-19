using _116.Content.Application.Interactions.UseCases.Public.Queries.GetArticleComments;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Pagination;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Interactions.UseCases.Public.Queries.GetArticleComments;

/// <summary>
/// Unit tests for <see cref="PublicGetArticleCommentsHandler"/>.
/// </summary>
public class PublicGetArticleCommentsHandlerTests : BaseContentHandlerTest
{
    private static readonly Guid ArticleId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();

    private readonly Mock<IArticleRepository> _articleRepositoryMock;
    private readonly PublicGetArticleCommentsHandler _handler;

    public PublicGetArticleCommentsHandlerTests()
    {
        _articleRepositoryMock = MockArticleRepository.Create();
        _handler = new PublicGetArticleCommentsHandler(_articleRepositoryMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenArticleHasComments_ShouldReturnMappedPaginatedComments()
    {
        // Arrange
        ArticleCommentEntity comment1 = ArticleCommentFactory.Create(ArticleId, UserId);
        ArticleCommentEntity comment2 = ArticleCommentFactory.Create(ArticleId, UserId);
        _articleRepositoryMock.SetupGetCommentsAsync(
            new List<ArticleCommentEntity> { comment1, comment2 },
            totalCount: 2
        );

        var query = new PublicGetArticleCommentsQuery(
            ArticleId: ArticleId,
            PaginatedRequest: new PaginatedRequest(PageIndex: 0, PageSize: 10)
        );

        // Act
        PublicGetArticleCommentsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Comments.Items.Count().Should().Be(2);
    }

    [Fact]
    public async Task Handle_WhenNoComments_ShouldReturnEmptyPage()
    {
        // Arrange
        _articleRepositoryMock.SetupGetCommentsAsync(new List<ArticleCommentEntity>(), totalCount: 0);

        var query = new PublicGetArticleCommentsQuery(
            ArticleId: ArticleId,
            PaginatedRequest: new PaginatedRequest(PageIndex: 0, PageSize: 10)
        );

        // Act
        PublicGetArticleCommentsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Comments.Items.Should().BeEmpty();
    }

    #endregion
}
