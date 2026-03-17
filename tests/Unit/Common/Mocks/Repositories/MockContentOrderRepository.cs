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

    public static void VerifyAddCalled(this Mock<IContentOrderRepository> mock)
    {
        mock.Verify(x => x.AddAsync(It.IsAny<ContentOrderEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    public static void VerifyUpdateCalled(this Mock<IContentOrderRepository> mock)
    {
        mock.Verify(x => x.UpdateAsync(It.IsAny<ContentOrderEntity>(), It.IsAny<CancellationToken>()), Times.Once);
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
        mock.Setup(x => x.GetByIdWithItemsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ContentOrderEntity?)null);
        mock.Setup(x => x.GetPaymentByOrderIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ContentPaymentEntity?)null);
        mock.Setup(x => x.GetItemByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ContentOrderItemEntity?)null);
        mock.Setup(x =>
                x.GetAllAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<EnumOrderStatus?>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((new List<ContentOrderEntity>(), 0));
    }
}
