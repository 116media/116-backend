using _116.Content.Application.Commerce.UseCases.Admin.Commands.RemoveItemTier;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Exceptions;
using _116.Content.Domain.StateMachines;
using _116.Shared.Application.Exceptions;
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
        ContentOrderItemEntity item = ContentOrderItemFactory.Create(order.Id, Guid.NewGuid());
        ContentItemTierEntity tier = ContentItemTierFactory.CreateDefault(item.Id, Guid.NewGuid());
        ContentItemTierEntity remainingTier = ContentItemTierFactory.Create(item.Id, Guid.NewGuid(), 75m);
        item.Tiers.Add(tier);
        item.Tiers.Add(remainingTier);
        order.AddItem(item);
        _orderRepositoryMock.SetupGetByIdOrThrow(order);

        var command = new AdminRemoveItemTierCommand(
            OrderId: order.Id.ToString(),
            ItemId: item.Id.ToString(),
            TierId: tier.Id.ToString()
        );

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        item.Tiers.Should().ContainSingle().Which.Should().Be(remainingTier);
        order.TotalAmountUsd.Should().Be(remainingTier.PriceSnapshotUsd);
        _unitOfWorkMock.VerifyCommitCalled();
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
        ContentOrderItemEntity item = ContentOrderItemFactory.Create(order.Id, Guid.NewGuid());
        order.AddItem(item);
        _orderRepositoryMock.SetupGetByIdOrThrow(order);

        Guid missingTierId = Guid.NewGuid();

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

        var command = new AdminRemoveItemTierCommand(
            OrderId: order.Id.ToString(),
            ItemId: foreignItemId.ToString(),
            TierId: Guid.NewGuid().ToString()
        );

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    #endregion
}
