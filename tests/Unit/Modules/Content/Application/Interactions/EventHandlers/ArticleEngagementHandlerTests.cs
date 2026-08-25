using _116.Content.Application.Interactions.EventHandlers;
using _116.Content.Application.Shared.Cache;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Interactions.EventHandlers;

/// <summary>
/// Unit tests for <see cref="ArticleEngagementHandler"/>.
/// </summary>
public class ArticleEngagementHandlerTests
{
    private static readonly Guid CategoryId = Guid.NewGuid();

    private readonly Mock<IArticleRepository> _articleRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IPopularArticlesCacheInvalidator> _cacheInvalidatorMock;
    private readonly ArticleEngagementHandler _handler;

    public ArticleEngagementHandlerTests()
    {
        _articleRepositoryMock = MockArticleRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _cacheInvalidatorMock = MockPopularArticlesCacheInvalidator.Create();
        _handler = new ArticleEngagementHandler(
            _articleRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _cacheInvalidatorMock.Object,
            NullLogger<ArticleEngagementHandler>.Instance
        );
    }

    [Theory]
    [InlineData(EnumEngagementKind.Like, 1)]
    [InlineData(EnumEngagementKind.Bookmark, 1)]
    [InlineData(EnumEngagementKind.Comment, 1)]
    [InlineData(EnumEngagementKind.Share, 1)]
    public async Task Handle_WithPositiveDelta_ShouldIncrementMatchingCounterCommitAndInvalidate(
        EnumEngagementKind kind,
        int delta
    )
    {
        // Arrange
        ArticleEntity article = ArticleFactory.CreatePublished(CategoryId);
        _articleRepositoryMock.SetupGetByIdAsync(article.Id, article);

        // Act
        await _handler.Handle(new ArticleEngagedEvent(article.Id, kind, delta), CancellationToken.None);

        // Assert
        int total = article.LikeCount + article.BookmarkCount + article.CommentCount + article.ShareCount;
        total.Should().Be(1);
        _articleRepositoryMock.VerifyUpdateCalled(article);
        _unitOfWorkMock.VerifyCommitCalled();
        _cacheInvalidatorMock.VerifyInvalidateCalled();
    }

    [Theory]
    [InlineData(EnumEngagementKind.Like)]
    [InlineData(EnumEngagementKind.Bookmark)]
    [InlineData(EnumEngagementKind.Comment)]
    public async Task Handle_WithNegativeDelta_ShouldDecrementMatchingCounterCommitAndInvalidate(
        EnumEngagementKind kind
    )
    {
        // Arrange
        ArticleEntity article = ArticleFactory.CreatePublished(CategoryId);
        article.IncrementLikeCount();
        article.IncrementBookmarkCount();
        article.IncrementCommentCount();
        _articleRepositoryMock.SetupGetByIdAsync(article.Id, article);

        // Act
        await _handler.Handle(new ArticleEngagedEvent(article.Id, kind, -1), CancellationToken.None);

        // Assert
        int total = article.LikeCount + article.BookmarkCount + article.CommentCount;
        total.Should().Be(2);
        _articleRepositoryMock.VerifyUpdateCalled(article);
        _unitOfWorkMock.VerifyCommitCalled();
        _cacheInvalidatorMock.VerifyInvalidateCalled();
    }

    [Fact]
    public async Task Handle_WithKindWithoutArticleCounter_ShouldOnlyInvalidate()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.CreatePublished(CategoryId);
        _articleRepositoryMock.SetupGetByIdAsync(article.Id, article);

        // Act
        await _handler.Handle(new ArticleEngagedEvent(article.Id, EnumEngagementKind.View, 1), CancellationToken.None);

        // Assert
        _unitOfWorkMock.VerifyCommitNotCalled();
        _cacheInvalidatorMock.VerifyInvalidateCalled();
    }

    [Fact]
    public async Task Handle_WhenArticleMissing_ShouldSkipWithoutCommitOrInvalidation()
    {
        // Arrange — the article vanished between the interaction commit and
        // the dispatch, which is a race, not an error.
        Guid articleId = Guid.NewGuid();
        _articleRepositoryMock.SetupGetByIdAsync(articleId, null);

        // Act
        await _handler.Handle(new ArticleEngagedEvent(articleId, EnumEngagementKind.Like, 1), CancellationToken.None);

        // Assert
        _articleRepositoryMock.Verify(x => x.Update(It.IsAny<ArticleEntity>()), Times.Never);
        _unitOfWorkMock.VerifyCommitNotCalled();
        _cacheInvalidatorMock.VerifyInvalidateNotCalled();
    }
}
