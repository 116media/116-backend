using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using Moq;

namespace _116.Unit.Tests.Common.Mocks.Repositories;

/// <summary>
/// Provides mock setup helpers for <see cref="ITranslationRevisionRepository"/>.
/// </summary>
public static class MockTranslationRevisionRepository
{
    /// <summary>
    /// Creates a new mock instance of ITranslationRevisionRepository with safe default setups.
    /// </summary>
    public static Mock<ITranslationRevisionRepository> Create()
    {
        Mock<ITranslationRevisionRepository> mock = new();
        SetupDefaults(mock);
        return mock;
    }

    public static Mock<ITranslationRevisionRepository> SetupGetByIdOrThrow(
        this Mock<ITranslationRevisionRepository> mock,
        LyricsTranslationRevisionEntity entity
    )
    {
        mock.Setup(x => x.GetByIdOrThrowAsync(entity.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        return mock;
    }

    public static Mock<ITranslationRevisionRepository> SetupGetByIdOrThrowNotFound(
        this Mock<ITranslationRevisionRepository> mock,
        Guid id
    )
    {
        mock.Setup(x => x.GetByIdOrThrowAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Translation revision with id '{id}' was not found."));
        return mock;
    }

    public static Mock<ITranslationRevisionRepository> SetupGetAllByTranslationId(
        this Mock<ITranslationRevisionRepository> mock,
        Guid translationId,
        IReadOnlyList<LyricsTranslationRevisionEntity> revisions
    )
    {
        mock.Setup(x => x.GetAllByTranslationIdAsync(translationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(revisions);
        return mock;
    }

    public static Mock<ITranslationRevisionRepository> SetupGetAcceptedButUnapplied(
        this Mock<ITranslationRevisionRepository> mock,
        IReadOnlyList<LyricsTranslationRevisionEntity> revisions
    )
    {
        mock.Setup(x => x.GetAcceptedButUnappliedAsync(It.IsAny<CancellationToken>())).ReturnsAsync(revisions);
        return mock;
    }

    public static void VerifyAddCalled(this Mock<ITranslationRevisionRepository> mock)
    {
        mock.Verify(
            x => x.AddAsync(It.IsAny<LyricsTranslationRevisionEntity>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    /// <summary>
    /// Verifies that the repository was handed exactly the expected entity once,
    /// so updating a different instance than the one looked up fails the test.
    /// </summary>
    public static void VerifyUpdateCalled(
        this Mock<ITranslationRevisionRepository> mock,
        LyricsTranslationRevisionEntity expected
    )
    {
        mock.Verify(x => x.Update(expected), Times.Once);
    }

    /// <summary>
    /// Installs defaults for write, void and aggregate members only. Identity lookups are left
    /// unconfigured so that a miss has to be arranged by the test, naming the identifier it is a
    /// miss for, rather than being asserted for every identifier before the test says anything.
    /// </summary>
    /// <param name="mock">The repository mock to configure.</param>
    private static void SetupDefaults(Mock<ITranslationRevisionRepository> mock)
    {
        mock.Setup(x => x.AddAsync(It.IsAny<LyricsTranslationRevisionEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(x => x.GetAllByTranslationIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<LyricsTranslationRevisionEntity>)new List<LyricsTranslationRevisionEntity>());
        mock.Setup(x => x.GetAcceptedButUnappliedAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<LyricsTranslationRevisionEntity>)new List<LyricsTranslationRevisionEntity>());
    }
}
