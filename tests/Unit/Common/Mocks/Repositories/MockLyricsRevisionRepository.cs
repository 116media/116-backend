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

    public static void VerifyUpdateCalled(this Mock<ILyricsRevisionRepository> mock)
    {
        mock.Verify(x => x.Update(It.IsAny<LyricsRevisionEntity>()), Times.Once);
    }

    private static void SetupDefaults(Mock<ILyricsRevisionRepository> mock)
    {
        mock.Setup(x => x.AddAsync(It.IsAny<LyricsRevisionEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((LyricsRevisionEntity?)null);
    }
}
