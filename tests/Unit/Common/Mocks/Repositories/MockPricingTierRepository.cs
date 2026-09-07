using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using Moq;

namespace _116.Unit.Tests.Common.Mocks.Repositories;

/// <summary>
/// Provides mock setup helpers for <see cref="IPricingTierRepository"/>.
/// </summary>
public static class MockPricingTierRepository
{
    /// <summary>
    /// Creates a new mock instance of IPricingTierRepository with default setups.
    /// </summary>
    public static Mock<IPricingTierRepository> Create()
    {
        Mock<IPricingTierRepository> mock = new();
        mock.Setup(x => x.ExistsByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        mock.Setup(x => x.AddAsync(It.IsAny<PricingTierEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(x => x.GetAllAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PricingTierEntity>());
        return mock;
    }

    public static Mock<IPricingTierRepository> SetupPricingTierExistsByName(
        this Mock<IPricingTierRepository> mock,
        string name,
        bool exists
    )
    {
        mock.Setup(x => x.ExistsByNameAsync(name, It.IsAny<CancellationToken>())).ReturnsAsync(exists);
        return mock;
    }

    public static Mock<IPricingTierRepository> SetupGetPricingTierByIdOrThrow(
        this Mock<IPricingTierRepository> mock,
        PricingTierEntity entity
    )
    {
        mock.Setup(x => x.GetByIdOrThrowAsync(entity.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        return mock;
    }

    public static Mock<IPricingTierRepository> SetupGetPricingTierByIdOrThrowNotFound(
        this Mock<IPricingTierRepository> mock,
        Guid id
    )
    {
        mock.Setup(x => x.GetByIdOrThrowAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"PricingTier with id '{id}' was not found."));
        return mock;
    }

    public static Mock<IPricingTierRepository> SetupGetAllPricingTiers(
        this Mock<IPricingTierRepository> mock,
        IReadOnlyList<PricingTierEntity> list
    )
    {
        mock.Setup(x => x.GetAllAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>())).ReturnsAsync(list);
        return mock;
    }

    public static void VerifyAddPricingTierCalled(this Mock<IPricingTierRepository> mock)
    {
        mock.Verify(x => x.AddAsync(It.IsAny<PricingTierEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    public static void VerifyAddPricingTierNotCalled(this Mock<IPricingTierRepository> mock)
    {
        mock.Verify(x => x.AddAsync(It.IsAny<PricingTierEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
