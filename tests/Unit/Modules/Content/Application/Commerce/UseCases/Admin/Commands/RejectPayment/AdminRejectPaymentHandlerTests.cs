using _116.Content.Application.Commerce.UseCases.Admin.Commands.RejectPayment;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;
using _116.Content.Domain.Exceptions;
using _116.Content.Domain.StateMachines;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Commands.RejectPayment;

/// <summary>
/// Unit tests for <see cref="AdminRejectPaymentHandler"/>.
/// </summary>
public class AdminRejectPaymentHandlerTests
{
    private readonly Mock<IContentOrderRepository> _orderRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminRejectPaymentHandler _handler;

    public AdminRejectPaymentHandlerTests()
    {
        _orderRepositoryMock = MockContentOrderRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminRejectPaymentHandler(
            _orderRepositoryMock.Object,
            _unitOfWorkMock.Object,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    /// <summary>
    /// Arranges an order carrying a payment, as the submission flow leaves it.
    /// </summary>
    private static (ContentOrderEntity Order, ContentPaymentEntity Payment) CreateOrderWithPayment()
    {
        ContentOrderEntity order = ContentOrderFactory.CreateWithId(Guid.NewGuid());
        ContentPaymentEntity payment = order.AttachPayment();

        return (order, payment);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenPaymentFound_ShouldTransitionToRejectedWithNotes()
    {
        // Arrange
        (ContentOrderEntity order, ContentPaymentEntity payment) = CreateOrderWithPayment();
        _orderRepositoryMock.SetupGetByIdOrThrow(order);

        var command = new AdminRejectPaymentCommand(
            OrderId: order.Id.ToString(),
            Notes: TestConstants.Commerce.ValidRejectionNotes
        );

        // Act
        AdminRejectPaymentResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        payment.Status.Should().Be(EnumPaymentStatus.Rejected);
        payment.Notes.Should().Be(TestConstants.Commerce.ValidRejectionNotes);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenPaymentFound_ShouldRaisePaymentRejectedEventOnTheOrder()
    {
        // Arrange
        (ContentOrderEntity order, ContentPaymentEntity payment) = CreateOrderWithPayment();
        order.ClearDomainEvents();
        _orderRepositoryMock.SetupGetByIdOrThrow(order);

        var command = new AdminRejectPaymentCommand(
            OrderId: order.Id.ToString(),
            Notes: TestConstants.Commerce.ValidRejectionNotes
        );

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        order
            .DomainEvents.OfType<PaymentRejectedEvent>()
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(
                new PaymentRejectedEvent(
                    OrderId: order.Id,
                    PaymentId: payment.Id,
                    Notes: TestConstants.Commerce.ValidRejectionNotes
                )
            );
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenOrderHasNoPayment_ShouldThrowNotFoundException()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.CreateWithId(Guid.NewGuid());
        _orderRepositoryMock.SetupGetByIdOrThrow(order);

        var command = new AdminRejectPaymentCommand(OrderId: order.Id.ToString(), Notes: null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task Handle_WhenOrderNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid missingOrderId = Guid.NewGuid();
        _orderRepositoryMock.SetupGetByIdOrThrowNotFound(missingOrderId);

        var command = new AdminRejectPaymentCommand(OrderId: missingOrderId.ToString(), Notes: null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task Handle_WhenPaymentAlreadyVerified_ShouldThrowAlreadyVerifiedRule()
    {
        // Arrange
        (ContentOrderEntity order, ContentPaymentEntity payment) = CreateOrderWithPayment();
        payment.AttachProof(proofFileId: Guid.NewGuid(), paymentMethod: EnumPaymentMethod.BankTransfer);
        payment.Verify(
            adminUserId: Guid.NewGuid(),
            receiptUrl: TestConstants.Commerce.ValidReceiptUrl,
            now: TestConstants.Clock.Instant
        );
        _orderRepositoryMock.SetupGetByIdOrThrow(order);

        var command = new AdminRejectPaymentCommand(OrderId: order.Id.ToString(), Notes: null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<ContentRuleException>()
            .Where(exception => exception.Code == ContentRuleCodes.PaymentAlreadyVerified);
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task Handle_WhenPaymentAlreadyRejected_ShouldThrowAlreadyRejectedRule()
    {
        // Arrange
        (ContentOrderEntity order, _) = CreateOrderWithPayment();
        order.RejectPayment(notes: "first rejection");
        _orderRepositoryMock.SetupGetByIdOrThrow(order);

        var command = new AdminRejectPaymentCommand(OrderId: order.Id.ToString(), Notes: null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<ContentRuleException>()
            .Where(exception => exception.Code == ContentRuleCodes.PaymentAlreadyRejected);
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    #endregion
}
