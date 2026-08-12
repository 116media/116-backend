using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Exceptions;
using Moq;

namespace _116.Unit.Tests.Common.Mocks.Repositories;

/// <summary>
/// Provides mock setup helpers for <see cref="IContentOrderRepository"/>.
/// </summary>
public static class MockContentOrderRepository
{
    /// <summary>
    /// Creates a new mock instance of IContentOrderRepository with default setups.
    /// </summary>
    public static Mock<IContentOrderRepository> Create()
    {
        Mock<IContentOrderRepository> mock = new();
        SetupDefaults(mock);
        return mock;
    }

    public static Mock<IContentOrderRepository> SetupGetByIdOrThrow(
        this Mock<IContentOrderRepository> mock,
        ContentOrderEntity order
    )
    {
        mock.Setup(x => x.GetByIdOrThrowAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        return mock;
    }

    public static Mock<IContentOrderRepository> SetupGetByIdOrThrowNotFound(
        this Mock<IContentOrderRepository> mock,
        Guid id
    )
    {
        mock.Setup(x => x.GetByIdOrThrowAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Order with id '{id}' was not found."));
        return mock;
    }

    public static Mock<IContentOrderRepository> SetupGetByIdWithItems(
        this Mock<IContentOrderRepository> mock,
        ContentOrderEntity? order
    )
    {
        mock.Setup(x => x.GetByIdWithItemsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(order);
        return mock;
    }

    public static Mock<IContentOrderRepository> SetupGetAllAsync(
        this Mock<IContentOrderRepository> mock,
        IReadOnlyList<ContentOrderEntity> list,
        int totalCount
    )
    {
        mock.Setup(x =>
                x.GetAllAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<EnumOrderStatus?>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<string?>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((list, totalCount));
        return mock;
    }

    public static Mock<IContentOrderRepository> SetupGetPaymentByOrderId(
        this Mock<IContentOrderRepository> mock,
        Guid orderId,
        ContentPaymentEntity? payment
    )
    {
        mock.Setup(x => x.GetPaymentByOrderIdAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync(payment);
        return mock;
    }

    public static Mock<IContentOrderRepository> SetupGetItemById(
        this Mock<IContentOrderRepository> mock,
        Guid orderId,
        Guid itemId,
        ContentOrderItemEntity? item
    )
    {
        mock.Setup(x => x.GetItemByIdAsync(orderId, itemId, It.IsAny<CancellationToken>())).ReturnsAsync(item);
        return mock;
    }

    /// <summary>
    /// Sets up the item lookup to answer only for the item's own order and item ids, so a handler
    /// that asks for an item under a different order is not silently handed this one.
    /// </summary>
    public static Mock<IContentOrderRepository> SetupGetItemByIdOrThrow(
        this Mock<IContentOrderRepository> mock,
        ContentOrderItemEntity item
    )
    {
        mock.Setup(x =>
                x.GetItemByIdOrThrowAsync(
                    It.Is<Guid>(id => id == item.OrderId),
                    It.Is<Guid>(id => id == item.Id),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(item);
        return mock;
    }

    /// <summary>
    /// Arranges a miss for the given order and item pair. Naming both identifiers keeps the
    /// not-found branch tied to the pair the test declares rather than to every pair.
    /// </summary>
    /// <param name="mock">The repository mock to configure.</param>
    /// <param name="orderId">The order the item is looked up under.</param>
    /// <param name="itemId">The item identifier that must resolve to nothing.</param>
    /// <returns>The same mock, for chaining.</returns>
    public static Mock<IContentOrderRepository> SetupGetItemByIdOrThrowNotFound(
        this Mock<IContentOrderRepository> mock,
        Guid orderId,
        Guid itemId
    )
    {
        mock.Setup(x => x.GetItemByIdOrThrowAsync(orderId, itemId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("ContentOrderItem", "id", itemId));
        return mock;
    }

    public static void VerifyAddCalled(this Mock<IContentOrderRepository> mock)
    {
        mock.Verify(x => x.AddAsync(It.IsAny<ContentOrderEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Verifies that the repository was handed exactly the expected order once,
    /// so updating a different instance than the one looked up fails the test.
    /// </summary>
    public static void VerifyUpdateCalled(this Mock<IContentOrderRepository> mock, ContentOrderEntity expected)
    {
        mock.Verify(x => x.UpdateAsync(expected, It.IsAny<CancellationToken>()), Times.Once);
    }

    public static void VerifyAddItemCalled(this Mock<IContentOrderRepository> mock)
    {
        mock.Verify(x => x.AddItemAsync(It.IsAny<ContentOrderItemEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    public static void VerifyAddItemTierCalled(this Mock<IContentOrderRepository> mock)
    {
        mock.Verify(
            x => x.AddItemTierAsync(It.IsAny<ContentItemTierEntity>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    public static void VerifyAddPaymentCalled(this Mock<IContentOrderRepository> mock)
    {
        mock.Verify(
            x => x.AddPaymentAsync(It.IsAny<ContentPaymentEntity>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    public static void VerifyUpdatePaymentCalled(this Mock<IContentOrderRepository> mock)
    {
        mock.Verify(
            x => x.UpdatePaymentAsync(It.IsAny<ContentPaymentEntity>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    public static Mock<IContentOrderRepository> SetupGetItemTierById(
        this Mock<IContentOrderRepository> mock,
        ContentItemTierEntity? tier
    )
    {
        mock.Setup(x => x.GetItemTierByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(tier);
        return mock;
    }

    /// <summary>
    /// Sets up the tier lookup to answer only for the tier's own order-item and tier ids, so a
    /// handler that asks for a tier under a different item is not silently handed this one.
    /// </summary>
    public static Mock<IContentOrderRepository> SetupGetItemTierByIdOrThrow(
        this Mock<IContentOrderRepository> mock,
        ContentItemTierEntity tier
    )
    {
        mock.Setup(x =>
                x.GetItemTierByIdOrThrowAsync(
                    It.Is<Guid>(id => id == tier.OrderItemId),
                    It.Is<Guid>(id => id == tier.Id),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(tier);
        return mock;
    }

    /// <summary>
    /// Arranges a miss for the given order-item and tier pair, so the not-found branch is reached
    /// only for the identifiers the test declares.
    /// </summary>
    /// <param name="mock">The repository mock to configure.</param>
    /// <param name="orderItemId">The order item the tier is looked up under.</param>
    /// <param name="tierId">The tier identifier that must resolve to nothing.</param>
    /// <returns>The same mock, for chaining.</returns>
    public static Mock<IContentOrderRepository> SetupGetItemTierByIdOrThrowNotFound(
        this Mock<IContentOrderRepository> mock,
        Guid orderItemId,
        Guid tierId
    )
    {
        mock.Setup(x => x.GetItemTierByIdOrThrowAsync(orderItemId, tierId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("ContentItemTier", "id", tierId));
        return mock;
    }

    public static void VerifyUpdateItemCalled(this Mock<IContentOrderRepository> mock)
    {
        mock.Verify(
            x => x.UpdateItemAsync(It.IsAny<ContentOrderItemEntity>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    public static void VerifyRemoveItemCalled(this Mock<IContentOrderRepository> mock)
    {
        mock.Verify(
            x => x.RemoveItemAsync(It.IsAny<ContentOrderItemEntity>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    public static void VerifyRemoveItemTierCalled(this Mock<IContentOrderRepository> mock)
    {
        mock.Verify(
            x => x.RemoveItemTierAsync(It.IsAny<ContentItemTierEntity>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    public static Mock<IContentOrderRepository> SetupGetOrderByItemId(
        this Mock<IContentOrderRepository> mock,
        Guid orderItemId,
        ContentOrderEntity? order
    )
    {
        mock.Setup(x => x.GetOrderByItemIdAsync(orderItemId, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        return mock;
    }

    public static Mock<IContentOrderRepository> SetupGetAllPaymentsAsync(
        this Mock<IContentOrderRepository> mock,
        IReadOnlyList<ContentPaymentEntity> list,
        int totalCount
    )
    {
        mock.Setup(x =>
                x.GetAllPaymentsAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<EnumPaymentStatus?>(),
                    It.IsAny<EnumPaymentMethod?>(),
                    It.IsAny<string?>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((list, totalCount));
        return mock;
    }

    /// <summary>
    /// Installs defaults for write, void and aggregate members only. Identity lookups are left
    /// unconfigured so that a miss has to be arranged by the test, naming the identifier it is a
    /// miss for, rather than being asserted for every identifier before the test says anything.
    /// </summary>
    /// <param name="mock">The repository mock to configure.</param>
    private static void SetupDefaults(Mock<IContentOrderRepository> mock)
    {
        mock.Setup(x => x.AddAsync(It.IsAny<ContentOrderEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(x => x.AddItemAsync(It.IsAny<ContentOrderItemEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(x => x.AddItemTierAsync(It.IsAny<ContentItemTierEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(x => x.AddPaymentAsync(It.IsAny<ContentPaymentEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(x => x.UpdateAsync(It.IsAny<ContentOrderEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(x => x.UpdatePaymentAsync(It.IsAny<ContentPaymentEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(x =>
                x.GetAllAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<EnumOrderStatus?>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<string?>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((new List<ContentOrderEntity>(), 0));
        mock.Setup(x =>
                x.GetAllPaymentsAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<EnumPaymentStatus?>(),
                    It.IsAny<EnumPaymentMethod?>(),
                    It.IsAny<string?>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((new List<ContentPaymentEntity>(), 0));
        mock.Setup(x => x.UpdateItemAsync(It.IsAny<ContentOrderItemEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(x => x.RemoveItemAsync(It.IsAny<ContentOrderItemEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(x => x.RemoveItemTierAsync(It.IsAny<ContentItemTierEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }
}
