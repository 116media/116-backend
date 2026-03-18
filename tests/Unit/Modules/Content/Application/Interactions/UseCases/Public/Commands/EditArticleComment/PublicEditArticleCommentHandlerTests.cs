using _116.Content.Application.Interactions.UseCases.Public.Commands.EditArticleComment;
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

namespace _116.Unit.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.EditArticleComment;

/// <summary>
/// Unit tests for <see cref="PublicEditArticleCommentHandler"/>.
/// </summary>
public class PublicEditArticleCommentHandlerTests
{
    private readonly Mock<IArticleRepository> _articleRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly PublicEditArticleCommentHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public PublicEditArticleCommentHandlerTests()
    {
        _articleRepositoryMock = MockArticleRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new PublicEditArticleCommentHandler(_articleRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenCommentExistsAndOwner_ShouldEditAndCommit()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.CreatePublished(CategoryId);
        Guid userId = Guid.NewGuid();
        ArticleCommentEntity comment = ArticleCommentFactory.Create(article.Id, userId);
        var command = new PublicEditArticleCommentCommand(
            ArticleId: article.Id,
            CommentId: comment.Id,
            UserId: comment.UserId,
            Body: "Updated comment body."
        );
        _articleRepositoryMock.SetupGetCommentByIdAsync(comment);

        // Act
        PublicEditArticleCommentResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _articleRepositoryMock.VerifyUpdateCommentCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenCommentNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new PublicEditArticleCommentCommand(
            ArticleId: Guid.NewGuid(),
            CommentId: Guid.NewGuid(),
            UserId: Guid.NewGuid(),
            Body: "Updated comment body."
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
        var command = new PublicEditArticleCommentCommand(
            ArticleId: article.Id,
            CommentId: comment.Id,
            UserId: Guid.NewGuid(),
            Body: "Updated comment body."
        );
        _articleRepositoryMock.SetupGetCommentByIdAsync(comment);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    #endregion
}
