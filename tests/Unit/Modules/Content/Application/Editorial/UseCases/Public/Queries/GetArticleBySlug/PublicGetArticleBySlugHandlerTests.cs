using _116.Content.Application.Editorial.UseCases.Public.Queries.GetArticleBySlug;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetArticleBySlug;

/// <summary>
/// Unit tests for <see cref="PublicGetArticleBySlugHandler"/>.
/// </summary>
public class PublicGetArticleBySlugHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IArticleRepository> _articleRepositoryMock;
    private readonly PublicGetArticleBySlugHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public PublicGetArticleBySlugHandlerTests()
    {
        _articleRepositoryMock = MockArticleRepository.Create();
        _handler = new PublicGetArticleBySlugHandler(_articleRepositoryMock.Object, Mapper);
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
        result.Should().NotBeNull();
        result.Article.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WhenArticleNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        string slug = TestConstants.Content.Editorial.Article.ValidSlug;
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
}
