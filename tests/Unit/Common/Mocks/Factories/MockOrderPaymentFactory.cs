using _116.Content.Application.Commerce.Factories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using Moq;

namespace _116.Unit.Tests.Common.Mocks.Factories;

/// <summary>
/// Provides mock setup helpers for <see cref="IOrderPaymentFactory"/>.
/// </summary>
public static class MockOrderPaymentFactory
{
    /// <summary>
    /// Creates a new mock instance of IOrderPaymentFactory.
    /// </summary>
    public static Mock<IOrderPaymentFactory> Create() => new();

    public static Mock<IOrderPaymentFactory> SetupGetByOrderId(
        this Mock<IOrderPaymentFactory> mock,
        Guid orderId,
        ContentPaymentEntity payment
    )
    {
        mock.Setup(x => x.GetByOrderIdOrThrowAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync(payment);
        return mock;
    }

    public static Mock<IOrderPaymentFactory> SetupGetByOrderIdNotFound(
        this Mock<IOrderPaymentFactory> mock,
        Guid orderId
    )
    {
        mock.Setup(x => x.GetByOrderIdOrThrowAsync(orderId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Payment for order '{orderId}' was not found."));
        return mock;
    }

    public static void VerifyGetByOrderIdCalled(this Mock<IOrderPaymentFactory> mock, Guid orderId)
    {
        mock.Verify(x => x.GetByOrderIdOrThrowAsync(orderId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
