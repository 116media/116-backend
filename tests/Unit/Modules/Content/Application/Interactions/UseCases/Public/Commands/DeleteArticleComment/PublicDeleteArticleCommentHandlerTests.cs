using _116.Content.Application.Interactions.UseCases.Public.Commands.DeleteArticleComment;
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

namespace _116.Unit.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.DeleteArticleComment;

/// <summary>
/// Unit tests for <see cref="PublicDeleteArticleCommentHandler"/>.
/// </summary>
public class PublicDeleteArticleCommentHandlerTests
{
    private readonly Mock<IArticleRepository> _articleRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly PublicDeleteArticleCommentHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public PublicDeleteArticleCommentHandlerTests()
    {
        _articleRepositoryMock = MockArticleRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new PublicDeleteArticleCommentHandler(_articleRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenCommentExistsAndOwner_ShouldSoftDeleteDecrementAndCommit()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.CreatePublished(CategoryId);
        Guid userId = Guid.NewGuid();
        ArticleCommentEntity comment = ArticleCommentFactory.Create(article.Id, userId);
        var command = new PublicDeleteArticleCommentCommand(
            UserId: comment.UserId,
            ArticleId: article.Id,
            CommentId: comment.Id
        );
        _articleRepositoryMock.SetupGetCommentByIdAsync(comment);
        _articleRepositoryMock.SetupGetByIdOrThrow(article);

        // Act
        PublicDeleteArticleCommentResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _articleRepositoryMock.VerifyUpdateCommentCalled();
        _articleRepositoryMock.VerifyUpdateCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenCommentNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new PublicDeleteArticleCommentCommand(
            UserId: Guid.NewGuid(),
            ArticleId: Guid.NewGuid(),
            CommentId: Guid.NewGuid()
        );
        _articleRepositoryMock.SetupGetCommentByIdAsync(null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenNotCommentOwner_ShouldThrowBadRequestException()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.CreatePublished(CategoryId);
        Guid commentOwnerId = Guid.NewGuid();
        ArticleCommentEntity comment = ArticleCommentFactory.Create(article.Id, commentOwnerId);
        var command = new PublicDeleteArticleCommentCommand(
            UserId: Guid.NewGuid(),
            ArticleId: article.Id,
            CommentId: comment.Id
        );
        _articleRepositoryMock.SetupGetCommentByIdAsync(comment);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    #endregion
}
