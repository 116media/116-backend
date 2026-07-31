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
/// Unit tests for <see cref="PaymentRejectedEmailHandler"/>.
/// </summary>
public class PaymentRejectedEmailHandlerTests
{
    private readonly Mock<IContentOrderRepository> _orderRepositoryMock;
    private readonly Mock<ICommerceCustomerNotifier> _notifierMock = new();
    private readonly PaymentRejectedEmailHandler _handler;

    public PaymentRejectedEmailHandlerTests()
    {
        _orderRepositoryMock = MockContentOrderRepository.Create();
        _handler = new PaymentRejectedEmailHandler(
            _orderRepositoryMock.Object,
            _notifierMock.Object,
            NullLogger<PaymentRejectedEmailHandler>.Instance
        );
    }

    [Fact]
    public async Task Handle_WithResolvedOrder_ShouldSendTheRejectionEmailQuotingTheNotes()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.CreateSubmitted();
        _orderRepositoryMock.SetupGetByIdWithItems(order);
        const string notes = "Proof is not legible.";

        // Act
        await _handler.Handle(new PaymentRejectedEvent(order.Id, Guid.NewGuid(), notes), CancellationToken.None);

        // Assert
        _notifierMock.Verify(
            x => x.NotifyPaymentRejectedAsync(order, notes, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithNullNotes_ShouldStillSendTheRejectionEmail()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.CreateSubmitted();
        _orderRepositoryMock.SetupGetByIdWithItems(order);

        // Act
        await _handler.Handle(new PaymentRejectedEvent(order.Id, Guid.NewGuid(), null), CancellationToken.None);

        // Assert
        _notifierMock.Verify(x => x.NotifyPaymentRejectedAsync(order, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenOrderNotFound_ShouldSkipTheEmail()
    {
        // Arrange — repository returns null by default

        // Act
        await _handler.Handle(
            new PaymentRejectedEvent(Guid.NewGuid(), Guid.NewGuid(), "notes"),
            CancellationToken.None
        );

        // Assert
        _notifierMock.VerifyNoOtherCalls();
    }
}
