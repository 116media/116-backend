using _116.Content.Application.Editorial.UseCases.Admin.Commands.RejectArticle;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Builders.Entities.Content;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Helpers;
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
        _handler = new AdminRejectArticleHandler(
            _articleRepositoryMock.Object,
            _unitOfWorkMock.Object,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenArticleInPendingReview_ShouldRecordStatusAndReason()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.CreatePendingReview(CategoryId);
        var command = new AdminRejectArticleCommand(
            Id: article.Id.ToString(),
            Reason: TestConstants.Article.ValidRejectionReason
        );
        _articleRepositoryMock.SetupGetByIdOrThrow(article);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        article.Status.Should().Be(EnumContentStatus.Rejected);
        article.RejectionReason.Should().Be(TestConstants.Article.ValidRejectionReason);
        _articleRepositoryMock.VerifyUpdateCalled(article);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenArticleInPendingReview_ShouldRaiseCommissionedContentRejectedEvent()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.CreatePendingReview(CategoryId);
        article.ClearDomainEvents();
        var command = new AdminRejectArticleCommand(
            Id: article.Id.ToString(),
            Reason: TestConstants.Article.ValidRejectionReason
        );
        _articleRepositoryMock.SetupGetByIdOrThrow(article);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        article
            .DomainEvents.OfType<CommissionedContentRejectedEvent>()
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(
                new CommissionedContentRejectedEvent(
                    ContentId: article.Id,
                    ContentType: EnumCoreContentType.Article,
                    CustomerId: article.CustomerId,
                    Title: article.Title,
                    Reason: TestConstants.Article.ValidRejectionReason
                )
            );
        article.DomainEvents.OfType<ArticleUnpublishedEvent>().Should().BeEmpty();
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
            Reason: TestConstants.Article.ValidRejectionReason
        );
        _articleRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task Handle_WhenArticleAlreadyRejected_ShouldThrowConflictException()
    {
        // Arrange
        ArticleEntity article = new ArticleBuilder(CategoryId).AsRejected().Build();
        article.ClearDomainEvents();
        var command = new AdminRejectArticleCommand(
            Id: article.Id.ToString(),
            Reason: TestConstants.Article.ValidRejectionReason
        );
        _articleRepositoryMock.SetupGetByIdOrThrow(article);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
        article.Status.Should().Be(EnumContentStatus.Rejected);
        article.DomainEvents.Should().BeEmpty();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task Handle_WhenArticleInWrongStatus_ShouldThrowBadRequestException()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        article.ClearDomainEvents();
        var command = new AdminRejectArticleCommand(
            Id: article.Id.ToString(),
            Reason: TestConstants.Article.ValidRejectionReason
        );
        _articleRepositoryMock.SetupGetByIdOrThrow(article);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
        article.Status.Should().Be(EnumContentStatus.Draft);
        article.DomainEvents.Should().BeEmpty();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    #endregion
}
