using _116.Content.Application.Commerce.UseCases.Admin.Commands.CancelOrder;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Commands.CancelOrder;

/// <summary>
/// Unit tests for <see cref="AdminCancelOrderHandler"/>.
/// </summary>
public class AdminCancelOrderHandlerTests
{
    private readonly Mock<IContentOrderRepository> _orderRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminCancelOrderHandler _handler;

    public AdminCancelOrderHandlerTests()
    {
        _orderRepositoryMock = MockContentOrderRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminCancelOrderHandler(
            _orderRepositoryMock.Object,
            _unitOfWorkMock.Object,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenOrderIsDraft_ShouldTransitionToCancelled()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.Create();
        _orderRepositoryMock.SetupGetByIdOrThrow(order);

        var command = new AdminCancelOrderCommand(OrderId: order.Id.ToString());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        order.Status.Should().Be(EnumOrderStatus.Cancelled);
        _orderRepositoryMock.VerifyUpdateCalled(order);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenOrderIsPendingPayment_ShouldTransitionToCancelled()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.CreateSubmitted();
        _orderRepositoryMock.SetupGetByIdOrThrow(order);

        var command = new AdminCancelOrderCommand(OrderId: order.Id.ToString());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        order.Status.Should().Be(EnumOrderStatus.Cancelled);
        _orderRepositoryMock.VerifyUpdateCalled(order);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenOrderIsDraft_ShouldRaiseOrderCancelledEvent()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.Create();
        order.ClearDomainEvents();
        _orderRepositoryMock.SetupGetByIdOrThrow(order);

        var command = new AdminCancelOrderCommand(OrderId: order.Id.ToString());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        order
            .DomainEvents.OfType<OrderCancelledEvent>()
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(new OrderCancelledEvent(OrderId: order.Id));
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenOrderNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid orderId = Guid.NewGuid();
        _orderRepositoryMock.SetupGetByIdOrThrowNotFound(orderId);

        var command = new AdminCancelOrderCommand(OrderId: orderId.ToString());

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task Handle_WhenOrderIsPaid_ShouldThrowBadRequestException()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.CreatePaid();
        order.ClearDomainEvents();
        _orderRepositoryMock.SetupGetByIdOrThrow(order);

        var command = new AdminCancelOrderCommand(OrderId: order.Id.ToString());

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
        order.Status.Should().Be(EnumOrderStatus.Paid);
        order.DomainEvents.Should().BeEmpty();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task Handle_WhenOrderAlreadyCancelled_ShouldThrowConflictException()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.CreateCancelled();
        order.ClearDomainEvents();
        _orderRepositoryMock.SetupGetByIdOrThrow(order);

        var command = new AdminCancelOrderCommand(OrderId: order.Id.ToString());

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
        order.Status.Should().Be(EnumOrderStatus.Cancelled);
        order.DomainEvents.Should().BeEmpty();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    #endregion
}
