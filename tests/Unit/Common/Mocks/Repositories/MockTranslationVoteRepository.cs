using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using Moq;

namespace _116.Unit.Tests.Common.Mocks.Repositories;

/// <summary>
/// Provides mock setup helpers for <see cref="ITranslationVoteRepository"/>.
/// </summary>
public static class MockTranslationVoteRepository
{
    /// <summary>
    /// Creates a new mock instance of ITranslationVoteRepository with safe default setups.
    /// </summary>
    public static Mock<ITranslationVoteRepository> Create()
    {
        Mock<ITranslationVoteRepository> mock = new();
        SetupDefaults(mock);
        return mock;
    }

    public static Mock<ITranslationVoteRepository> SetupHasVoted(
        this Mock<ITranslationVoteRepository> mock,
        Guid revisionId,
        Guid userId,
        bool hasVoted
    )
    {
        mock.Setup(x => x.HasVotedAsync(revisionId, userId, It.IsAny<CancellationToken>())).ReturnsAsync(hasVoted);
        return mock;
    }

    public static Mock<ITranslationVoteRepository> SetupGetNetApprovals(
        this Mock<ITranslationVoteRepository> mock,
        Guid revisionId,
        int netApprovals
    )
    {
        mock.Setup(x => x.GetNetApprovalsAsync(revisionId, It.IsAny<CancellationToken>())).ReturnsAsync(netApprovals);
        return mock;
    }

    public static void VerifyAddCalled(this Mock<ITranslationVoteRepository> mock)
    {
        mock.Verify(
            x => x.AddAsync(It.IsAny<LyricsTranslationVoteEntity>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    private static void SetupDefaults(Mock<ITranslationVoteRepository> mock)
    {
        mock.Setup(x => x.AddAsync(It.IsAny<LyricsTranslationVoteEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(x => x.HasVotedAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        mock.Setup(x => x.GetNetApprovalsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(0);
    }
}
