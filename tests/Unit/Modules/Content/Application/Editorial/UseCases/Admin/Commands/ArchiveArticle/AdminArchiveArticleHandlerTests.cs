using _116.Content.Application.Editorial.UseCases.Admin.Commands.ArchiveArticle;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Builders.Entities.Content;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.ArchiveArticle;

/// <summary>
/// Unit tests for <see cref="AdminArchiveArticleHandler"/>.
/// </summary>
public class AdminArchiveArticleHandlerTests
{
    private readonly Mock<IArticleRepository> _articleRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminArchiveArticleHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public AdminArchiveArticleHandlerTests()
    {
        _articleRepositoryMock = MockArticleRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminArchiveArticleHandler(
            _articleRepositoryMock.Object,
            _unitOfWorkMock.Object,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenArticleIsPublished_ShouldTransitionToArchived()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.CreatePublished(CategoryId);
        var command = new AdminArchiveArticleCommand(Id: article.Id.ToString());
        _articleRepositoryMock.SetupGetByIdOrThrow(article);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        article.Status.Should().Be(EnumContentStatus.Archived);
        _articleRepositoryMock.VerifyUpdateCalled(article);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenArticleIsPublished_ShouldRaiseArticleUnpublishedEvent()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.CreatePublished(CategoryId);
        article.ClearDomainEvents();
        var command = new AdminArchiveArticleCommand(Id: article.Id.ToString());
        _articleRepositoryMock.SetupGetByIdOrThrow(article);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        article
            .DomainEvents.OfType<ArticleUnpublishedEvent>()
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(new ArticleUnpublishedEvent(ArticleId: article.Id));
    }

    [Fact]
    public async Task Handle_WhenArticleIsNotPublished_ShouldArchiveWithoutUnpublishedEvent()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        article.ClearDomainEvents();
        var command = new AdminArchiveArticleCommand(Id: article.Id.ToString());
        _articleRepositoryMock.SetupGetByIdOrThrow(article);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        article.Status.Should().Be(EnumContentStatus.Archived);
        article.DomainEvents.Should().BeEmpty();
        _articleRepositoryMock.VerifyUpdateCalled(article);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenArticleNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        var command = new AdminArchiveArticleCommand(Id: nonExistentId.ToString());
        _articleRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task Handle_WhenArticleAlreadyArchived_ShouldNotChangeStateOrRaiseEvents()
    {
        // Arrange
        ArticleEntity article = new ArticleBuilder(CategoryId).AsArchived().Build();
        article.ClearDomainEvents();
        var command = new AdminArchiveArticleCommand(Id: article.Id.ToString());
        _articleRepositoryMock.SetupGetByIdOrThrow(article);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
        article.Status.Should().Be(EnumContentStatus.Archived);
        article.DomainEvents.Should().BeEmpty();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    #endregion
}
