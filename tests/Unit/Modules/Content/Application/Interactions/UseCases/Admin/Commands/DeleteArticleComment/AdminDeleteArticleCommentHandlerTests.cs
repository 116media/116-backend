using _116.Content.Application.Interactions.UseCases.Admin.Commands.DeleteArticleComment;
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

namespace _116.Unit.Tests.Modules.Content.Application.Interactions.UseCases.Admin.Commands.DeleteArticleComment;

/// <summary>
/// Unit tests for <see cref="AdminDeleteArticleCommentHandler"/>.
/// </summary>
public class AdminDeleteArticleCommentHandlerTests
{
    private readonly Mock<IArticleRepository> _articleRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminDeleteArticleCommentHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public AdminDeleteArticleCommentHandlerTests()
    {
        _articleRepositoryMock = MockArticleRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminDeleteArticleCommentHandler(
            _articleRepositoryMock.Object,
            _unitOfWorkMock.Object,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenCommentExists_ShouldSoftDeleteAndCommit()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.CreatePublished(CategoryId);
        Guid userId = Guid.NewGuid();
        ArticleCommentEntity comment = ArticleCommentFactory.Create(article.Id, userId);
        var command = new AdminDeleteArticleCommentCommand(ArticleId: article.Id, CommentId: comment.Id);
        _articleRepositoryMock.SetupGetCommentByIdInArticleAsync(comment, article.Id);

        // Act
        AdminDeleteArticleCommentResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _articleRepositoryMock.VerifyUpdateCommentCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenCommentAlreadyDeleted_ShouldReportSuccessWithoutCommitting()
    {
        // Arrange — moderating a comment its owner already deleted must not
        // decrement the article's cached comment count a second time.
        ArticleEntity article = ArticleFactory.CreatePublished(CategoryId);
        ArticleCommentEntity comment = ArticleCommentFactory.Create(article.Id, Guid.NewGuid());
        comment.SoftDelete();
        comment.ClearDomainEvents();
        var command = new AdminDeleteArticleCommentCommand(ArticleId: article.Id, CommentId: comment.Id);
        _articleRepositoryMock.SetupGetCommentByIdInArticleAsync(comment, article.Id);

        // Act
        AdminDeleteArticleCommentResult result = await _handler.Handle(command, CancellationToken.None);

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
        var command = new AdminDeleteArticleCommentCommand(ArticleId: Guid.NewGuid(), CommentId: Guid.NewGuid());
        _articleRepositoryMock.SetupGetCommentByIdInArticleAsync(null, command.ArticleId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
