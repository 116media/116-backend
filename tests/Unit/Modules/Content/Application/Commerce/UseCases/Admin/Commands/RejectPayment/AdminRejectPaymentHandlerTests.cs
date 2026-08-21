using _116.Content.Application.Commerce.Factories;
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
using _116.Unit.Tests.Common.Mocks.Factories;
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
    private readonly Mock<IOrderPaymentFactory> _orderPaymentFactoryMock;
    private readonly Mock<IContentOrderRepository> _orderRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminRejectPaymentHandler _handler;

    public AdminRejectPaymentHandlerTests()
    {
        _orderPaymentFactoryMock = MockOrderPaymentFactory.Create();
        _orderRepositoryMock = MockContentOrderRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminRejectPaymentHandler(
            _orderPaymentFactoryMock.Object,
            _orderRepositoryMock.Object,
            _unitOfWorkMock.Object,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenPaymentFound_ShouldTransitionToRejectedWithNotes()
    {
        // Arrange
        Guid orderId = Guid.NewGuid();
        ContentPaymentEntity payment = ContentPaymentFactory.Create(orderId);
        _orderPaymentFactoryMock.SetupGetByOrderId(orderId, payment);

        var command = new AdminRejectPaymentCommand(
            OrderId: orderId.ToString(),
            Notes: TestConstants.Commerce.ValidRejectionNotes
        );

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        payment.Status.Should().Be(EnumPaymentStatus.Rejected);
        payment.Notes.Should().Be(TestConstants.Commerce.ValidRejectionNotes);
        _orderRepositoryMock.Verify(x => x.UpdatePaymentAsync(payment, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenPaymentFound_ShouldRaisePaymentRejectedEvent()
    {
        // Arrange
        Guid orderId = Guid.NewGuid();
        ContentPaymentEntity payment = ContentPaymentFactory.Create(orderId);
        payment.ClearDomainEvents();
        _orderPaymentFactoryMock.SetupGetByOrderId(orderId, payment);

        var command = new AdminRejectPaymentCommand(
            OrderId: orderId.ToString(),
            Notes: TestConstants.Commerce.ValidRejectionNotes
        );

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        payment
            .DomainEvents.OfType<PaymentRejectedEvent>()
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(
                new PaymentRejectedEvent(
                    OrderId: orderId,
                    PaymentId: payment.Id,
                    Notes: TestConstants.Commerce.ValidRejectionNotes
                )
            );
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenPaymentNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid orderId = Guid.NewGuid();
        _orderPaymentFactoryMock.SetupGetByOrderIdNotFound(orderId);

        var command = new AdminRejectPaymentCommand(OrderId: orderId.ToString(), Notes: null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task Handle_WhenOrderNotFound_ShouldThrowNotFoundExceptionWithoutResolvingPayment()
    {
        // Arrange
        Guid orderId = Guid.NewGuid();
        _orderRepositoryMock.SetupGetByIdOrThrowNotFound(orderId);

        var command = new AdminRejectPaymentCommand(OrderId: orderId.ToString(), Notes: null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _orderPaymentFactoryMock.Verify(
            x => x.GetByOrderIdOrThrowAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task Handle_WhenPaymentAlreadyVerified_ShouldThrowConflictException()
    {
        // Arrange
        Guid orderId = Guid.NewGuid();
        ContentPaymentEntity payment = ContentPaymentFactory.CreateVerified(orderId);
        payment.ClearDomainEvents();
        _orderPaymentFactoryMock.SetupGetByOrderId(orderId, payment);

        var command = new AdminRejectPaymentCommand(
            OrderId: orderId.ToString(),
            Notes: TestConstants.Commerce.ValidRejectionNotes
        );

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        (await act.Should().ThrowAsync<ContentRuleException>())
            .Which.Code.Should()
            .Be(ContentRuleCodes.PaymentAlreadyVerified);
        payment.Status.Should().Be(EnumPaymentStatus.Verified);
        payment.Notes.Should().BeNull();
        payment.DomainEvents.Should().BeEmpty();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task Handle_WhenPaymentAlreadyRejected_ShouldThrowConflictException()
    {
        // Arrange
        Guid orderId = Guid.NewGuid();
        ContentPaymentEntity payment = ContentPaymentFactory.CreateRejected(orderId);
        payment.ClearDomainEvents();
        _orderPaymentFactoryMock.SetupGetByOrderId(orderId, payment);

        var command = new AdminRejectPaymentCommand(OrderId: orderId.ToString(), Notes: null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        (await act.Should().ThrowAsync<ContentRuleException>())
            .Which.Code.Should()
            .Be(ContentRuleCodes.PaymentAlreadyRejected);
        payment.Status.Should().Be(EnumPaymentStatus.Rejected);
        payment.Notes.Should().Be(TestConstants.Commerce.ValidRejectionNotes);
        payment.DomainEvents.Should().BeEmpty();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    #endregion
}
