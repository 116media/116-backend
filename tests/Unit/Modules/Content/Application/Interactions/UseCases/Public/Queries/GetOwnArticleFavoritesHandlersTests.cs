using _116.Content.Application.Interactions.UseCases.Public.Queries.GetOwnCommentedArticles;
using _116.Content.Application.Interactions.UseCases.Public.Queries.GetOwnCommentsForArticle;
using _116.Content.Application.Interactions.UseCases.Public.Queries.GetOwnLikedArticles;
using _116.Content.Application.Interactions.UseCases.Public.Queries.GetOwnSharedArticles;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
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

namespace _116.Unit.Tests.Modules.Content.Application.Interactions.UseCases.Public.Queries;

/// <summary>
/// Unit tests for the current-user article favorite query handlers.
/// </summary>
public class GetOwnArticleFavoritesHandlersTests : BaseContentHandlerTest
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid CategoryId = Guid.NewGuid();
    private readonly Mock<IArticleRepository> _articles = MockArticleRepository.Create();
    private readonly Mock<IFileRepository> _files = MockFileRepository.Create();

    [Fact]
    public async Task LikedHandler_MapsTimestampAndCurrentLikeState()
    {
        ArticleEntity article = ArticleFactory.CreatePublished(CategoryId);
        DateTimeOffset interactedAt = DateTimeOffset.UtcNow.AddHours(-1);
        _articles.SetupGetLikedArticlesAsync([new ArticleActivity(article, interactedAt, 1)], 1);
        _articles.SetupGetLikedAndBookmarkedIds(
            likedIds: new HashSet<Guid> { article.Id },
            bookmarkedIds: new HashSet<Guid>()
        );
        var handler = new PublicGetOwnLikedArticlesHandler(_articles.Object, _files.Object, Mapper);

        PublicGetOwnLikedArticlesResult result = await handler.Handle(
            new PublicGetOwnLikedArticlesQuery(UserId, new PaginatedRequest(0, 12)),
            CancellationToken.None
        );

        result.Articles.Items.Single().LastInteractedAt.Should().Be(interactedAt);
        result.Articles.Items.Single().Article.IsLiked.Should().BeTrue();
    }

    [Fact]
    public async Task SharedHandler_PreservesOwnCountLatestTimestampAndChannel()
    {
        ArticleEntity article = ArticleFactory.CreatePublished(CategoryId);
        DateTimeOffset interactedAt = DateTimeOffset.UtcNow.AddMinutes(-10);
        _articles.SetupGetSharedArticlesAsync(
            [new ArticleActivity(article, interactedAt, 3, EnumShareChannel.WhatsApp)],
            1
        );
        var handler = new PublicGetOwnSharedArticlesHandler(_articles.Object, _files.Object, Mapper);

        PublicGetOwnSharedArticlesResult result = await handler.Handle(
            new PublicGetOwnSharedArticlesQuery(UserId, new PaginatedRequest(0, 12)),
            CancellationToken.None
        );

        result.Articles.Items.Single().InteractionCount.Should().Be(3);
        result.Articles.Items.Single().LastInteractedAt.Should().Be(interactedAt);
        result.Articles.Items.Single().LastShareChannel.Should().Be(EnumShareChannel.WhatsApp);
    }

    [Fact]
    public async Task CommentedHandler_MapsLatestCommentAndRemainingCount()
    {
        ArticleEntity article = ArticleFactory.CreatePublished(CategoryId);
        ArticleCommentEntity comment = ArticleCommentFactory.Create(article.Id, UserId);
        DateTimeOffset commentedAt = DateTimeOffset.UtcNow.AddMinutes(-5);
        _articles.SetupGetCommentedArticlesAsync([new CommentedArticleActivity(article, comment, 2, commentedAt)], 1);
        var handler = new PublicGetOwnCommentedArticlesHandler(_articles.Object, _files.Object, Mapper);

        PublicGetOwnCommentedArticlesResult result = await handler.Handle(
            new PublicGetOwnCommentedArticlesQuery(UserId, new PaginatedRequest(0, 12)),
            CancellationToken.None
        );

        result.Articles.Items.Single().LatestComment.Id.Should().Be(comment.Id);
        result.Articles.Items.Single().CommentCount.Should().Be(2);
        result.Articles.Items.Single().LastCommentedAt.Should().Be(commentedAt);
    }

    [Fact]
    public async Task MyCommentsHandler_ReturnsTopLevelCommentsAndReplies()
    {
        ArticleEntity article = ArticleFactory.CreatePublished(CategoryId);
        ArticleCommentEntity parent = ArticleCommentFactory.Create(article.Id, UserId);
        ArticleCommentEntity reply = ArticleCommentEntity.CreateReply(
            Guid.NewGuid(),
            UserId,
            article.Id,
            parent.Id,
            "My reply"
        );
        _articles.SetupGetByIdAsync(article.Id, article);
        _articles.SetupGetOwnCommentsForArticleAsync([parent, reply], 2);
        var handler = new PublicGetOwnCommentsForArticleHandler(
            _articles.Object,
            Mapper,
            TestErrorsFactory.CreateContentI18n()
        );

        PublicGetOwnCommentsForArticleResult result = await handler.Handle(
            new PublicGetOwnCommentsForArticleQuery(UserId, article.Id, new PaginatedRequest(0, 20)),
            CancellationToken.None
        );

        result.Comments.Count.Should().Be(2);
        result.Comments.Items.Should().Contain(item => item.ParentCommentId == parent.Id);
    }

    [Fact]
    public async Task MyCommentsHandler_WhenArticleIsNotPublished_ThrowsNotFound()
    {
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        _articles.SetupGetByIdAsync(article.Id, article);
        var handler = new PublicGetOwnCommentsForArticleHandler(
            _articles.Object,
            Mapper,
            TestErrorsFactory.CreateContentI18n()
        );

        Func<Task> act = () =>
            handler.Handle(
                new PublicGetOwnCommentsForArticleQuery(UserId, article.Id, new PaginatedRequest(0, 20)),
                CancellationToken.None
            );

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
