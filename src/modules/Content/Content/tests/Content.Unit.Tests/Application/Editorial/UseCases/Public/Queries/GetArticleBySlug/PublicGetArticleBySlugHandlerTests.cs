using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetArticleBySlug;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.TestData;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Mocks.Repositories;
using _116.Content.TestData.Mocks.Services;
using _116.Storage.Contracts.Application.DTOs;
using _116.Storage.Contracts.Application.Services;
using _116.Storage.TestData.Factories;
using _116.Tests.TestData.Constants;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Editorial.UseCases.Public.Queries.GetArticleBySlug;

/// <summary>
/// Unit tests for <see cref="PublicGetArticleBySlugHandler"/>.
/// </summary>
public class PublicGetArticleBySlugHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IArticleRepository> _articleRepositoryMock;
    private readonly Mock<IArticleInteractionRepository> _articleInteractionRepositoryMock;
    private readonly Mock<IFileStorageService> _fileStorageMock;
    private readonly PublicGetArticleBySlugHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public PublicGetArticleBySlugHandlerTests()
    {
        _articleRepositoryMock = MockArticleRepository.Create();
        _articleInteractionRepositoryMock = MockArticleInteractionRepository.Create();
        _fileStorageMock = MockFileStorageService.Create();
        FileReferenceDto coverFile = FileReferenceDtoFactory.CreateImage();
        _fileStorageMock.SetupResolve(coverFile);
        _handler = new PublicGetArticleBySlugHandler(
            _articleRepositoryMock.Object,
            _articleInteractionRepositoryMock.Object,
            CreateArticleDtoService(_fileStorageMock.Object),
            TestErrorsFactory.CreateContentI18n()
        );
    }

    [Fact]
    public async Task Handle_WhenPublishedArticleExists_ShouldReturnArticleDetail()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.CreatePublished(CategoryId);
        string slug = article.Slug;
        var query = new PublicGetArticleBySlugQuery(Slug: slug);

        _articleRepositoryMock.SetupGetBySlug(slug, article);

        // Act
        PublicGetArticleBySlugResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Article.Id.Should().Be(article.Id);
    }

    [Fact]
    public async Task Handle_WhenArticleNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        string slug = TestConstants.Article.ValidSlug;
        var query = new PublicGetArticleBySlugQuery(Slug: slug);

        _articleRepositoryMock.SetupGetBySlug(slug, null);

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenArticleExistsButNotPublished_ShouldThrowNotFoundException()
    {
        // Arrange
        ArticleEntity draftArticle = ArticleFactory.Create(CategoryId); // Draft status
        string slug = draftArticle.Slug;
        var query = new PublicGetArticleBySlugQuery(Slug: slug);

        _articleRepositoryMock.SetupGetBySlug(slug, draftArticle);

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenAnonymous_ShouldReturnFalseFlagsAndSkipExistenceChecks()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.CreatePublished(CategoryId);
        var query = new PublicGetArticleBySlugQuery(Slug: article.Slug, CurrentUserId: null);

        _articleRepositoryMock.SetupGetBySlug(article.Slug, article);

        // Act
        PublicGetArticleBySlugResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Article.IsLiked.Should().BeFalse();
        result.Article.IsBookmarked.Should().BeFalse();
        _articleInteractionRepositoryMock.VerifyExistenceChecksNotCalled();
    }

    [Fact]
    public async Task Handle_WhenUserLikedAndBookmarked_ShouldReturnTrueFlags()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.CreatePublished(CategoryId);
        var userId = Guid.NewGuid();
        var query = new PublicGetArticleBySlugQuery(Slug: article.Slug, CurrentUserId: userId);

        _articleRepositoryMock.SetupGetBySlug(article.Slug, article);
        _articleInteractionRepositoryMock.SetupHasLikedAsync(userId, article.Id, result: true);
        _articleInteractionRepositoryMock.SetupHasBookmarkedAsync(userId, article.Id, result: true);

        // Act
        PublicGetArticleBySlugResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Article.IsLiked.Should().BeTrue();
        result.Article.IsBookmarked.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenUserLikedButNotBookmarked_ShouldReflectEachFlagIndependently()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.CreatePublished(CategoryId);
        var userId = Guid.NewGuid();
        var query = new PublicGetArticleBySlugQuery(Slug: article.Slug, CurrentUserId: userId);

        _articleRepositoryMock.SetupGetBySlug(article.Slug, article);
        _articleInteractionRepositoryMock.SetupHasLikedAsync(userId, article.Id, result: true);
        _articleInteractionRepositoryMock.SetupHasBookmarkedAsync(userId, article.Id, result: false);

        // Act
        PublicGetArticleBySlugResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Article.IsLiked.Should().BeTrue();
        result.Article.IsBookmarked.Should().BeFalse();
    }
}
