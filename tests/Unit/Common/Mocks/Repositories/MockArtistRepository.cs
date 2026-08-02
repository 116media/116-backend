using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using Moq;

namespace _116.Unit.Tests.Common.Mocks.Repositories;

/// <summary>
/// Provides mock setup helpers for <see cref="IArtistRepository"/>.
/// </summary>
public static class MockArtistRepository
{
    /// <summary>
    /// Creates a new mock instance of IArtistRepository with safe default setups.
    /// </summary>
    public static Mock<IArtistRepository> Create()
    {
        Mock<IArtistRepository> mock = new();
        SetupDefaults(mock);
        return mock;
    }

    public static Mock<IArtistRepository> SetupGetByIdOrThrow(this Mock<IArtistRepository> mock, ArtistEntity entity)
    {
        mock.Setup(x => x.GetByIdOrThrowAsync(entity.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        return mock;
    }

    public static Mock<IArtistRepository> SetupGetByIdOrThrowNotFound(this Mock<IArtistRepository> mock, Guid id)
    {
        mock.Setup(x => x.GetByIdOrThrowAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Artist with id '{id}' was not found."));
        return mock;
    }

    public static Mock<IArtistRepository> SetupGetByIdAsync(
        this Mock<IArtistRepository> mock,
        Guid id,
        ArtistEntity? entity
    )
    {
        mock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        return mock;
    }

    public static Mock<IArtistRepository> SetupGetBySlug(
        this Mock<IArtistRepository> mock,
        string slug,
        ArtistEntity? entity
    )
    {
        mock.Setup(x => x.GetBySlugAsync(slug, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        return mock;
    }

    public static Mock<IArtistRepository> SetupGetByUserId(
        this Mock<IArtistRepository> mock,
        Guid userId,
        ArtistEntity? entity
    )
    {
        mock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        return mock;
    }

    public static Mock<IArtistRepository> SetupGetAllAsync(
        this Mock<IArtistRepository> mock,
        List<ArtistEntity> artists,
        int totalCount
    )
    {
        mock.Setup(x =>
                x.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((artists, totalCount));
        return mock;
    }

    public static void VerifyAddCalled(this Mock<IArtistRepository> mock)
    {
        mock.Verify(x => x.AddAsync(It.IsAny<ArtistEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    public static void VerifyAddNotCalled(this Mock<IArtistRepository> mock)
    {
        mock.Verify(x => x.AddAsync(It.IsAny<ArtistEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    public static void VerifyUpdateCalled(this Mock<IArtistRepository> mock)
    {
        mock.Verify(x => x.Update(It.IsAny<ArtistEntity>()), Times.Once);
    }

    private static void SetupDefaults(Mock<IArtistRepository> mock)
    {
        mock.Setup(x => x.AddAsync(It.IsAny<ArtistEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(x => x.GetBySlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ArtistEntity?)null);
        mock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ArtistEntity?)null);
        mock.Setup(x => x.GetByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ArtistEntity?)null);
        mock.Setup(x =>
                x.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((new List<ArtistEntity>(), 0));
    }
}
