using _116.Content.Application.Interactions.UseCases.Public.Commands.DeleteArticleComment;
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
        _handler = new PublicDeleteArticleCommentHandler(
            _articleRepositoryMock.Object,
            _unitOfWorkMock.Object,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenCommentExistsAndOwner_ShouldSoftDeleteAndCommit()
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
        _articleRepositoryMock.SetupGetCommentByIdInArticleAsync(comment, article.Id);

        // Act
        PublicDeleteArticleCommentResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _articleRepositoryMock.VerifyUpdateCommentCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenCommentAlreadyDeleted_ShouldReportSuccessWithoutCommitting()
    {
        // Arrange — a repeated delete must not decrement the article's cached
        // comment count a second time.
        ArticleEntity article = ArticleFactory.CreatePublished(CategoryId);
        ArticleCommentEntity comment = ArticleCommentFactory.Create(article.Id, Guid.NewGuid());
        comment.SoftDelete();
        comment.ClearDomainEvents();
        var command = new PublicDeleteArticleCommentCommand(
            UserId: comment.UserId,
            ArticleId: article.Id,
            CommentId: comment.Id
        );
        _articleRepositoryMock.SetupGetCommentByIdInArticleAsync(comment, article.Id);

        // Act
        PublicDeleteArticleCommentResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        comment.DomainEvents.Should().BeEmpty();
        _articleRepositoryMock.Verify(x => x.UpdateComment(It.IsAny<ArticleCommentEntity>()), Times.Never);
        _unitOfWorkMock.VerifyCommitNotCalled();
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
        _articleRepositoryMock.SetupGetCommentByIdInArticleAsync(null, command.ArticleId);

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
        _articleRepositoryMock.SetupGetCommentByIdInArticleAsync(comment, article.Id);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    #endregion
}
