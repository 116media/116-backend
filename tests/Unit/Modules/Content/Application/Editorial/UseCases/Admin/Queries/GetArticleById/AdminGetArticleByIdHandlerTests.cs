using _116.Content.Application.Editorial.UseCases.Admin.Queries.GetArticleById;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Queries.GetArticleById;

/// <summary>
/// Unit tests for <see cref="AdminGetArticleByIdHandler"/>.
/// </summary>
public class AdminGetArticleByIdHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IArticleRepository> _articleRepositoryMock;
    private readonly AdminGetArticleByIdHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public AdminGetArticleByIdHandlerTests()
    {
        _articleRepositoryMock = MockArticleRepository.Create();
        _handler = new AdminGetArticleByIdHandler(_articleRepositoryMock.Object, Mapper);
    }

    [Fact]
    public async Task Handle_WhenArticleExists_ShouldReturnArticleDetail()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        var query = new AdminGetArticleByIdQuery(Id: article.Id.ToString());
        _articleRepositoryMock.SetupGetByIdOrThrow(article);

        // Act
        AdminGetArticleByIdResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Article.Should().NotBeNull();
        result.Article.Id.Should().Be(article.Id);
    }

    [Fact]
    public async Task Handle_WhenArticleNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        var query = new AdminGetArticleByIdQuery(Id: nonExistentId.ToString());
        _articleRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
