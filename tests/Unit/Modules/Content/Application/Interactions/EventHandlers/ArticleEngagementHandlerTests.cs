using _116.Content.Application.Interactions.EventHandlers;
using _116.Content.Application.Shared.Cache;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Interactions.EventHandlers;

/// <summary>
/// Unit tests for <see cref="ArticleEngagementHandler"/>. The counter itself is applied in SQL,
/// so these assert the delta the handler forwards and the cache eviction; the arithmetic is
/// proven against the database in the repository and workflow integration tests.
/// </summary>
public class ArticleEngagementHandlerTests
{
    private readonly Mock<IArticleInteractionRepository> _articleInteractionRepositoryMock;
    private readonly Mock<HybridCache> _cacheMock;
    private readonly ArticleEngagementHandler _handler;

    public ArticleEngagementHandlerTests()
    {
        _articleInteractionRepositoryMock = MockArticleInteractionRepository.Create();
        _cacheMock = MockHybridCache.Create();
        _handler = new ArticleEngagementHandler(
            _articleInteractionRepositoryMock.Object,
            _cacheMock.Object,
            NullLogger<ArticleEngagementHandler>.Instance
        );
    }

    [Theory]
    [InlineData(EnumEngagementKind.Like, 1)]
    [InlineData(EnumEngagementKind.Bookmark, 1)]
    [InlineData(EnumEngagementKind.Comment, 1)]
    [InlineData(EnumEngagementKind.Share, 1)]
    [InlineData(EnumEngagementKind.Like, -1)]
    [InlineData(EnumEngagementKind.Bookmark, -1)]
    [InlineData(EnumEngagementKind.Comment, -1)]
    public async Task Handle_ShouldForwardTheKindAndDeltaThenInvalidate(EnumEngagementKind kind, int delta)
    {
        // Arrange
        var articleId = Guid.NewGuid();
        _articleInteractionRepositoryMock
            .Setup(x => x.ApplyEngagementDeltaAsync(articleId, kind, delta, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(new ArticleEngagedEvent(articleId, kind, delta), CancellationToken.None);

        // Assert
        _articleInteractionRepositoryMock.Verify(
            x => x.ApplyEngagementDeltaAsync(articleId, kind, delta, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _cacheMock.VerifyRemovedByTag(ContentCacheTags.PopularArticles);
    }

    [Fact]
    public async Task Handle_ShouldNeverLoadOrTrackTheArticle()
    {
        // Arrange
        var articleId = Guid.NewGuid();
        _articleInteractionRepositoryMock
            .Setup(x =>
                x.ApplyEngagementDeltaAsync(
                    articleId,
                    It.IsAny<EnumEngagementKind>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(new ArticleEngagedEvent(articleId, EnumEngagementKind.Like, 1), CancellationToken.None);

        // Assert — the interaction repository exposes no aggregate load or update, so the
        // set-based path cannot fall back to load-then-mutate; the delta call is the whole write.
        _articleInteractionRepositoryMock.Verify(
            x => x.ApplyEngagementDeltaAsync(articleId, EnumEngagementKind.Like, 1, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithKindWithoutArticleCounter_ShouldStillInvalidate()
    {
        // Arrange
        var articleId = Guid.NewGuid();
        _articleInteractionRepositoryMock
            .Setup(x =>
                x.ApplyEngagementDeltaAsync(articleId, EnumEngagementKind.View, 1, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(0);

        // Act
        await _handler.Handle(new ArticleEngagedEvent(articleId, EnumEngagementKind.View, 1), CancellationToken.None);

        // Assert
        _cacheMock.VerifyRemovedByTag(ContentCacheTags.PopularArticles);
    }

    [Fact]
    public async Task Handle_WhenArticleMissing_ShouldStillInvalidate()
    {
        // Arrange
        // a race, not an error. The ranked list is evicted regardless.
        var articleId = Guid.NewGuid();
        _articleInteractionRepositoryMock
            .Setup(x =>
                x.ApplyEngagementDeltaAsync(articleId, EnumEngagementKind.Like, 1, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(0);

        // Act
        await _handler.Handle(new ArticleEngagedEvent(articleId, EnumEngagementKind.Like, 1), CancellationToken.None);

        // Assert
        _cacheMock.VerifyRemovedByTag(ContentCacheTags.PopularArticles);
    }
}
