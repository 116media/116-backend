using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPublishedArticles;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Shared.Application.Pagination;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Factories.Core;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetPublishedArticles;

/// <summary>
/// Unit tests for <see cref="PublicGetPublishedArticlesHandler"/>.
/// </summary>
public class PublicGetPublishedArticlesHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IArticleRepository> _articleRepositoryMock;
    private readonly Mock<IFileRepository> _fileRepositoryMock;
    private readonly PublicGetPublishedArticlesHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public PublicGetPublishedArticlesHandlerTests()
    {
        _articleRepositoryMock = MockArticleRepository.Create();
        _fileRepositoryMock = MockFileRepository.Create();
        FileEntity coverFile = FileFactory.CreateImage();
        _fileRepositoryMock.SetupGetById(coverFile);
        _handler = new PublicGetPublishedArticlesHandler(
            _articleRepositoryMock.Object,
            _fileRepositoryMock.Object,
            Mapper
        );
    }

    [Fact]
    public async Task Handle_WhenPublishedArticlesExist_ShouldReturnPaginatedResult()
    {
        // Arrange
        List<ArticleEntity> articles = ArticleFactory.CreateManyPublished(CategoryId, 3);
        var query = new PublicGetPublishedArticlesQuery(
            PaginatedRequest: new PaginatedRequest(0, 10),
            Search: null,
            CategoryId: null,
            TagSlug: null
        );

        _articleRepositoryMock.SetupGetAllAsync(articles, articles.Count);

        // Act
        PublicGetPublishedArticlesResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Articles.Items.Should().HaveCount(articles.Count);
        result.Articles.Count.Should().Be((long)articles.Count);
    }

    [Fact]
    public async Task Handle_WhenNoPublishedArticlesExist_ShouldReturnEmptyPaginatedResult()
    {
        // Arrange
        var query = new PublicGetPublishedArticlesQuery(
            PaginatedRequest: new PaginatedRequest(0, 10),
            Search: null,
            CategoryId: null,
            TagSlug: null
        );

        _articleRepositoryMock.SetupGetAllAsync(new List<ArticleEntity>(), 0);

        // Act
        PublicGetPublishedArticlesResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Articles.Items.Should().BeEmpty();
        result.Articles.Count.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WhenAnonymous_ShouldReturnAllFalseFlagsAndSkipBatchLookups()
    {
        // Arrange
        List<ArticleEntity> articles = ArticleFactory.CreateManyPublished(CategoryId, 3);
        var query = new PublicGetPublishedArticlesQuery(
            PaginatedRequest: new PaginatedRequest(0, 10),
            Search: null,
            CategoryId: null,
            TagSlug: null
        );

        _articleRepositoryMock.SetupGetAllAsync(articles, articles.Count);

        // Act
        PublicGetPublishedArticlesResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Articles.Items.Should().OnlyContain(article => !article.IsLiked && !article.IsBookmarked);
        _articleRepositoryMock.VerifyGetLikedAndBookmarkedIdsCalledWithUser(Times.Never());
    }

    [Fact]
    public async Task Handle_WhenAuthenticated_ShouldStampOnlyInteractedItems_WithSingleBatchPerType()
    {
        // Arrange
        List<ArticleEntity> articles = ArticleFactory.CreateManyPublished(CategoryId, 3);
        Guid likedId = articles[0].Id;
        Guid bookmarkedId = articles[1].Id;
        var query = new PublicGetPublishedArticlesQuery(
            PaginatedRequest: new PaginatedRequest(0, 10),
            Search: null,
            CategoryId: null,
            TagSlug: null,
            CurrentUserId: Guid.NewGuid()
        );

        _articleRepositoryMock.SetupGetAllAsync(articles, articles.Count);
        _articleRepositoryMock.SetupGetLikedAndBookmarkedIds([likedId], [bookmarkedId]);

        // Act
        PublicGetPublishedArticlesResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Articles.Items.Single(article => article.Id == likedId).IsLiked.Should().BeTrue();
        result.Articles.Items.Single(article => article.Id == likedId).IsBookmarked.Should().BeFalse();
        result.Articles.Items.Single(article => article.Id == bookmarkedId).IsBookmarked.Should().BeTrue();
        result.Articles.Items.Single(article => article.Id == bookmarkedId).IsLiked.Should().BeFalse();
        result.Articles.Items.Single(article => article.Id == articles[2].Id).IsLiked.Should().BeFalse();
        result.Articles.Items.Single(article => article.Id == articles[2].Id).IsBookmarked.Should().BeFalse();

        _articleRepositoryMock.VerifyGetLikedAndBookmarkedIdsCalledWithUser(Times.Once());
    }
}
