using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Interactions.UseCases.Public.Commands.DeleteArticleComment;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Factories.Helpers;
using _116.Content.TestData.Mocks.Factories;
using _116.Content.TestData.Mocks.Infrastructure;
using _116.Content.TestData.Mocks.Repositories;
using _116.Content.TestData.Mocks.Services;
using _116.Identity.TestData.Factories;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Identity.TestData.Mocks.Repositories;
using _116.Identity.TestData.Mocks.Services;
using _116.Storage.TestData.Factories;
using _116.Storage.TestData.Mocks.Infrastructure;
using _116.Storage.TestData.Mocks.Services;
using _116.Tests.TestData.Constants;
using _116.Tests.TestData.Helpers;
using _116.Tests.TestData.Mocks;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Interactions.UseCases.Public.Commands.DeleteArticleComment;

/// <summary>
/// Unit tests for <see cref="PublicDeleteArticleCommentHandler"/>.
/// </summary>
public class PublicDeleteArticleCommentHandlerTests
{
    private readonly Mock<IArticleCommentRepository> _articleCommentRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly PublicDeleteArticleCommentHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public PublicDeleteArticleCommentHandlerTests()
    {
        _articleCommentRepositoryMock = MockArticleCommentRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new PublicDeleteArticleCommentHandler(
            _articleCommentRepositoryMock.Object,
            _unitOfWorkMock.Object,
            TestErrorsFactory.CreateContentI18n(),
            TimeProvider.System
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
        _articleCommentRepositoryMock.SetupGetCommentByIdInArticleAsync(comment, article.Id);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        comment.IsDeleted.Should().BeTrue();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenCommentAlreadyDeleted_ShouldReportSuccessWithoutCommitting()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.CreatePublished(CategoryId);
        ArticleCommentEntity comment = ArticleCommentFactory.Create(article.Id, Guid.NewGuid());
        comment.SoftDelete(TestConstants.Clock.Instant);
        comment.ClearDomainEvents();
        var command = new PublicDeleteArticleCommentCommand(
            UserId: comment.UserId,
            ArticleId: article.Id,
            CommentId: comment.Id
        );
        _articleCommentRepositoryMock.SetupGetCommentByIdInArticleAsync(comment, article.Id);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        comment.DomainEvents.Should().BeEmpty();
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
        _articleCommentRepositoryMock.SetupGetCommentByIdInArticleNotFound(command.CommentId, command.ArticleId);

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
        _articleCommentRepositoryMock.SetupGetCommentByIdInArticleAsync(comment, article.Id);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    #endregion
}
