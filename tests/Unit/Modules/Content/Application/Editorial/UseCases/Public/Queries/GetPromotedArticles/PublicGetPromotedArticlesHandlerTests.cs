using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPromotedArticles;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Factories.Core;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetPromotedArticles;

/// <summary>
/// Unit tests for <see cref="PublicGetPromotedArticlesHandler"/>.
/// </summary>
public class PublicGetPromotedArticlesHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IArticleRepository> _articleRepositoryMock;
    private readonly Mock<IFileRepository> _fileRepositoryMock;
    private readonly PublicGetPromotedArticlesHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public PublicGetPromotedArticlesHandlerTests()
    {
        _articleRepositoryMock = MockArticleRepository.Create();
        _fileRepositoryMock = MockFileRepository.Create();
        FileEntity coverFile = FileFactory.CreateImage();
        _fileRepositoryMock.SetupGetById(coverFile);
        _handler = new PublicGetPromotedArticlesHandler(
            _articleRepositoryMock.Object,
            _fileRepositoryMock.Object,
            Mapper
        );
    }

    [Fact]
    public async Task Handle_WhenPromotedArticlesExist_ShouldReturnArticleList()
    {
        // Arrange
        List<ArticleEntity> promoted = ArticleFactory.CreateMany(CategoryId, 2);
        var query = new PublicGetPromotedArticlesQuery();

        _articleRepositoryMock.SetupGetPromotedAsync(promoted);

        // Act
        PublicGetPromotedArticlesResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Articles.Count.Should().Be(promoted.Count);
    }

    [Fact]
    public async Task Handle_WhenNoPromotedArticlesExist_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new PublicGetPromotedArticlesQuery();

        _articleRepositoryMock.SetupGetPromotedAsync(new List<ArticleEntity>());

        // Act
        PublicGetPromotedArticlesResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Articles.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenAnonymous_ShouldReturnAllFalseFlagsAndSkipBatchLookups()
    {
        // Arrange
        List<ArticleEntity> promoted = ArticleFactory.CreateManyPublished(CategoryId, 2);
        var query = new PublicGetPromotedArticlesQuery();

        _articleRepositoryMock.SetupGetPromotedAsync(promoted);

        // Act
        PublicGetPromotedArticlesResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Articles.Should().OnlyContain(article => !article.IsLiked && !article.IsBookmarked);
        _articleRepositoryMock.VerifyGetLikedAndBookmarkedIdsCalledWithUser(Times.Never());
    }

    [Fact]
    public async Task Handle_WhenAuthenticated_ShouldStampOnlyInteractedItems_WithSingleBatchPerType()
    {
        // Arrange
        List<ArticleEntity> promoted = ArticleFactory.CreateManyPublished(CategoryId, 3);
        Guid likedId = promoted[0].Id;
        Guid bookmarkedId = promoted[1].Id;
        var query = new PublicGetPromotedArticlesQuery(CurrentUserId: Guid.NewGuid());

        _articleRepositoryMock.SetupGetPromotedAsync(promoted);
        _articleRepositoryMock.SetupGetLikedAndBookmarkedIds([likedId], [bookmarkedId]);

        // Act
        PublicGetPromotedArticlesResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Articles.Single(article => article.Id == likedId).IsLiked.Should().BeTrue();
        result.Articles.Single(article => article.Id == bookmarkedId).IsBookmarked.Should().BeTrue();
        result.Articles.Single(article => article.Id == promoted[2].Id).IsLiked.Should().BeFalse();
        result.Articles.Single(article => article.Id == promoted[2].Id).IsBookmarked.Should().BeFalse();

        _articleRepositoryMock.VerifyGetLikedAndBookmarkedIdsCalledWithUser(Times.Once());
    }
}
