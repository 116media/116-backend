using _116.Content.Application.Commerce.UseCases.Admin.Commands.AddItemTier.Contracts;
using _116.Content.Domain.Entities;
using Moq;

namespace _116.Unit.Tests.Common.Mocks.Factories;

/// <summary>
/// Provides mock setup helpers for <see cref="IAddItemTierFactory"/>.
/// </summary>
public static class MockAddItemTierFactory
{
    /// <summary>
    /// Creates a new mock instance of IAddItemTierFactory.
    /// </summary>
    public static Mock<IAddItemTierFactory> Create() => new();

    public static Mock<IAddItemTierFactory> SetupAttachTierAsync(
        this Mock<IAddItemTierFactory> mock,
        (ContentItemTierEntity Tier, string TierName) result
    )
    {
        mock.Setup(x =>
                x.AttachTierAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(result);
        return mock;
    }

    public static Mock<IAddItemTierFactory> SetupAttachTierAsyncThrows(
        this Mock<IAddItemTierFactory> mock,
        Exception exception
    )
    {
        mock.Setup(x =>
                x.AttachTierAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())
            )
            .ThrowsAsync(exception);
        return mock;
    }

    public static void VerifyAttachTierCalled(this Mock<IAddItemTierFactory> mock)
    {
        mock.Verify(
            x => x.AttachTierAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
