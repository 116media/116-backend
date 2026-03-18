using _116.Content.Application.Interactions.UseCases.Public.Commands.UnlikeArticle;
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

namespace _116.Unit.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.UnlikeArticle;

/// <summary>
/// Unit tests for <see cref="PublicUnlikeArticleHandler"/>.
/// </summary>
public class PublicUnlikeArticleHandlerTests
{
    private readonly Mock<IArticleRepository> _articleRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly PublicUnlikeArticleHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public PublicUnlikeArticleHandlerTests()
    {
        _articleRepositoryMock = MockArticleRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new PublicUnlikeArticleHandler(_articleRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenArticleExistsAndLiked_ShouldRemoveLikeDecrementAndCommit()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.CreatePublished(CategoryId);
        Guid userId = Guid.NewGuid();
        var command = new PublicUnlikeArticleCommand(ArticleId: article.Id, UserId: userId);
        _articleRepositoryMock.SetupGetByIdOrThrow(article);
        _articleRepositoryMock.SetupHasLikedAsync(true);

        // Act
        PublicUnlikeArticleResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _articleRepositoryMock.VerifyRemoveLikeCalled(userId, article.Id);
        _articleRepositoryMock.VerifyUpdateCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenArticleNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid articleId = Guid.NewGuid();
        var command = new PublicUnlikeArticleCommand(ArticleId: articleId, UserId: Guid.NewGuid());
        _articleRepositoryMock.SetupGetByIdOrThrowNotFound(articleId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenLikeNotFound_ShouldThrowBadRequestException()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.CreatePublished(CategoryId);
        var command = new PublicUnlikeArticleCommand(ArticleId: article.Id, UserId: Guid.NewGuid());
        _articleRepositoryMock.SetupGetByIdOrThrow(article);
        _articleRepositoryMock.SetupHasLikedAsync(false);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    #endregion
}
