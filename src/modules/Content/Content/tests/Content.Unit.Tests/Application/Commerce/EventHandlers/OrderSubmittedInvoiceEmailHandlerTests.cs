using _116.Content.Application.Commerce.EventHandlers;
using _116.Content.Application.Commerce.Services;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Events;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Factories.Helpers;
using _116.Content.TestData.Mocks.Factories;
using _116.Content.TestData.Mocks.Infrastructure;
using _116.Content.TestData.Mocks.Repositories;
using _116.Content.TestData.Mocks.Services;
using _116.Identity.TestData.Factories;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Identity.TestData.Mocks.Repositories;
using _116.Identity.TestData.Mocks.Services;
using _116.Storage.TestData.Factories;
using _116.Storage.TestData.Mocks.Infrastructure;
using _116.Storage.TestData.Mocks.Services;
using _116.Tests.TestData.Mocks;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Commerce.EventHandlers;

/// <summary>
/// Unit tests for <see cref="OrderSubmittedInvoiceEmailHandler"/>.
/// </summary>
public class OrderSubmittedInvoiceEmailHandlerTests
{
    private readonly Mock<IContentOrderRepository> _orderRepositoryMock;
    private readonly Mock<ICommerceCustomerNotifier> _notifierMock = new();
    private readonly OrderSubmittedInvoiceEmailHandler _handler;

    public OrderSubmittedInvoiceEmailHandlerTests()
    {
        _orderRepositoryMock = MockContentOrderRepository.Create();
        _handler = new OrderSubmittedInvoiceEmailHandler(
            _orderRepositoryMock.Object,
            _notifierMock.Object,
            NullLogger<OrderSubmittedInvoiceEmailHandler>.Instance
        );
    }

    [Fact]
    public async Task Handle_WithResolvedOrder_ShouldSendTheInvoiceEmail()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.CreateSubmitted();
        _orderRepositoryMock.SetupGetByIdWithItems(order);

        // Act
        await _handler.Handle(new OrderSubmittedEvent(order.Id), CancellationToken.None);

        // Assert
        _notifierMock.Verify(x => x.NotifyOrderInvoiceAsync(order, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenOrderNotFound_ShouldSkipTheEmail()
    {
        // Arrange

        // Act
        await _handler.Handle(new OrderSubmittedEvent(Guid.NewGuid()), CancellationToken.None);

        // Assert
        _notifierMock.VerifyNoOtherCalls();
    }
}
