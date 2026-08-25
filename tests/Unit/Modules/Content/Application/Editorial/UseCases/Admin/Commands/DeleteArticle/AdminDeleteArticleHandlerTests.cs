using _116.Content.Application.Editorial.UseCases.Admin.Commands.DeleteArticle;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
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

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.DeleteArticle;

/// <summary>
/// Unit tests for <see cref="AdminDeleteArticleHandler"/>.
/// </summary>
public class AdminDeleteArticleHandlerTests
{
    private readonly Mock<IArticleRepository> _articleRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminDeleteArticleHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public AdminDeleteArticleHandlerTests()
    {
        _articleRepositoryMock = MockArticleRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminDeleteArticleHandler(
            _articleRepositoryMock.Object,
            _unitOfWorkMock.Object,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenDraftArticleWithNoImages_ShouldDeleteAndReturnSuccess()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        var command = new AdminDeleteArticleCommand(Id: article.Id.ToString());

        _articleRepositoryMock.SetupGetByIdOrThrow(article);
        _articleRepositoryMock.SetupGetImagesByArticleId(article.Id, new List<ArticleImageEntity>());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _articleRepositoryMock.VerifyRemoveCalled(article);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenDraftArticleWithImages_ShouldRaiseDeletionEventWithCapturedKeys()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        List<ArticleImageEntity> images = ArticleImageFactory.CreateMany(article.Id, 2);
        var command = new AdminDeleteArticleCommand(Id: article.Id.ToString());

        _articleRepositoryMock.SetupGetByIdOrThrow(article);
        _articleRepositoryMock.SetupGetImagesByArticleId(article.Id, images);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        article
            .DomainEvents.OfType<ArticleDeletedEvent>()
            .Should()
            .ContainSingle()
            .Which.BodyImageStorageKeys.Should()
            .BeEquivalentTo(images.Select(img => img.StorageKey));
        _articleRepositoryMock.VerifyRemoveCalled(article);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenArticleHasACoverImageRow_ShouldCaptureOnlyTheBodyImageKeys()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        ArticleImageEntity cover = ArticleImageFactory.CreateCover(article.Id);
        ArticleImageEntity body = ArticleImageFactory.CreateBody(article.Id);
        var command = new AdminDeleteArticleCommand(Id: article.Id.ToString());

        _articleRepositoryMock.SetupGetByIdOrThrow(article);
        _articleRepositoryMock.SetupGetImagesByArticleId(article.Id, [cover, body]);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        article
            .DomainEvents.OfType<ArticleDeletedEvent>()
            .Should()
            .ContainSingle()
            .Which.BodyImageStorageKeys.Should()
            .BeEquivalentTo([body.StorageKey]);
    }

    [Fact]
    public async Task Handle_WhenRejectedArticle_ShouldDeleteAndReturnSuccess()
    {
        // Arrange
        ArticleEntity article = new ArticleBuilder(CategoryId).AsRejected().Build();
        var command = new AdminDeleteArticleCommand(Id: article.Id.ToString());

        _articleRepositoryMock.SetupGetByIdOrThrow(article);
        _articleRepositoryMock.SetupGetImagesByArticleId(article.Id, new List<ArticleImageEntity>());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _articleRepositoryMock.VerifyRemoveCalled(article);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenArticleNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        var command = new AdminDeleteArticleCommand(Id: nonExistentId.ToString());
        _articleRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenArticleIsPublished_ShouldThrowBadRequestException()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.CreatePublished(CategoryId);
        var command = new AdminDeleteArticleCommand(Id: article.Id.ToString());
        _articleRepositoryMock.SetupGetByIdOrThrow(article);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    #endregion
}
