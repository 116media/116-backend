using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.ApproveArticle;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
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
using _116.Shared.Domain.Exceptions;
using _116.Storage.TestData.Factories;
using _116.Storage.TestData.Mocks.Infrastructure;
using _116.Storage.TestData.Mocks.Services;
using _116.Tests.TestData.Helpers;
using _116.Tests.TestData.Mocks;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Editorial.UseCases.Admin.Commands.ApproveArticle;

/// <summary>
/// Unit tests for <see cref="AdminApproveArticleHandler"/>.
/// </summary>
public class AdminApproveArticleHandlerTests
{
    private readonly Mock<IArticleRepository> _articleRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminApproveArticleHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public AdminApproveArticleHandlerTests()
    {
        _articleRepositoryMock = MockArticleRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminApproveArticleHandler(
            _articleRepositoryMock.Object,
            _unitOfWorkMock.Object,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenArticleInPendingReview_ShouldTransitionToApproved()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.CreatePendingReview(CategoryId);
        var command = new AdminApproveArticleCommand(Id: article.Id.ToString());
        _articleRepositoryMock.SetupGetByIdOrThrow(article);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        article.Status.Should().Be(EnumContentStatus.Approved);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenArticleNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        var command = new AdminApproveArticleCommand(Id: nonExistentId.ToString());
        _articleRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task Handle_WhenArticleAlreadyApproved_ShouldThrowConflictException()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.CreateApproved(CategoryId);
        var command = new AdminApproveArticleCommand(Id: article.Id.ToString());
        _articleRepositoryMock.SetupGetByIdOrThrow(article);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
        article.Status.Should().Be(EnumContentStatus.Approved);
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task Handle_WhenArticleInWrongStatus_ShouldThrowDomainRuleException()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        var command = new AdminApproveArticleCommand(Id: article.Id.ToString());
        _articleRepositoryMock.SetupGetByIdOrThrow(article);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainRuleException>();
        article.Status.Should().Be(EnumContentStatus.Draft);
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    #endregion
}
