using _116.Content.Application.Commerce.UseCases.Admin.Commands.RemoveItemTier;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Exceptions;
using _116.Content.Domain.StateMachines;
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
    public async Task Handle_WhenDraftOrderAndTierExists_ShouldRemoveTierAndRecalculateTotal()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.Create();
        _orderRepositoryMock.SetupGetByIdOrThrow(order);

        ContentOrderItemEntity item = ContentOrderItemFactory.Create(order.Id, Guid.NewGuid());
        _orderRepositoryMock.SetupGetItemByIdOrThrow(item);

        ContentItemTierEntity tier = ContentItemTierFactory.CreateDefault(item.Id, Guid.NewGuid());
        _orderRepositoryMock.SetupGetItemTierByIdOrThrow(tier);

        CustomerEntity customer = CustomerFactory.Create();
        ContentOrderEntity orderWithItems = new ContentOrderBuilder().WithId(order.Id).WithCustomer(customer).Build();
        ContentOrderItemEntity remainingItem = ContentOrderItemFactory.Create(order.Id, Guid.NewGuid());
        ContentItemTierEntity remainingTier = ContentItemTierFactory.Create(remainingItem.Id, Guid.NewGuid(), 75m);
        remainingItem.Tiers.Add(remainingTier);
        orderWithItems.Items.Add(remainingItem);

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
        orderWithItems.TotalAmountUsd.Should().Be(remainingTier.PriceSnapshotUsd);
        _orderRepositoryMock.Verify(x => x.RemoveItemTierAsync(tier, It.IsAny<CancellationToken>()), Times.Once);
        _orderRepositoryMock.VerifyUpdateCalled(orderWithItems);
        _unitOfWorkMock.VerifyCommitCalled(times: 2);
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
        _unitOfWorkMock.VerifyCommitNotCalled();
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
        (await act.Should().ThrowAsync<ContentRuleException>())
            .Which.Code.Should()
            .Be(ContentRuleCodes.CannotAddItemToNonDraftOrder);
        _unitOfWorkMock.VerifyCommitNotCalled();
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
        (await act.Should().ThrowAsync<ContentRuleException>())
            .Which.Code.Should()
            .Be(ContentRuleCodes.CannotAddItemToNonDraftOrder);
        _unitOfWorkMock.VerifyCommitNotCalled();
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
        (await act.Should().ThrowAsync<ContentRuleException>())
            .Which.Code.Should()
            .Be(ContentRuleCodes.CannotAddItemToNonDraftOrder);
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task Handle_WhenTierNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.Create();
        _orderRepositoryMock.SetupGetByIdOrThrow(order);

        ContentOrderItemEntity item = ContentOrderItemFactory.Create(order.Id, Guid.NewGuid());
        _orderRepositoryMock.SetupGetItemByIdOrThrow(item);

        Guid missingTierId = Guid.NewGuid();
        _orderRepositoryMock.SetupGetItemTierByIdOrThrowNotFound(item.Id, missingTierId);

        var command = new AdminRemoveItemTierCommand(
            OrderId: order.Id.ToString(),
            ItemId: item.Id.ToString(),
            TierId: missingTierId.ToString()
        );

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task Handle_WhenItemBelongsToAnotherOrder_ShouldThrowNotFoundExceptionWithoutRemoving()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.Create();
        _orderRepositoryMock.SetupGetByIdOrThrow(order);

        Guid foreignItemId = Guid.NewGuid();
        _orderRepositoryMock.SetupGetItemByIdOrThrowNotFound(order.Id, foreignItemId);

        var command = new AdminRemoveItemTierCommand(
            OrderId: order.Id.ToString(),
            ItemId: foreignItemId.ToString(),
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
        _unitOfWorkMock.VerifyCommitNotCalled();
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
        _orderRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<ContentOrderEntity>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _unitOfWorkMock.VerifyCommitCalled(times: 1);
    }

    #endregion
}
