using _116.Content.Application.Interactions.EventHandlers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Events;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Interactions.EventHandlers;

/// <summary>
/// Unit tests for <see cref="CommentEngagementHandler"/>.
/// </summary>
public class CommentEngagementHandlerTests
{
    private readonly Mock<IArticleRepository> _articleRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly CommentEngagementHandler _handler;

    public CommentEngagementHandlerTests()
    {
        _articleRepositoryMock = MockArticleRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new CommentEngagementHandler(
            _articleRepositoryMock.Object,
            _unitOfWorkMock.Object,
            NullLogger<CommentEngagementHandler>.Instance
        );
    }

    [Fact]
    public async Task Handle_WithPositiveDelta_ShouldIncrementLikeCountAndCommit()
    {
        // Arrange
        ArticleCommentEntity comment = ArticleCommentFactory.Create(Guid.NewGuid(), Guid.NewGuid());
        _articleRepositoryMock.SetupGetCommentByIdAsync(comment);

        // Act
        await _handler.Handle(new CommentEngagedEvent(comment.Id, 1), CancellationToken.None);

        // Assert
        comment.LikeCount.Should().Be(1);
        _articleRepositoryMock.VerifyUpdateCommentCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WithNegativeDelta_ShouldDecrementLikeCountAndCommit()
    {
        // Arrange
        ArticleCommentEntity comment = ArticleCommentFactory.Create(Guid.NewGuid(), Guid.NewGuid());
        comment.IncrementLikeCount();
        _articleRepositoryMock.SetupGetCommentByIdAsync(comment);

        // Act
        await _handler.Handle(new CommentEngagedEvent(comment.Id, -1), CancellationToken.None);

        // Assert
        comment.LikeCount.Should().Be(0);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenCommentMissing_ShouldSkipWithoutCommit()
    {
        // Arrange
        _articleRepositoryMock.SetupGetCommentByIdAsync(null);

        // Act
        await _handler.Handle(new CommentEngagedEvent(Guid.NewGuid(), 1), CancellationToken.None);

        // Assert
        _unitOfWorkMock.VerifyCommitNotCalled();
    }
}
