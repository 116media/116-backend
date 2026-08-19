using _116.Content.Application.Interactions.UseCases.Public.Commands.LikeArticleComment;
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

namespace _116.Unit.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.LikeArticleComment;

/// <summary>
/// Unit tests for <see cref="PublicLikeArticleCommentHandler" />.
/// </summary>
public class PublicLikeArticleCommentHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IArticleRepository> _articleRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly PublicLikeArticleCommentHandler _handler;

    public PublicLikeArticleCommentHandlerTests()
    {
        _articleRepositoryMock = MockArticleRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new PublicLikeArticleCommentHandler(
            _articleRepositoryMock.Object,
            _unitOfWorkMock.Object,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    [Fact]
    public async Task Handle_WhenNotYetLiked_ShouldAddLikeAndCommit()
    {
        ArticleCommentEntity comment = ArticleCommentFactory.Create(Guid.NewGuid(), Guid.NewGuid());
        _articleRepositoryMock.SetupGetCommentByIdAsync(comment);
        _articleRepositoryMock.SetupHasLikedCommentAsync(false);

        var command = new PublicLikeArticleCommentCommand(comment.Id, Guid.NewGuid());

        PublicLikeArticleCommentResult result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _articleRepositoryMock.VerifyAddCommentLikeCalled(Times.Once());
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenAlreadyLiked_ShouldBeIdempotentNoOp()
    {
        ArticleCommentEntity comment = ArticleCommentFactory.Create(Guid.NewGuid(), Guid.NewGuid());
        _articleRepositoryMock.SetupGetCommentByIdAsync(comment);
        _articleRepositoryMock.SetupHasLikedCommentAsync(true);

        var command = new PublicLikeArticleCommentCommand(comment.Id, Guid.NewGuid());

        PublicLikeArticleCommentResult result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        comment.LikeCount.Should().Be(0);
        _articleRepositoryMock.VerifyAddCommentLikeCalled(Times.Never());
    }

    [Fact]
    public async Task Handle_WhenCommentNotFound_ShouldThrowNotFound()
    {
        _articleRepositoryMock.SetupGetCommentByIdAsync(null);

        var command = new PublicLikeArticleCommentCommand(Guid.NewGuid(), Guid.NewGuid());

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
