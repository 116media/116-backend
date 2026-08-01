using _116.Content.Application.Interactions.UseCases.Public.Commands.UnlikeArticleComment;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.UnlikeArticleComment;

/// <summary>
/// Unit tests for <see cref="PublicUnlikeArticleCommentHandler" />.
/// </summary>
public class PublicUnlikeArticleCommentHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IArticleRepository> _articleRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly PublicUnlikeArticleCommentHandler _handler;

    public PublicUnlikeArticleCommentHandlerTests()
    {
        _articleRepositoryMock = MockArticleRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new PublicUnlikeArticleCommentHandler(
            _articleRepositoryMock.Object,
            _unitOfWorkMock.Object,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    [Fact]
    public async Task Handle_WhenLiked_ShouldRemoveLikeAndCommit()
    {
        ArticleCommentEntity comment = ArticleCommentFactory.Create(Guid.NewGuid(), Guid.NewGuid());
        _articleRepositoryMock.SetupGetCommentByIdAsync(comment);
        _articleRepositoryMock.SetupHasLikedCommentAsync(true);

        var command = new PublicUnlikeArticleCommentCommand(comment.Id, Guid.NewGuid());

        PublicUnlikeArticleCommentResult result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        comment.LikeCount.Should().Be(0);
        _articleRepositoryMock.VerifyRemoveCommentLikeCalled(Times.Once());
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenNotLiked_ShouldBeIdempotentNoOp()
    {
        ArticleCommentEntity comment = ArticleCommentFactory.Create(Guid.NewGuid(), Guid.NewGuid());
        _articleRepositoryMock.SetupGetCommentByIdAsync(comment);
        _articleRepositoryMock.SetupHasLikedCommentAsync(false);

        var command = new PublicUnlikeArticleCommentCommand(comment.Id, Guid.NewGuid());

        PublicUnlikeArticleCommentResult result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        comment.LikeCount.Should().Be(0);
        _articleRepositoryMock.VerifyRemoveCommentLikeCalled(Times.Never());
    }

    [Fact]
    public async Task Handle_WhenCommentNotFound_ShouldThrowNotFound()
    {
        _articleRepositoryMock.SetupGetCommentByIdAsync(null);

        var command = new PublicUnlikeArticleCommentCommand(Guid.NewGuid(), Guid.NewGuid());

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
