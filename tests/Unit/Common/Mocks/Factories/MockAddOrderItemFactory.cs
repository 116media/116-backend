using _116.Content.Application.Commerce.UseCases.Admin.Commands.AddOrderItem.Contracts;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using Moq;

namespace _116.Unit.Tests.Common.Mocks.Factories;

/// <summary>
/// Provides mock setup helpers for <see cref="IAddOrderItemFactory"/>.
/// </summary>
public static class MockAddOrderItemFactory
{
    /// <summary>
    /// Creates a new mock instance of IAddOrderItemFactory.
    /// </summary>
    public static Mock<IAddOrderItemFactory> Create() => new();

    public static Mock<IAddOrderItemFactory> SetupCreateItemAsync(
        this Mock<IAddOrderItemFactory> mock,
        (ContentOrderItemEntity Item, string CategoryName, string? PromotionLevelName) result
    )
    {
        mock.Setup(x =>
                x.CreateItemAsync(
                    It.IsAny<ContentOrderEntity>(),
                    It.IsAny<EnumCoreContentType>(),
                    It.IsAny<Guid>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(result);
        return mock;
    }

    public static Mock<IAddOrderItemFactory> SetupCreateItemAsyncThrows(
        this Mock<IAddOrderItemFactory> mock,
        Exception exception
    )
    {
        mock.Setup(x =>
                x.CreateItemAsync(
                    It.IsAny<ContentOrderEntity>(),
                    It.IsAny<EnumCoreContentType>(),
                    It.IsAny<Guid>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ThrowsAsync(exception);
        return mock;
    }

    public static void VerifyCreateItemCalled(this Mock<IAddOrderItemFactory> mock)
    {
        mock.Verify(
            x =>
                x.CreateItemAsync(
                    It.IsAny<ContentOrderEntity>(),
                    It.IsAny<EnumCoreContentType>(),
                    It.IsAny<Guid>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }
}
