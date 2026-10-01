using _116.Content.Application.Commerce.UseCases.Admin.Commands.AddOrderItem.Contracts;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using Moq;

namespace _116.Content.TestData.Mocks.Services;

/// <summary>
/// Provides mock setup helpers for <see cref="IAddOrderItemService"/>.
/// </summary>
public static class MockAddOrderItemService
{
    /// <summary>
    /// Creates a new mock instance of IAddOrderItemService.
    /// </summary>
    public static Mock<IAddOrderItemService> Create() => new();

    public static Mock<IAddOrderItemService> SetupCreateItemAsync(
        this Mock<IAddOrderItemService> mock,
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
}
