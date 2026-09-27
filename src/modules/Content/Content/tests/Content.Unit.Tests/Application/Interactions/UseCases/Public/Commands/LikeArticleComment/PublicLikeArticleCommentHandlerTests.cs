using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Interactions.UseCases.Public.Commands.LikeArticleComment;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.TestData;
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
using _116.Tests.TestData;
using _116.Tests.TestData.Helpers;
using _116.Tests.TestData.Mocks;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Interactions.UseCases.Public.Commands.LikeArticleComment;

/// <summary>
/// Unit tests for <see cref="PublicLikeArticleCommentHandler" />.
/// </summary>
public class PublicLikeArticleCommentHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IArticleCommentRepository> _articleCommentRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly PublicLikeArticleCommentHandler _handler;

    public PublicLikeArticleCommentHandlerTests()
    {
        _articleCommentRepositoryMock = MockArticleCommentRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new PublicLikeArticleCommentHandler(
            _articleCommentRepositoryMock.Object,
            _unitOfWorkMock.Object,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    [Fact]
    public async Task Handle_WhenNotYetLiked_ShouldAddLikeAndCommit()
    {
        ArticleCommentEntity comment = ArticleCommentFactory.Create(Guid.NewGuid(), Guid.NewGuid());
        var userId = Guid.NewGuid();
        _articleCommentRepositoryMock.SetupGetCommentByIdAsync(comment);
        _articleCommentRepositoryMock.SetupHasLikedCommentAsync(userId, comment.Id, result: false);

        var command = new PublicLikeArticleCommentCommand(comment.Id, userId);

        await _handler.Handle(command, CancellationToken.None);

        _articleCommentRepositoryMock.VerifyAddCommentLikeCalled(Times.Once());
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenAlreadyLiked_ShouldBeIdempotentNoOp()
    {
        ArticleCommentEntity comment = ArticleCommentFactory.Create(Guid.NewGuid(), Guid.NewGuid());
        var userId = Guid.NewGuid();
        _articleCommentRepositoryMock.SetupGetCommentByIdAsync(comment);
        _articleCommentRepositoryMock.SetupHasLikedCommentAsync(userId, comment.Id, result: true);

        var command = new PublicLikeArticleCommentCommand(comment.Id, userId);

        await _handler.Handle(command, CancellationToken.None);

        comment.LikeCount.Should().Be(0);
        _articleCommentRepositoryMock.VerifyAddCommentLikeCalled(Times.Never());
    }

    [Fact]
    public async Task Handle_WhenCommentNotFound_ShouldThrowNotFound()
    {
        Guid missingCommentId = Guid.NewGuid();
        _articleCommentRepositoryMock.SetupGetCommentByIdNotFound(missingCommentId);

        var command = new PublicLikeArticleCommentCommand(missingCommentId, Guid.NewGuid());

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
