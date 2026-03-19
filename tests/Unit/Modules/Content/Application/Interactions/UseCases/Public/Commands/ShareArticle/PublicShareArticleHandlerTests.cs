using _116.Content.Application.Interactions.UseCases.Public.Commands.ShareArticle;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.ShareArticle;

/// <summary>
/// Unit tests for <see cref="PublicShareArticleHandler"/>.
/// </summary>
public class PublicShareArticleHandlerTests
{
    private readonly Mock<IArticleRepository> _articleRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly PublicShareArticleHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public PublicShareArticleHandlerTests()
    {
        _articleRepositoryMock = MockArticleRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new PublicShareArticleHandler(_articleRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenArticleExistsAndAnonymous_ShouldAddShareIncrementAndCommit()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.CreatePublished(CategoryId);
        var command = new PublicShareArticleCommand(ArticleId: article.Id, UserId: null);
        _articleRepositoryMock.SetupGetByIdOrThrow(article);

        // Act
        PublicShareArticleResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _articleRepositoryMock.VerifyAddShareCalled();
        _articleRepositoryMock.VerifyUpdateCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenArticleExistsAndAuthenticated_ShouldAddShareWithUserId()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.CreatePublished(CategoryId);
        var command = new PublicShareArticleCommand(ArticleId: article.Id, UserId: Guid.NewGuid());
        _articleRepositoryMock.SetupGetByIdOrThrow(article);

        // Act
        PublicShareArticleResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _articleRepositoryMock.VerifyAddShareCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenArticleNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid articleId = Guid.NewGuid();
        var command = new PublicShareArticleCommand(ArticleId: articleId, UserId: null);
        _articleRepositoryMock.SetupGetByIdOrThrowNotFound(articleId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
