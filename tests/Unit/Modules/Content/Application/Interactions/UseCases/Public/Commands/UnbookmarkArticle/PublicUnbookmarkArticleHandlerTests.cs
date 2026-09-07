using _116.Content.Application.Interactions.UseCases.Public.Commands.UnbookmarkArticle;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.UnbookmarkArticle;

/// <summary>
/// Unit tests for <see cref="PublicUnbookmarkArticleHandler"/>.
/// </summary>
public class PublicUnbookmarkArticleHandlerTests
{
    private readonly Mock<IArticleInteractionRepository> _articleInteractionRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly PublicUnbookmarkArticleHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public PublicUnbookmarkArticleHandlerTests()
    {
        _articleInteractionRepositoryMock = MockArticleInteractionRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new PublicUnbookmarkArticleHandler(
            _articleInteractionRepositoryMock.Object,
            _unitOfWorkMock.Object,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenArticleExistsAndBookmarked_ShouldRemoveBookmarkAndCommit()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.CreatePublished(CategoryId);
        Guid userId = Guid.NewGuid();
        var command = new PublicUnbookmarkArticleCommand(ArticleId: article.Id, UserId: userId);
        _articleInteractionRepositoryMock.SetupExistsOrThrow(article.Id);
        _articleInteractionRepositoryMock.SetupHasBookmarkedAsync(userId, article.Id, result: true);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _articleInteractionRepositoryMock.VerifyRemoveBookmarkCalled(userId, article.Id);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenArticleNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid articleId = Guid.NewGuid();
        var command = new PublicUnbookmarkArticleCommand(ArticleId: articleId, UserId: Guid.NewGuid());
        _articleInteractionRepositoryMock.SetupExistsOrThrowNotFound(articleId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenBookmarkNotFound_ShouldThrowBadRequestException()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.CreatePublished(CategoryId);
        Guid userId = Guid.NewGuid();
        var command = new PublicUnbookmarkArticleCommand(ArticleId: article.Id, UserId: userId);
        _articleInteractionRepositoryMock.SetupExistsOrThrow(article.Id);
        _articleInteractionRepositoryMock.SetupHasBookmarkedAsync(userId, article.Id, result: false);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    #endregion
}
