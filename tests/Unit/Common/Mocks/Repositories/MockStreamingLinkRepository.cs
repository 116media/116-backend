using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using Moq;

namespace _116.Unit.Tests.Common.Mocks.Repositories;

/// <summary>
/// Provides mock setup helpers for <see cref="IStreamingLinkRepository"/>.
/// </summary>
public static class MockStreamingLinkRepository
{
    /// <summary>
    /// Creates a new mock instance of IStreamingLinkRepository with safe default setups.
    /// </summary>
    public static Mock<IStreamingLinkRepository> Create()
    {
        Mock<IStreamingLinkRepository> mock = new();
        SetupDefaults(mock);
        return mock;
    }

    public static Mock<IStreamingLinkRepository> SetupGetByAlbumAndPlatformAsync(
        this Mock<IStreamingLinkRepository> mock,
        Guid albumId,
        EnumStreamingPlatform platform,
        StreamingLinkEntity? entity
    )
    {
        mock.Setup(x => x.GetByAlbumAndPlatformAsync(albumId, platform, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        return mock;
    }

    public static Mock<IStreamingLinkRepository> SetupGetByAlbumAsync(
        this Mock<IStreamingLinkRepository> mock,
        Guid albumId,
        IReadOnlyDictionary<EnumStreamingPlatform, string> curated
    )
    {
        mock.Setup(x => x.GetByAlbumAsync(albumId, It.IsAny<CancellationToken>())).ReturnsAsync(curated);
        return mock;
    }

    public static Mock<IStreamingLinkRepository> SetupGetByLyricsAndPlatformAsync(
        this Mock<IStreamingLinkRepository> mock,
        Guid lyricsId,
        EnumStreamingPlatform platform,
        StreamingLinkEntity? entity
    )
    {
        mock.Setup(x => x.GetByLyricsAndPlatformAsync(lyricsId, platform, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        return mock;
    }

    public static Mock<IStreamingLinkRepository> SetupGetByLyricsAsync(
        this Mock<IStreamingLinkRepository> mock,
        Guid lyricsId,
        IReadOnlyDictionary<EnumStreamingPlatform, string> curated
    )
    {
        mock.Setup(x => x.GetByLyricsAsync(lyricsId, It.IsAny<CancellationToken>())).ReturnsAsync(curated);
        return mock;
    }

    public static void VerifyAddCalled(this Mock<IStreamingLinkRepository> mock)
    {
        mock.Verify(x => x.AddAsync(It.IsAny<StreamingLinkEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    public static void VerifyUpdateCalled(this Mock<IStreamingLinkRepository> mock)
    {
        mock.Verify(x => x.Update(It.IsAny<StreamingLinkEntity>()), Times.Once);
    }

    public static void VerifyRemoveCalled(this Mock<IStreamingLinkRepository> mock)
    {
        mock.Verify(x => x.Remove(It.IsAny<StreamingLinkEntity>()), Times.Once);
    }

    private static void SetupDefaults(Mock<IStreamingLinkRepository> mock)
    {
        mock.Setup(x =>
                x.GetByAlbumAndPlatformAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<EnumStreamingPlatform>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((StreamingLinkEntity?)null);
        mock.Setup(x => x.GetByAlbumAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<EnumStreamingPlatform, string>());
        mock.Setup(x =>
                x.GetByLyricsAndPlatformAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<EnumStreamingPlatform>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((StreamingLinkEntity?)null);
        mock.Setup(x => x.GetByLyricsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<EnumStreamingPlatform, string>());
        mock.Setup(x => x.AddAsync(It.IsAny<StreamingLinkEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }
}
