using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using Moq;

namespace _116.Unit.Tests.Common.Mocks.Repositories;

/// <summary>
/// Provides mock setup helpers for <see cref="ILyricsRepository"/>.
/// </summary>
public static class MockLyricsRepository
{
    /// <summary>
    /// Creates a new mock instance of ILyricsRepository with safe default setups.
    /// </summary>
    public static Mock<ILyricsRepository> Create()
    {
        Mock<ILyricsRepository> mock = new();
        SetupDefaults(mock);
        return mock;
    }

    public static Mock<ILyricsRepository> SetupGetByIdOrThrow(this Mock<ILyricsRepository> mock, LyricsEntity entity)
    {
        mock.Setup(x => x.GetByIdOrThrowAsync(entity.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        return mock;
    }

    public static Mock<ILyricsRepository> SetupGetByIdOrThrowNotFound(this Mock<ILyricsRepository> mock, Guid id)
    {
        mock.Setup(x => x.GetByIdOrThrowAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Lyrics with id '{id}' was not found."));
        return mock;
    }

    public static Mock<ILyricsRepository> SetupGetByIdAsync(
        this Mock<ILyricsRepository> mock,
        Guid id,
        LyricsEntity? entity
    )
    {
        mock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        return mock;
    }

    public static Mock<ILyricsRepository> SetupGetBySongTitleAndArtist(
        this Mock<ILyricsRepository> mock,
        string songTitle,
        string artistName,
        LyricsEntity? entity
    )
    {
        mock.Setup(x => x.GetBySongTitleAndArtistAsync(songTitle, artistName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        return mock;
    }

    public static Mock<ILyricsRepository> SetupGetAllAsync(
        this Mock<ILyricsRepository> mock,
        List<LyricsEntity> lyrics,
        int totalCount
    )
    {
        mock.Setup(x =>
                x.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((lyrics, totalCount));
        return mock;
    }

    public static void VerifyAddCalled(this Mock<ILyricsRepository> mock)
    {
        mock.Verify(x => x.AddAsync(It.IsAny<LyricsEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    public static void VerifyUpdateCalled(this Mock<ILyricsRepository> mock)
    {
        mock.Verify(x => x.Update(It.IsAny<LyricsEntity>()), Times.Once);
    }

    private static void SetupDefaults(Mock<ILyricsRepository> mock)
    {
        mock.Setup(x => x.AddAsync(It.IsAny<LyricsEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(x =>
                x.GetBySongTitleAndArtistAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((LyricsEntity?)null);
        mock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((LyricsEntity?)null);
        mock.Setup(x =>
                x.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((new List<LyricsEntity>(), 0));
    }
}
