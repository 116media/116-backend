using _116.BuildingBlocks.Application.Exceptions;
using _116.BuildingBlocks.Application.Services;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.ForceUnpromoteArticle;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Mocks.Infrastructure;
using _116.Content.TestData.Mocks.Repositories;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Storage.TestData.Mocks.Infrastructure;
using _116.Tests.TestData.Constants;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Editorial.UseCases.Admin.Commands.ForceUnpromoteArticle;

/// <summary>
/// Unit tests for <see cref="AdminForceUnpromoteArticleHandler"/>.
/// </summary>
public class AdminForceUnpromoteArticleHandlerTests
{
    private readonly Mock<IArticleRepository> _articleRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminForceUnpromoteArticleHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();
    private static readonly string ActorUserId = Guid.NewGuid().ToString();

    public AdminForceUnpromoteArticleHandlerTests()
    {
        _articleRepositoryMock = MockArticleRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();

        ICurrentActor currentActor = Mock.Of<ICurrentActor>(a =>
            a.UserId == ActorUserId && a.IsAuthenticated == true && a.HasHttpContext == true
        );

        _handler = new AdminForceUnpromoteArticleHandler(
            new AdminForceUnpromoteArticleService(
                _articleRepositoryMock.Object,
                currentActor,
                TestErrorsFactory.CreateContentI18n(),
                TimeProvider.System
            ),
            _unitOfWorkMock.Object
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenArticleIsPromoted_ShouldUnpromoteAndReturnResult()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.CreatePromoted(CategoryId);
        var command = new AdminForceUnpromoteArticleCommand(
            Slug: article.Slug,
            Reason: TestConstants.Article.ValidRejectionReason
        );
        _articleRepositoryMock.SetupGetBySlug(article.Slug, article);

        // Act
        AdminForceUnpromoteArticleResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        article.IsPromoted.Should().BeFalse();
        article.PromotedUntil.Should().BeNull();
        article.PromotionLevelId.Should().BeNull();
        article.UnpromotedBy.Should().Be(ActorUserId);
        article.UnpromotedReason.Should().Be(TestConstants.Article.ValidRejectionReason);
        article.UnpromotedAt.Should().NotBeNull();
        result.ArticleId.Should().Be(article.Id);
        result.UnpromotedAt.Should().Be(article.UnpromotedAt);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenArticleIsPromoted_ShouldRaiseContentPromotionRemovedEvent()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.CreatePromoted(CategoryId);
        article.ClearDomainEvents();
        var command = new AdminForceUnpromoteArticleCommand(
            Slug: article.Slug,
            Reason: TestConstants.Article.ValidRejectionReason
        );
        _articleRepositoryMock.SetupGetBySlug(article.Slug, article);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        article
            .DomainEvents.OfType<ContentPromotionRemovedEvent>()
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(
                new ContentPromotionRemovedEvent(
                    ContentId: article.Id,
                    ContentType: EnumCoreContentType.Article,
                    CustomerId: article.CustomerId,
                    Title: article.Title,
                    Reason: TestConstants.Article.ValidRejectionReason
                )
            );
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenArticleNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new AdminForceUnpromoteArticleCommand(
            Slug: "non-existent-slug",
            Reason: TestConstants.Article.ValidRejectionReason
        );
        _articleRepositoryMock.SetupGetBySlug("non-existent-slug", null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    #endregion
}
