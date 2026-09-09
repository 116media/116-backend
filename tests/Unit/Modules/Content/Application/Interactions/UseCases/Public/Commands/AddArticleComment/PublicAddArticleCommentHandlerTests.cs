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
    private readonly Mock<IArticleCommentRepository> _articleCommentRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly PublicAddArticleCommentHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public PublicAddArticleCommentHandlerTests()
    {
        _articleCommentRepositoryMock = MockArticleCommentRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new PublicAddArticleCommentHandler(_articleCommentRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenArticleExists_ShouldAddCommentAndCommit()
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
        _articleCommentRepositoryMock.SetupExistsOrThrow(article.Id);

        // Act
        PublicAddArticleCommentResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Comment.Body.Should().Be(command.Body);
        _articleCommentRepositoryMock.VerifyAddCommentCalled();
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
        _articleCommentRepositoryMock.SetupExistsOrThrowNotFound(articleId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
