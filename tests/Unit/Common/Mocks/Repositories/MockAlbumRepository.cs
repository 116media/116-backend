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

    /// <summary>
    /// Verifies that the repository was handed exactly the expected entity once,
    /// so updating a different instance than the one looked up fails the test.
    /// </summary>
    public static void VerifyUpdateCalled(this Mock<IAlbumRepository> mock, AlbumEntity expected)
    {
        mock.Verify(x => x.Update(expected), Times.Once);
    }

    /// <summary>
    /// Installs defaults for write, void and aggregate members only. Identity lookups are left
    /// unconfigured so that a miss has to be arranged by the test, naming the identifier it is a
    /// miss for, rather than being asserted for every identifier before the test says anything.
    /// </summary>
    /// <param name="mock">The repository mock to configure.</param>
    private static void SetupDefaults(Mock<IAlbumRepository> mock)
    {
        mock.Setup(x => x.AddAsync(It.IsAny<AlbumEntity>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        mock.Setup(x =>
                x.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((new List<AlbumEntity>(), 0));
    }
}
