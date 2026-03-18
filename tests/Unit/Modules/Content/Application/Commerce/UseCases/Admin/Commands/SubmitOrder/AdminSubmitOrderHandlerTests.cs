using _116.Content.Application.Commerce.UseCases.Admin.Commands.SubmitOrder;
using _116.Content.Application.Commerce.UseCases.Admin.Commands.SubmitOrder.Contracts;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common.Mocks.Factories;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Commands.SubmitOrder;

/// <summary>
/// Unit tests for <see cref="AdminSubmitOrderHandler"/>.
/// </summary>
public class AdminSubmitOrderHandlerTests
{
    private readonly Mock<IContentOrderRepository> _orderRepositoryMock;
    private readonly Mock<ISubmitOrderFactory> _factoryMock;
    private readonly AdminSubmitOrderHandler _handler;

    public AdminSubmitOrderHandlerTests()
    {
        _orderRepositoryMock = MockContentOrderRepository.Create();
        _factoryMock = MockSubmitOrderFactory.Create();
        _handler = new AdminSubmitOrderHandler(_orderRepositoryMock.Object, _factoryMock.Object);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenOrderFound_ShouldCallSubmitAndReturnSuccess()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.Create();
        _orderRepositoryMock.SetupGetByIdWithItems(order);
        _factoryMock.SetupSubmitAsync();

        var command = new AdminSubmitOrderCommand(OrderId: order.Id.ToString());

        // Act
        AdminSubmitOrderResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _factoryMock.VerifySubmitCalled();
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
