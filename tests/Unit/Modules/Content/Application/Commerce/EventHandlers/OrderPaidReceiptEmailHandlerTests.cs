using _116.Content.Application.Commerce.EventHandlers;
using _116.Content.Application.Commerce.Services;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Events;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common.Mocks.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.EventHandlers;

/// <summary>
/// Unit tests for <see cref="OrderPaidReceiptEmailHandler"/>.
/// </summary>
public class OrderPaidReceiptEmailHandlerTests
{
    private readonly Mock<IContentOrderRepository> _orderRepositoryMock;
    private readonly Mock<ICommerceCustomerNotifier> _notifierMock = new();
    private readonly OrderPaidReceiptEmailHandler _handler;

    public OrderPaidReceiptEmailHandlerTests()
    {
        _orderRepositoryMock = MockContentOrderRepository.Create();
        _handler = new OrderPaidReceiptEmailHandler(
            _orderRepositoryMock.Object,
            _notifierMock.Object,
            NullLogger<OrderPaidReceiptEmailHandler>.Instance
        );
    }

    [Fact]
    public async Task Handle_WithResolvedOrderAndPayment_ShouldSendTheReceiptEmail()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.CreatePaid();
        ContentPaymentEntity payment = ContentPaymentFactory.CreateVerified(order.Id);
        _orderRepositoryMock.SetupGetByIdWithItems(order);
        _orderRepositoryMock.SetupGetPaymentByOrderId(order.Id, payment);

        // Act
        await _handler.Handle(
            new OrderPaidEvent(order.Id, payment.Id, DateTimeOffset.UtcNow, Array.Empty<PaidItemEffect>()),
            CancellationToken.None
        );

        // Assert
        _notifierMock.Verify(
            x => x.NotifyPaymentReceiptAsync(order, payment, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WhenOrderNotFound_ShouldSkipTheEmail()
    {
        // Arrange — repository returns null by default

        // Act
        await _handler.Handle(
            new OrderPaidEvent(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow, Array.Empty<PaidItemEffect>()),
            CancellationToken.None
        );

        // Assert
        _notifierMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenPaymentNotFound_ShouldSkipTheEmail()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.CreatePaid();
        _orderRepositoryMock.SetupGetByIdWithItems(order);

        // Act
        await _handler.Handle(
            new OrderPaidEvent(order.Id, Guid.NewGuid(), DateTimeOffset.UtcNow, Array.Empty<PaidItemEffect>()),
            CancellationToken.None
        );

        // Assert
        _notifierMock.VerifyNoOtherCalls();
    }
}
