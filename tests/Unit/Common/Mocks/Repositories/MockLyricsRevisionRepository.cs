using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using Moq;

namespace _116.Unit.Tests.Common.Mocks.Repositories;

/// <summary>
/// Provides mock setup helpers for <see cref="ILyricsRevisionRepository"/>.
/// </summary>
public static class MockLyricsRevisionRepository
{
    /// <summary>
    /// Creates a new mock instance of ILyricsRevisionRepository with safe default setups.
    /// </summary>
    public static Mock<ILyricsRevisionRepository> Create()
    {
        Mock<ILyricsRevisionRepository> mock = new();
        SetupDefaults(mock);
        return mock;
    }

    public static Mock<ILyricsRevisionRepository> SetupGetByIdOrThrow(
        this Mock<ILyricsRevisionRepository> mock,
        LyricsRevisionEntity entity
    )
    {
        mock.Setup(x => x.GetByIdOrThrowAsync(entity.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        return mock;
    }

    public static Mock<ILyricsRevisionRepository> SetupGetByIdOrThrowNotFound(
        this Mock<ILyricsRevisionRepository> mock,
        Guid id
    )
    {
        mock.Setup(x => x.GetByIdOrThrowAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Lyrics revision with id '{id}' was not found."));
        return mock;
    }

    public static void VerifyAddCalled(this Mock<ILyricsRevisionRepository> mock)
    {
        mock.Verify(x => x.AddAsync(It.IsAny<LyricsRevisionEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Verifies that the repository was handed exactly the expected entity once,
    /// so updating a different instance than the one looked up fails the test.
    /// </summary>
    public static void VerifyUpdateCalled(this Mock<ILyricsRevisionRepository> mock, LyricsRevisionEntity expected)
    {
        mock.Verify(x => x.Update(expected), Times.Once);
    }

    /// <summary>
    /// Installs defaults for write, void and aggregate members only. Identity lookups are left
    /// unconfigured so that a miss has to be arranged by the test, naming the identifier it is a
    /// miss for, rather than being asserted for every identifier before the test says anything.
    /// </summary>
    /// <param name="mock">The repository mock to configure.</param>
    private static void SetupDefaults(Mock<ILyricsRevisionRepository> mock)
    {
        mock.Setup(x => x.AddAsync(It.IsAny<LyricsRevisionEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }
}
