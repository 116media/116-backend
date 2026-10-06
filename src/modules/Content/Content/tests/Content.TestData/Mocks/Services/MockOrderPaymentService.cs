using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Commerce.Services;
using _116.Content.Domain.Entities;
using Moq;

namespace _116.Content.TestData.Mocks.Services;

/// <summary>
/// Provides mock setup helpers for <see cref="IOrderPaymentService"/>.
/// </summary>
public static class MockOrderPaymentService
{
    /// <summary>
    /// Creates a new mock instance of IOrderPaymentService.
    /// </summary>
    public static Mock<IOrderPaymentService> Create() => new();

    public static Mock<IOrderPaymentService> SetupGetByOrderId(
        this Mock<IOrderPaymentService> mock,
        Guid orderId,
        ContentPaymentEntity payment
    )
    {
        mock.Setup(x => x.GetByOrderIdOrThrowAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync(payment);
        return mock;
    }

    public static Mock<IOrderPaymentService> SetupGetByOrderIdNotFound(
        this Mock<IOrderPaymentService> mock,
        Guid orderId
    )
    {
        mock.Setup(x => x.GetByOrderIdOrThrowAsync(orderId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Payment for order '{orderId}' was not found."));
        return mock;
    }
}
