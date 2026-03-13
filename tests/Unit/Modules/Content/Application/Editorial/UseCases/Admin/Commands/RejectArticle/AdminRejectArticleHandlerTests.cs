using _116.Content.Application.Editorial.UseCases.Admin.Commands.RejectArticle;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.RejectArticle;

/// <summary>
/// Unit tests for <see cref="AdminRejectArticleHandler"/>.
/// </summary>
public class AdminRejectArticleHandlerTests
{
    private readonly Mock<IArticleRepository> _articleRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminRejectArticleHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public AdminRejectArticleHandlerTests()
    {
        _articleRepositoryMock = MockArticleRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminRejectArticleHandler(_articleRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenArticleInPendingReview_ShouldRejectWithReasonAndReturnSuccess()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.CreatePendingReview(CategoryId);
        var command = new AdminRejectArticleCommand(
            Id: article.Id.ToString(),
            Reason: TestConstants.Content.Editorial.Article.ValidRejectionReason
        );
        _articleRepositoryMock.SetupGetByIdOrThrow(article);

        // Act
        AdminRejectArticleResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _articleRepositoryMock.VerifyUpdateCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenArticleNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        var command = new AdminRejectArticleCommand(
            Id: nonExistentId.ToString(),
            Reason: TestConstants.Content.Editorial.Article.ValidRejectionReason
        );
        _articleRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenArticleAlreadyRejected_ShouldThrowConflictException()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.CreateRejected(CategoryId);
        var command = new AdminRejectArticleCommand(
            Id: article.Id.ToString(),
            Reason: TestConstants.Content.Editorial.Article.ValidRejectionReason
        );
        _articleRepositoryMock.SetupGetByIdOrThrow(article);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenArticleInWrongStatus_ShouldThrowBadRequestException()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId); // Draft status
        var command = new AdminRejectArticleCommand(
            Id: article.Id.ToString(),
            Reason: TestConstants.Content.Editorial.Article.ValidRejectionReason
        );
        _articleRepositoryMock.SetupGetByIdOrThrow(article);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    #endregion
}
