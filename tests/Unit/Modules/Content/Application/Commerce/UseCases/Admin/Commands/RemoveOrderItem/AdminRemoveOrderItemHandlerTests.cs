using _116.Content.Application.Commerce.UseCases.Admin.Commands.RemoveOrderItem;
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
    public async Task Handle_WhenDraftOrderAndItemExists_ShouldRemoveAndReturnSuccess()
    {
        // Arrange
        CustomerEntity customer = CustomerFactory.Create();
        ContentOrderEntity order = new ContentOrderBuilder().WithCustomer(customer).Build();

        ContentOrderItemEntity item = ContentOrderItemFactory.Create(order.Id, Guid.NewGuid());
        order.Items.Add(item);

        _orderRepositoryMock
            .Setup(x => x.GetByIdWithItemsAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        _orderRepositoryMock.SetupGetItemByIdOrThrow(item);

        var command = new AdminRemoveOrderItemCommand(OrderId: order.Id.ToString(), ItemId: item.Id.ToString());

        // Act
        AdminRemoveOrderItemResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _orderRepositoryMock.VerifyRemoveItemCalled();
        _orderRepositoryMock.VerifyUpdateCalled();
        _unitOfWorkMock.VerifyCommitCalled(times: 2);
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
    }

    [Fact]
    public async Task Handle_WhenItemNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        CustomerEntity customer = CustomerFactory.Create();
        ContentOrderEntity order = new ContentOrderBuilder().WithCustomer(customer).Build();

        _orderRepositoryMock
            .Setup(x => x.GetByIdWithItemsAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var command = new AdminRemoveOrderItemCommand(OrderId: order.Id.ToString(), ItemId: Guid.NewGuid().ToString());

        // Act — default mock throws NotFoundException for GetItemByIdOrThrowAsync
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
