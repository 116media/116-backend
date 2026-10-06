using _116.Content.Application.Commerce.UseCases.Admin.Commands.AddItemTier.Contracts;
using _116.Content.Domain.Entities;
using Moq;

namespace _116.Content.TestData.Mocks.Services;

/// <summary>
/// Provides mock setup helpers for <see cref="IAddItemTierService"/>.
/// </summary>
public static class MockAddItemTierService
{
    /// <summary>
    /// Creates a new mock instance of IAddItemTierService.
    /// </summary>
    public static Mock<IAddItemTierService> Create() => new();

    public static Mock<IAddItemTierService> SetupAttachTierAsync(
        this Mock<IAddItemTierService> mock,
        (ContentItemTierEntity Tier, string TierName) result
    )
    {
        mock.Setup(x =>
                x.AttachTierAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(result);
        return mock;
    }

    public static Mock<IAddItemTierService> SetupAttachTierAsyncThrows(
        this Mock<IAddItemTierService> mock,
        Exception exception
    )
    {
        mock.Setup(x =>
                x.AttachTierAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())
            )
            .ThrowsAsync(exception);
        return mock;
    }
}
