using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using Moq;

namespace _116.Unit.Tests.Common.Mocks.Repositories;

/// <summary>
/// Provides mock setup helpers for <see cref="ITranslationRepository"/>.
/// </summary>
public static class MockTranslationRepository
{
    /// <summary>
    /// Creates a new mock instance of ITranslationRepository with safe default setups.
    /// </summary>
    public static Mock<ITranslationRepository> Create()
    {
        Mock<ITranslationRepository> mock = new();
        SetupDefaults(mock);
        return mock;
    }

    public static Mock<ITranslationRepository> SetupGetByIdOrThrow(
        this Mock<ITranslationRepository> mock,
        LyricsTranslationEntity entity
    )
    {
        mock.Setup(x => x.GetByIdOrThrowAsync(entity.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        return mock;
    }

    public static Mock<ITranslationRepository> SetupGetByIdOrThrowNotFound(
        this Mock<ITranslationRepository> mock,
        Guid id
    )
    {
        mock.Setup(x => x.GetByIdOrThrowAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Translation with id '{id}' was not found."));
        return mock;
    }

    public static Mock<ITranslationRepository> SetupGetByLyricsAndLanguage(
        this Mock<ITranslationRepository> mock,
        Guid lyricsId,
        string language,
        LyricsTranslationEntity? entity
    )
    {
        mock.Setup(x => x.GetByLyricsAndLanguageAsync(lyricsId, language, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        return mock;
    }

    public static Mock<ITranslationRepository> SetupGetAllByLyricsId(
        this Mock<ITranslationRepository> mock,
        Guid lyricsId,
        IReadOnlyList<LyricsTranslationEntity> translations
    )
    {
        mock.Setup(x => x.GetAllByLyricsIdAsync(lyricsId, It.IsAny<CancellationToken>())).ReturnsAsync(translations);
        return mock;
    }

    public static void VerifyAddCalled(this Mock<ITranslationRepository> mock)
    {
        mock.Verify(x => x.AddAsync(It.IsAny<LyricsTranslationEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Verifies that the repository was handed exactly the expected entity once,
    /// so updating a different instance than the one looked up fails the test.
    /// </summary>
    public static void VerifyUpdateCalled(this Mock<ITranslationRepository> mock, LyricsTranslationEntity expected)
    {
        mock.Verify(x => x.Update(expected), Times.Once);
    }

    public static void VerifyUpdateNotCalled(this Mock<ITranslationRepository> mock)
    {
        mock.Verify(x => x.Update(It.IsAny<LyricsTranslationEntity>()), Times.Never);
    }

    /// <summary>
    /// Installs defaults for write, void and aggregate members only. Identity lookups are left
    /// unconfigured so that a miss has to be arranged by the test, naming the identifier it is a
    /// miss for, rather than being asserted for every identifier before the test says anything.
    /// </summary>
    /// <param name="mock">The repository mock to configure.</param>
    private static void SetupDefaults(Mock<ITranslationRepository> mock)
    {
        mock.Setup(x => x.AddAsync(It.IsAny<LyricsTranslationEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(x => x.GetAllByLyricsIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<LyricsTranslationEntity>)new List<LyricsTranslationEntity>());
    }
}
