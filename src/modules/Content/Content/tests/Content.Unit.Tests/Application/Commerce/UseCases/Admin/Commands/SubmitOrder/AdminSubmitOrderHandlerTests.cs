using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Commerce.UseCases.Admin.Commands.SubmitOrder;
using _116.Content.Application.Commerce.UseCases.Admin.Commands.SubmitOrder.Contracts;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Mocks.Repositories;
using _116.Content.TestData.Mocks.Services;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Commerce.UseCases.Admin.Commands.SubmitOrder;

/// <summary>
/// Unit tests for <see cref="AdminSubmitOrderHandler"/>.
/// </summary>
public class AdminSubmitOrderHandlerTests
{
    private readonly Mock<IContentOrderRepository> _orderRepositoryMock;
    private readonly Mock<ISubmitOrderService> _serviceMock;
    private readonly AdminSubmitOrderHandler _handler;

    public AdminSubmitOrderHandlerTests()
    {
        _orderRepositoryMock = MockContentOrderRepository.Create();
        _serviceMock = MockSubmitOrderService.Create();
        _handler = new AdminSubmitOrderHandler(
            _orderRepositoryMock.Object,
            _serviceMock.Object,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenOrderFound_ShouldCallSubmitAndReturnSuccess()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.Create();
        _orderRepositoryMock.SetupGetByIdWithItems(order);
        _serviceMock.SetupSubmitAsync();

        var command = new AdminSubmitOrderCommand(OrderId: order.Id.ToString());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _serviceMock.VerifySubmitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenOrderNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        _orderRepositoryMock.SetupGetByIdWithItems(null);

        var command = new AdminSubmitOrderCommand(OrderId: Guid.NewGuid().ToString());

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
