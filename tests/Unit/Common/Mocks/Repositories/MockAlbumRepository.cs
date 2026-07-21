using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using Moq;

namespace _116.Unit.Tests.Common.Mocks.Repositories;

/// <summary>
/// Provides mock setup helpers for <see cref="IAlbumRepository"/>.
/// </summary>
public static class MockAlbumRepository
{
    /// <summary>
    /// Creates a new mock instance of IAlbumRepository with safe default setups.
    /// </summary>
    public static Mock<IAlbumRepository> Create()
    {
        Mock<IAlbumRepository> mock = new();
        SetupDefaults(mock);
        return mock;
    }

    public static Mock<IAlbumRepository> SetupGetByIdOrThrow(this Mock<IAlbumRepository> mock, AlbumEntity entity)
    {
        mock.Setup(x => x.GetByIdOrThrowAsync(entity.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        return mock;
    }

    public static Mock<IAlbumRepository> SetupGetByIdOrThrowNotFound(this Mock<IAlbumRepository> mock, Guid id)
    {
        mock.Setup(x => x.GetByIdOrThrowAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Album with id '{id}' was not found."));
        return mock;
    }

    public static Mock<IAlbumRepository> SetupGetByIdAsync(
        this Mock<IAlbumRepository> mock,
        Guid id,
        AlbumEntity? entity
    )
    {
        mock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        return mock;
    }

    public static Mock<IAlbumRepository> SetupGetAllAsync(
        this Mock<IAlbumRepository> mock,
        List<AlbumEntity> albums,
        int totalCount
    )
    {
        mock.Setup(x =>
                x.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((albums, totalCount));
        return mock;
    }

    public static void VerifyAddCalled(this Mock<IAlbumRepository> mock)
    {
        mock.Verify(x => x.AddAsync(It.IsAny<AlbumEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    public static void VerifyAddNotCalled(this Mock<IAlbumRepository> mock)
    {
        mock.Verify(x => x.AddAsync(It.IsAny<AlbumEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    public static void VerifyUpdateCalled(this Mock<IAlbumRepository> mock)
    {
        mock.Verify(x => x.Update(It.IsAny<AlbumEntity>()), Times.Once);
    }

    private static void SetupDefaults(Mock<IAlbumRepository> mock)
    {
        mock.Setup(x => x.AddAsync(It.IsAny<AlbumEntity>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        mock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AlbumEntity?)null);
        mock.Setup(x =>
                x.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((new List<AlbumEntity>(), 0));
    }
}
