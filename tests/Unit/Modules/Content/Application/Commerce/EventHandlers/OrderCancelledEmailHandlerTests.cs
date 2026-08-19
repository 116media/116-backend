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
/// Unit tests for <see cref="OrderCancelledEmailHandler"/>.
/// </summary>
public class OrderCancelledEmailHandlerTests
{
    private readonly Mock<IContentOrderRepository> _orderRepositoryMock;
    private readonly Mock<ICommerceCustomerNotifier> _notifierMock = new();
    private readonly OrderCancelledEmailHandler _handler;

    public OrderCancelledEmailHandlerTests()
    {
        _orderRepositoryMock = MockContentOrderRepository.Create();
        _handler = new OrderCancelledEmailHandler(
            _orderRepositoryMock.Object,
            _notifierMock.Object,
            NullLogger<OrderCancelledEmailHandler>.Instance
        );
    }

    [Fact]
    public async Task Handle_WithResolvedOrder_ShouldSendTheCancellationEmail()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.CreateCancelled();
        _orderRepositoryMock.SetupGetByIdWithItems(order);

        // Act
        await _handler.Handle(new OrderCancelledEvent(order.Id), CancellationToken.None);

        // Assert
        _notifierMock.Verify(x => x.NotifyOrderCancelledAsync(order, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenOrderNotFound_ShouldSkipTheEmail()
    {
        // Arrange — repository returns null by default

        // Act
        await _handler.Handle(new OrderCancelledEvent(Guid.NewGuid()), CancellationToken.None);

        // Assert
        _notifierMock.VerifyNoOtherCalls();
    }
}
