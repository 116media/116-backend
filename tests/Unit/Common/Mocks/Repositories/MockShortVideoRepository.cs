using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using Moq;

namespace _116.Unit.Tests.Common.Mocks.Repositories;

/// <summary>
/// Provides mock setup helpers for <see cref="IShortVideoRepository"/>.
/// </summary>
public static class MockShortVideoRepository
{
    /// <summary>
    /// Creates a new mock instance of IShortVideoRepository with safe default setups.
    /// </summary>
    public static Mock<IShortVideoRepository> Create()
    {
        Mock<IShortVideoRepository> mock = new();
        SetupDefaults(mock);
        return mock;
    }

    public static Mock<IShortVideoRepository> SetupGetByIdOrThrow(
        this Mock<IShortVideoRepository> mock,
        ShortVideoEntity entity
    )
    {
        mock.Setup(x => x.GetByIdOrThrowAsync(entity.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        return mock;
    }

    public static Mock<IShortVideoRepository> SetupGetByIdOrThrowNotFound(
        this Mock<IShortVideoRepository> mock,
        Guid id
    )
    {
        mock.Setup(x => x.GetByIdOrThrowAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Short video with id '{id}' was not found."));
        return mock;
    }

    public static Mock<IShortVideoRepository> SetupGetByIdAsync(
        this Mock<IShortVideoRepository> mock,
        Guid id,
        ShortVideoEntity? entity
    )
    {
        mock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        return mock;
    }

    public static Mock<IShortVideoRepository> SetupGetBySlug(
        this Mock<IShortVideoRepository> mock,
        string slug,
        ShortVideoEntity? entity
    )
    {
        mock.Setup(x => x.GetBySlugAsync(slug, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        return mock;
    }

    public static Mock<IShortVideoRepository> SetupGetAllAsync(
        this Mock<IShortVideoRepository> mock,
        List<ShortVideoEntity> shortVideos,
        int totalCount
    )
    {
        mock.Setup(x =>
                x.GetAllAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((shortVideos, totalCount));
        return mock;
    }

    public static void VerifyAddCalled(this Mock<IShortVideoRepository> mock)
    {
        mock.Verify(x => x.AddAsync(It.IsAny<ShortVideoEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    public static void VerifyUpdateCalled(this Mock<IShortVideoRepository> mock)
    {
        mock.Verify(x => x.Update(It.IsAny<ShortVideoEntity>()), Times.Once);
    }

    public static void VerifyRemoveCalled(this Mock<IShortVideoRepository> mock, ShortVideoEntity shortVideo)
    {
        mock.Verify(x => x.Remove(shortVideo), Times.Once);
    }

    private static void SetupDefaults(Mock<IShortVideoRepository> mock)
    {
        mock.Setup(x => x.AddAsync(It.IsAny<ShortVideoEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(x => x.GetBySlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ShortVideoEntity?)null);
        mock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ShortVideoEntity?)null);
        mock.Setup(x =>
                x.GetAllAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((new List<ShortVideoEntity>(), 0));
    }
}
