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

    public static void VerifyAddCalled(this Mock<IContentOrderRepository> mock)
    {
        mock.Verify(x => x.AddAsync(It.IsAny<ContentOrderEntity>(), It.IsAny<CancellationToken>()), Times.Once);
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

    public static Mock<IContentOrderRepository> SetupGetOrdersWithPaymentAsync(
        this Mock<IContentOrderRepository> mock,
        IReadOnlyList<ContentOrderEntity> list,
        int totalCount
    )
    {
        mock.Setup(x =>
                x.GetOrdersWithPaymentAsync(
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
                x.GetOrdersWithPaymentAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<EnumPaymentStatus?>(),
                    It.IsAny<EnumPaymentMethod?>(),
                    It.IsAny<string?>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((new List<ContentOrderEntity>(), 0));
    }
}
