using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using Moq;

namespace _116.Unit.Tests.Common.Mocks.Repositories;

/// <summary>
/// Provides mock setup helpers for <see cref="IPackageRepository"/>.
/// </summary>
public static class MockPackageRepository
{
    /// <summary>
    /// Creates a new mock instance of IPackageRepository with default setups.
    /// </summary>
    public static Mock<IPackageRepository> Create()
    {
        Mock<IPackageRepository> mock = new();
        SetupDefaults(mock);
        return mock;
    }

    public static Mock<IPackageRepository> SetupGetByIdOrThrow(this Mock<IPackageRepository> mock, PackageEntity entity)
    {
        mock.Setup(x => x.GetByIdOrThrowAsync(entity.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        return mock;
    }

    public static Mock<IPackageRepository> SetupGetByIdOrThrowNotFound(this Mock<IPackageRepository> mock, Guid id)
    {
        mock.Setup(x => x.GetByIdOrThrowAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Package with id '{id}' was not found."));
        return mock;
    }

    public static Mock<IPackageRepository> SetupGetAllAsync(
        this Mock<IPackageRepository> mock,
        List<PackageEntity> list,
        int totalCount
    )
    {
        mock.Setup(x =>
                x.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool?>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((list, totalCount));
        return mock;
    }

    public static void VerifyAddCalled(this Mock<IPackageRepository> mock)
    {
        mock.Verify(x => x.AddAsync(It.IsAny<PackageEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Installs defaults for write, void and aggregate members only. Identity lookups are left
    /// unconfigured so that a miss has to be arranged by the test, naming the identifier it is a
    /// miss for, rather than being asserted for every identifier before the test says anything.
    /// </summary>
    /// <param name="mock">The repository mock to configure.</param>
    private static void SetupDefaults(Mock<IPackageRepository> mock)
    {
        mock.Setup(x => x.AddAsync(It.IsAny<PackageEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(x =>
                x.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool?>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((new List<PackageEntity>(), 0));
    }
}
