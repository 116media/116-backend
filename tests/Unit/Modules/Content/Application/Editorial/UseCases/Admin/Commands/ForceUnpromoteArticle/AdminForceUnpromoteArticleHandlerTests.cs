using _116.Content.Application.Editorial.UseCases.Admin.Commands.ForceUnpromoteArticle;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Services;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.ForceUnpromoteArticle;

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
            _articleRepositoryMock.Object,
            _unitOfWorkMock.Object,
            currentActor,
            TestErrorsFactory.CreateContentI18n()
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
        result.ArticleId.Should().Be(article.Id);
        result.UnpromotedAt.Should().NotBe(default);
        _articleRepositoryMock.VerifyUpdateCalled();
        _unitOfWorkMock.VerifyCommitCalled();
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
    }

    #endregion
}
