using _116.Content.Application.Commerce.UseCases.Admin.Commands.RemoveItemTier;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Builders.Entities.Content;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Commands.RemoveItemTier;

/// <summary>
/// Unit tests for <see cref="AdminRemoveItemTierHandler"/>.
/// </summary>
public class AdminRemoveItemTierHandlerTests
{
    private readonly Mock<IContentOrderRepository> _orderRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminRemoveItemTierHandler _handler;

    public AdminRemoveItemTierHandlerTests()
    {
        _orderRepositoryMock = MockContentOrderRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminRemoveItemTierHandler(
            _orderRepositoryMock.Object,
            _unitOfWorkMock.Object,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenDraftOrderAndTierExists_ShouldRemoveAndReturnSuccess()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.Create();
        _orderRepositoryMock.SetupGetByIdOrThrow(order);

        ContentOrderItemEntity item = ContentOrderItemFactory.Create(order.Id, Guid.NewGuid());
        _orderRepositoryMock.SetupGetItemByIdOrThrow(item);

        ContentItemTierEntity tier = ContentItemTierFactory.CreateDefault(item.Id, Guid.NewGuid());
        _orderRepositoryMock.SetupGetItemTierByIdOrThrow(tier);

        CustomerEntity customer = CustomerFactory.Create();
        ContentOrderEntity orderWithItems = new ContentOrderBuilder().WithCustomer(customer).Build();

        _orderRepositoryMock
            .Setup(x => x.GetByIdWithItemsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderWithItems);

        var command = new AdminRemoveItemTierCommand(
            OrderId: order.Id.ToString(),
            ItemId: item.Id.ToString(),
            TierId: tier.Id.ToString()
        );

        // Act
        AdminRemoveItemTierResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldCallRemoveItemTierAsync()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.Create();
        _orderRepositoryMock.SetupGetByIdOrThrow(order);

        ContentOrderItemEntity item = ContentOrderItemFactory.Create(order.Id, Guid.NewGuid());
        _orderRepositoryMock.SetupGetItemByIdOrThrow(item);

        ContentItemTierEntity tier = ContentItemTierFactory.CreateDefault(item.Id, Guid.NewGuid());
        _orderRepositoryMock.SetupGetItemTierByIdOrThrow(tier);

        CustomerEntity customer = CustomerFactory.Create();
        ContentOrderEntity orderWithItems = new ContentOrderBuilder().WithCustomer(customer).Build();

        _orderRepositoryMock
            .Setup(x => x.GetByIdWithItemsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderWithItems);

        var command = new AdminRemoveItemTierCommand(
            OrderId: order.Id.ToString(),
            ItemId: item.Id.ToString(),
            TierId: tier.Id.ToString()
        );

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _orderRepositoryMock.VerifyRemoveItemTierCalled();
    }

    [Fact]
    public async Task Handle_ShouldCommitTwice()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.Create();
        _orderRepositoryMock.SetupGetByIdOrThrow(order);

        ContentOrderItemEntity item = ContentOrderItemFactory.Create(order.Id, Guid.NewGuid());
        _orderRepositoryMock.SetupGetItemByIdOrThrow(item);

        ContentItemTierEntity tier = ContentItemTierFactory.CreateDefault(item.Id, Guid.NewGuid());
        _orderRepositoryMock.SetupGetItemTierByIdOrThrow(tier);

        CustomerEntity customer = CustomerFactory.Create();
        ContentOrderEntity orderWithItems = new ContentOrderBuilder().WithCustomer(customer).Build();

        _orderRepositoryMock
            .Setup(x => x.GetByIdWithItemsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderWithItems);

        var command = new AdminRemoveItemTierCommand(
            OrderId: order.Id.ToString(),
            ItemId: item.Id.ToString(),
            TierId: tier.Id.ToString()
        );

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _unitOfWorkMock.VerifyCommitCalled(times: 2);
    }

    [Fact]
    public async Task Handle_ShouldCallUpdateAsyncAfterRecalculation()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.Create();
        _orderRepositoryMock.SetupGetByIdOrThrow(order);

        ContentOrderItemEntity item = ContentOrderItemFactory.Create(order.Id, Guid.NewGuid());
        _orderRepositoryMock.SetupGetItemByIdOrThrow(item);

        ContentItemTierEntity tier = ContentItemTierFactory.CreateDefault(item.Id, Guid.NewGuid());
        _orderRepositoryMock.SetupGetItemTierByIdOrThrow(tier);

        CustomerEntity customer = CustomerFactory.Create();
        ContentOrderEntity orderWithItems = new ContentOrderBuilder().WithCustomer(customer).Build();

        _orderRepositoryMock
            .Setup(x => x.GetByIdWithItemsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderWithItems);

        var command = new AdminRemoveItemTierCommand(
            OrderId: order.Id.ToString(),
            ItemId: item.Id.ToString(),
            TierId: tier.Id.ToString()
        );

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _orderRepositoryMock.VerifyUpdateCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenOrderNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid orderId = Guid.NewGuid();
        _orderRepositoryMock.SetupGetByIdOrThrowNotFound(orderId);

        var command = new AdminRemoveItemTierCommand(
            OrderId: orderId.ToString(),
            ItemId: Guid.NewGuid().ToString(),
            TierId: Guid.NewGuid().ToString()
        );

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenSubmittedOrder_ShouldThrowBadRequestException()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.CreateSubmitted();
        _orderRepositoryMock.SetupGetByIdOrThrow(order);

        var command = new AdminRemoveItemTierCommand(
            OrderId: order.Id.ToString(),
            ItemId: Guid.NewGuid().ToString(),
            TierId: Guid.NewGuid().ToString()
        );

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WhenPaidOrder_ShouldThrowBadRequestException()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.CreatePaid();
        _orderRepositoryMock.SetupGetByIdOrThrow(order);

        var command = new AdminRemoveItemTierCommand(
            OrderId: order.Id.ToString(),
            ItemId: Guid.NewGuid().ToString(),
            TierId: Guid.NewGuid().ToString()
        );

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WhenCancelledOrder_ShouldThrowBadRequestException()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.CreateCancelled();
        _orderRepositoryMock.SetupGetByIdOrThrow(order);

        var command = new AdminRemoveItemTierCommand(
            OrderId: order.Id.ToString(),
            ItemId: Guid.NewGuid().ToString(),
            TierId: Guid.NewGuid().ToString()
        );

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WhenTierNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.Create();
        _orderRepositoryMock.SetupGetByIdOrThrow(order);

        ContentOrderItemEntity item = ContentOrderItemFactory.Create(order.Id, Guid.NewGuid());
        _orderRepositoryMock.SetupGetItemByIdOrThrow(item);

        var command = new AdminRemoveItemTierCommand(
            OrderId: order.Id.ToString(),
            ItemId: item.Id.ToString(),
            TierId: Guid.NewGuid().ToString()
        );

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenItemBelongsToAnotherOrder_ShouldThrowNotFoundExceptionWithoutRemoving()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.Create();
        _orderRepositoryMock.SetupGetByIdOrThrow(order);

        var command = new AdminRemoveItemTierCommand(
            OrderId: order.Id.ToString(),
            ItemId: Guid.NewGuid().ToString(),
            TierId: Guid.NewGuid().ToString()
        );

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _orderRepositoryMock.Verify(
            x => x.GetItemTierByIdOrThrowAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _orderRepositoryMock.Verify(
            x => x.RemoveItemTierAsync(It.IsAny<ContentItemTierEntity>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_WhenRefetchOrderFailsAfterRemoval_ShouldThrowNotFoundException()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.Create();
        _orderRepositoryMock.SetupGetByIdOrThrow(order);

        ContentOrderItemEntity item = ContentOrderItemFactory.Create(order.Id, Guid.NewGuid());
        _orderRepositoryMock.SetupGetItemByIdOrThrow(item);

        ContentItemTierEntity tier = ContentItemTierFactory.CreateDefault(item.Id, Guid.NewGuid());
        _orderRepositoryMock.SetupGetItemTierByIdOrThrow(tier);

        _orderRepositoryMock
            .Setup(x => x.GetByIdWithItemsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ContentOrderEntity?)null);

        var command = new AdminRemoveItemTierCommand(
            OrderId: order.Id.ToString(),
            ItemId: item.Id.ToString(),
            TierId: tier.Id.ToString()
        );

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
