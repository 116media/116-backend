using _116.Content.Application.Interactions.EventHandlers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Events;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Interactions.EventHandlers;

/// <summary>
/// Unit tests for <see cref="CommentEngagementHandler"/>. The counter itself is applied in SQL,
/// so these assert the delta the handler forwards; the arithmetic is proven against the database
/// in the repository integration tests.
/// </summary>
public class CommentEngagementHandlerTests
{
    private readonly Mock<IArticleCommentRepository> _articleCommentRepositoryMock;
    private readonly CommentEngagementHandler _handler;

    public CommentEngagementHandlerTests()
    {
        _articleCommentRepositoryMock = MockArticleCommentRepository.Create();
        _handler = new CommentEngagementHandler(
            _articleCommentRepositoryMock.Object,
            NullLogger<CommentEngagementHandler>.Instance
        );
    }

    [Theory]
    [InlineData(1)]
    [InlineData(-1)]
    public async Task Handle_ShouldForwardTheDeltaThenSkipTracking(int delta)
    {
        // Arrange
        var commentId = Guid.NewGuid();
        _articleCommentRepositoryMock
            .Setup(x => x.ApplyCommentLikeDeltaAsync(commentId, delta, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(new CommentEngagedEvent(commentId, delta), CancellationToken.None);

        // Assert — loading to mutate is the race stage 8 removed; the counter moves in SQL only.
        _articleCommentRepositoryMock.Verify(
            x => x.ApplyCommentLikeDeltaAsync(commentId, delta, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _articleCommentRepositoryMock.Verify(
            x => x.GetCommentByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_WhenNoRowIsUpdated_ShouldNotThrow()
    {
        // Arrange
        // a race, not an error.
        var commentId = Guid.NewGuid();
        _articleCommentRepositoryMock
            .Setup(x => x.ApplyCommentLikeDeltaAsync(commentId, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        Func<Task> act = async () =>
            await _handler.Handle(new CommentEngagedEvent(commentId, 1), CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }
}
