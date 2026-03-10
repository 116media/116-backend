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

    public static Mock<IPackageRepository> SetupGetByIdWithSlotsOrThrow(
        this Mock<IPackageRepository> mock,
        PackageEntity entity
    )
    {
        mock.Setup(x => x.GetByIdWithSlotsOrThrowAsync(entity.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        return mock;
    }

    public static Mock<IPackageRepository> SetupGetByIdWithSlotsOrThrowNotFound(
        this Mock<IPackageRepository> mock,
        Guid id
    )
    {
        mock.Setup(x => x.GetByIdWithSlotsOrThrowAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Package with id '{id}' was not found."));
        return mock;
    }

    public static Mock<IPackageRepository> SetupGetSlotById(
        this Mock<IPackageRepository> mock,
        Guid slotId,
        PackageSlotEntity? slot
    )
    {
        mock.Setup(x => x.GetSlotByIdAsync(slotId, It.IsAny<CancellationToken>())).ReturnsAsync(slot);
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

    public static void VerifyAddSlotCalled(this Mock<IPackageRepository> mock)
    {
        mock.Verify(x => x.AddSlotAsync(It.IsAny<PackageSlotEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    public static void VerifyRemoveSlotCalled(this Mock<IPackageRepository> mock, PackageSlotEntity slot)
    {
        mock.Verify(x => x.RemoveSlot(slot), Times.Once);
    }

    private static void SetupDefaults(Mock<IPackageRepository> mock)
    {
        mock.Setup(x => x.AddAsync(It.IsAny<PackageEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(x => x.AddSlotAsync(It.IsAny<PackageSlotEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(x => x.GetSlotByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PackageSlotEntity?)null);
        mock.Setup(x =>
                x.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool?>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((new List<PackageEntity>(), 0));
    }
}
