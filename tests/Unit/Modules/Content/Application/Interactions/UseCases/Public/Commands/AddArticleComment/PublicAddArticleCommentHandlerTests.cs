using _116.Content.Application.Interactions.UseCases.Public.Commands.AddArticleComment;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.AddArticleComment;

/// <summary>
/// Unit tests for <see cref="PublicAddArticleCommentHandler"/>.
/// </summary>
public class PublicAddArticleCommentHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IArticleRepository> _articleRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly PublicAddArticleCommentHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public PublicAddArticleCommentHandlerTests()
    {
        _articleRepositoryMock = MockArticleRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new PublicAddArticleCommentHandler(_articleRepositoryMock.Object, _unitOfWorkMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenArticleExists_ShouldAddCommentIncrementCountAndCommit()
    {
        // Arrange
        Guid articleId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        ArticleEntity article = ArticleFactory.CreatePublished(CategoryId);
        var command = new PublicAddArticleCommentCommand(
            ArticleId: article.Id,
            UserId: userId,
            Body: "This is a valid test comment body."
        );
        _articleRepositoryMock.SetupGetByIdOrThrow(article);

        // Act
        PublicAddArticleCommentResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Comment.Should().NotBeNull();
        _articleRepositoryMock.VerifyAddCommentCalled();
        _articleRepositoryMock.VerifyUpdateCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenArticleNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid articleId = Guid.NewGuid();
        var command = new PublicAddArticleCommentCommand(
            ArticleId: articleId,
            UserId: Guid.NewGuid(),
            Body: "This is a valid test comment body."
        );
        _articleRepositoryMock.SetupGetByIdOrThrowNotFound(articleId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
