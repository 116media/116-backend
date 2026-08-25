using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Exceptions;
using Moq;

namespace _116.Unit.Tests.Common.Mocks.Repositories;

/// <summary>
/// Provides mock setup helpers for <see cref="ILyricsSubmissionRepository"/>.
/// </summary>
public static class MockLyricsSubmissionRepository
{
    /// <summary>
    /// Creates a new mock instance of ILyricsSubmissionRepository with safe default setups.
    /// </summary>
    public static Mock<ILyricsSubmissionRepository> Create()
    {
        Mock<ILyricsSubmissionRepository> mock = new();
        SetupDefaults(mock);
        return mock;
    }

    public static Mock<ILyricsSubmissionRepository> SetupGetByIdOrThrow(
        this Mock<ILyricsSubmissionRepository> mock,
        LyricsSubmissionEntity entity
    )
    {
        mock.Setup(x => x.GetByIdOrThrowAsync(entity.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        return mock;
    }

    public static Mock<ILyricsSubmissionRepository> SetupGetByIdOrThrowNotFound(
        this Mock<ILyricsSubmissionRepository> mock,
        Guid id
    )
    {
        mock.Setup(x => x.GetByIdOrThrowAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"LyricsSubmission with id '{id}' was not found."));
        return mock;
    }

    public static Mock<ILyricsSubmissionRepository> SetupGetAllAsync(
        this Mock<ILyricsSubmissionRepository> mock,
        List<LyricsSubmissionEntity> submissions,
        int totalCount
    )
    {
        mock.Setup(x =>
                x.GetAllAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<EnumSubmissionStatus?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((submissions, totalCount));
        return mock;
    }

    public static Mock<ILyricsSubmissionRepository> SetupGetPendingWithMatchingLyrics(
        this Mock<ILyricsSubmissionRepository> mock,
        IReadOnlyList<LyricsSubmissionEntity> submissions
    )
    {
        mock.Setup(x => x.GetPendingWithMatchingLyricsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(submissions);
        return mock;
    }

    public static void VerifyAddCalled(this Mock<ILyricsSubmissionRepository> mock)
    {
        mock.Verify(x => x.AddAsync(It.IsAny<LyricsSubmissionEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    public static void VerifyAddNotCalled(this Mock<ILyricsSubmissionRepository> mock)
    {
        mock.Verify(x => x.AddAsync(It.IsAny<LyricsSubmissionEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Verifies that the repository was handed exactly the expected entity once,
    /// so updating a different instance than the one looked up fails the test.
    /// </summary>
    public static void VerifyUpdateCalled(this Mock<ILyricsSubmissionRepository> mock, LyricsSubmissionEntity expected)
    {
        mock.Verify(x => x.Update(expected), Times.Once);
    }

    /// <summary>
    /// Installs defaults for write, void and aggregate members only. Identity lookups are left
    /// unconfigured so that a miss has to be arranged by the test, naming the identifier it is a
    /// miss for, rather than being asserted for every identifier before the test says anything.
    /// </summary>
    /// <param name="mock">The repository mock to configure.</param>
    private static void SetupDefaults(Mock<ILyricsSubmissionRepository> mock)
    {
        mock.Setup(x => x.AddAsync(It.IsAny<LyricsSubmissionEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(x =>
                x.GetAllAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<EnumSubmissionStatus?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((new List<LyricsSubmissionEntity>(), 0));
        mock.Setup(x => x.GetPendingWithMatchingLyricsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<LyricsSubmissionEntity>)new List<LyricsSubmissionEntity>());
    }
}
