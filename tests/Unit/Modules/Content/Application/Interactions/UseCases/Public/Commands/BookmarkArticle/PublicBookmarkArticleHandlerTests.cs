using _116.Content.Application.Interactions.UseCases.Public.Commands.BookmarkArticle;
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

namespace _116.Unit.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.BookmarkArticle;

/// <summary>
/// Unit tests for <see cref="PublicBookmarkArticleHandler"/>.
/// </summary>
public class PublicBookmarkArticleHandlerTests
{
    private readonly Mock<IArticleInteractionRepository> _articleInteractionRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly PublicBookmarkArticleHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public PublicBookmarkArticleHandlerTests()
    {
        _articleInteractionRepositoryMock = MockArticleInteractionRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new PublicBookmarkArticleHandler(
            _articleInteractionRepositoryMock.Object,
            _unitOfWorkMock.Object,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenArticleExistsAndNotBookmarked_ShouldAddBookmarkAndCommit()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.CreatePublished(CategoryId);
        Guid userId = Guid.NewGuid();
        var command = new PublicBookmarkArticleCommand(ArticleId: article.Id, UserId: userId);
        _articleInteractionRepositoryMock.SetupExistsOrThrow(article.Id);
        _articleInteractionRepositoryMock.SetupHasBookmarkedAsync(userId, article.Id, result: false);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _articleInteractionRepositoryMock.VerifyAddBookmarkCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenArticleNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid articleId = Guid.NewGuid();
        var command = new PublicBookmarkArticleCommand(ArticleId: articleId, UserId: Guid.NewGuid());
        _articleInteractionRepositoryMock.SetupExistsOrThrowNotFound(articleId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenAlreadyBookmarked_ShouldThrowConflictException()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.CreatePublished(CategoryId);
        Guid userId = Guid.NewGuid();
        var command = new PublicBookmarkArticleCommand(ArticleId: article.Id, UserId: userId);
        _articleInteractionRepositoryMock.SetupExistsOrThrow(article.Id);
        _articleInteractionRepositoryMock.SetupHasBookmarkedAsync(userId, article.Id, result: true);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    #endregion
}
