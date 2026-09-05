using _116.Content.Application.Commerce.UseCases.Admin.Commands.RemoveOrderItem;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Builders.Entities.Content;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Commands.RemoveOrderItem;

/// <summary>
/// Unit tests for <see cref="AdminRemoveOrderItemHandler"/>.
/// </summary>
public class AdminRemoveOrderItemHandlerTests
{
    private readonly Mock<IContentOrderRepository> _orderRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminRemoveOrderItemHandler _handler;

    public AdminRemoveOrderItemHandlerTests()
    {
        _orderRepositoryMock = MockContentOrderRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminRemoveOrderItemHandler(
            _orderRepositoryMock.Object,
            _unitOfWorkMock.Object,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenDraftOrderAndItemExists_ShouldRemoveItemAndRecalculateTotal()
    {
        // Arrange
        CustomerEntity customer = CustomerFactory.Create();
        ContentOrderEntity order = new ContentOrderBuilder().WithCustomer(customer).Build();

        ContentOrderItemEntity item = ContentOrderItemFactory.Create(order.Id, Guid.NewGuid());
        ContentOrderItemEntity remainingItem = ContentOrderItemFactory.Create(order.Id, Guid.NewGuid());
        remainingItem.Tiers.Add(
            ContentItemTierFactory.Create(remainingItem.Id, Guid.NewGuid(), TestConstants.Commerce.ValidTierPriceUsd)
        );
        order.Items.Add(item);
        order.Items.Add(remainingItem);

        _orderRepositoryMock.SetupGetByIdWithItems(order);
        _orderRepositoryMock.SetupGetItemByIdOrThrow(item);

        var command = new AdminRemoveOrderItemCommand(OrderId: order.Id.ToString(), ItemId: item.Id.ToString());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert — the removal and the recalculated total commit together
        order.Items.Should().NotContain(item);
        order.TotalAmountUsd.Should().Be(TestConstants.Commerce.ValidTierPriceUsd);
        _orderRepositoryMock.Verify(x => x.RemoveItemAsync(item, It.IsAny<CancellationToken>()), Times.Once);
        _orderRepositoryMock.VerifyUpdateCalled(order);
        _unitOfWorkMock.VerifyCommitCalled(times: 1);
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenOrderNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid orderId = Guid.NewGuid();
        _orderRepositoryMock
            .Setup(x => x.GetByIdWithItemsAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ContentOrderEntity?)null);

        var command = new AdminRemoveOrderItemCommand(OrderId: orderId.ToString(), ItemId: Guid.NewGuid().ToString());

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task Handle_WhenNotDraft_ShouldThrowBadRequestException()
    {
        // Arrange
        CustomerEntity customer = CustomerFactory.Create();
        ContentOrderEntity order = new ContentOrderBuilder().AsSubmitted().WithCustomer(customer).Build();

        _orderRepositoryMock
            .Setup(x => x.GetByIdWithItemsAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var command = new AdminRemoveOrderItemCommand(OrderId: order.Id.ToString(), ItemId: Guid.NewGuid().ToString());

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
        _orderRepositoryMock.Verify(
            x => x.RemoveItemAsync(It.IsAny<ContentOrderItemEntity>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task Handle_WhenItemNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        CustomerEntity customer = CustomerFactory.Create();
        ContentOrderEntity order = new ContentOrderBuilder().WithCustomer(customer).Build();
        Guid missingItemId = Guid.NewGuid();

        _orderRepositoryMock
            .Setup(x => x.GetByIdWithItemsAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        _orderRepositoryMock.SetupGetItemByIdOrThrowNotFound(order.Id, missingItemId);

        var command = new AdminRemoveOrderItemCommand(OrderId: order.Id.ToString(), ItemId: missingItemId.ToString());

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    #endregion
}
