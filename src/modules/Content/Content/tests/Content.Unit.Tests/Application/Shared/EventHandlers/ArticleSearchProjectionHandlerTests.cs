using _116.Content.Application.Shared.EventHandlers;
using _116.Content.Application.Shared.Search;
using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Shared.EventHandlers;

/// <summary>
/// Unit tests for <see cref="ArticleSearchProjectionHandler" />. Each transition must reach
/// the projection with the article's own content type, or the document is written against
/// the wrong key and search answers with another type's row.
/// </summary>
public class ArticleSearchProjectionHandlerTests
{
    private readonly Mock<ISearchProjection> _projectionMock = new();
    private readonly ArticleSearchProjectionHandler _handler;
    private readonly Guid _articleId = Guid.NewGuid();

    public ArticleSearchProjectionHandlerTests()
    {
        _handler = new ArticleSearchProjectionHandler(_projectionMock.Object);
    }

    [Fact]
    public async Task Handle_WhenAnArticleIsPublished_ShouldProjectIt()
    {
        // Act
        await _handler.Handle(new ArticlePublishedEvent(_articleId));

        // Assert
        _projectionMock.Verify(
            p => p.ProjectAsync(EnumCoreContentType.Article, _articleId, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WhenAnArticleIsUnpublished_ShouldReprojectRatherThanRemove()
    {
        // Act
        await _handler.Handle(new ArticleUnpublishedEvent(_articleId));

        // Assert
        _projectionMock.Verify(
            p => p.ProjectAsync(EnumCoreContentType.Article, _articleId, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _projectionMock.Verify(
            p => p.RemoveAsync(It.IsAny<EnumCoreContentType>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "an unpublished article keeps its row, hidden"
        );
    }

    [Fact]
    public async Task Handle_WhenAnArticleIsDeleted_ShouldRemoveItsDocument()
    {
        // Act
        await _handler.Handle(new ArticleDeletedEvent(_articleId, null, []));

        // Assert
        _projectionMock.Verify(
            p => p.RemoveAsync(EnumCoreContentType.Article, _articleId, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
