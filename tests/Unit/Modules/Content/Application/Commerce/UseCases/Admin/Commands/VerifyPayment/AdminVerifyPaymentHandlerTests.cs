using _116.Content.Application.Commerce.Factories;
using _116.Content.Application.Commerce.UseCases.Admin.Commands.VerifyPayment;
using _116.Content.Application.Commerce.UseCases.Admin.Commands.VerifyPayment.Contracts;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common.Mocks.Factories;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Commands.VerifyPayment;

/// <summary>
/// Unit tests for <see cref="AdminVerifyPaymentHandler"/>.
/// </summary>
public class AdminVerifyPaymentHandlerTests
{
    private readonly Mock<IContentOrderRepository> _orderRepositoryMock;
    private readonly Mock<IOrderPaymentFactory> _orderPaymentFactoryMock;
    private readonly Mock<IVerifyPaymentFactory> _verifyPaymentFactoryMock;
    private readonly AdminVerifyPaymentHandler _handler;

    public AdminVerifyPaymentHandlerTests()
    {
        _orderRepositoryMock = MockContentOrderRepository.Create();
        _orderPaymentFactoryMock = MockOrderPaymentFactory.Create();
        _verifyPaymentFactoryMock = MockVerifyPaymentFactory.Create();
        _handler = new AdminVerifyPaymentHandler(
            _orderRepositoryMock.Object,
            _orderPaymentFactoryMock.Object,
            _verifyPaymentFactoryMock.Object
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenOrderAndPaymentFound_ShouldVerifyAndReturnSuccess()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.CreateSubmitted();
        ContentPaymentEntity payment = ContentPaymentFactory.Create(order.Id);

        _orderRepositoryMock.SetupGetByIdWithItems(order);
        _orderPaymentFactoryMock.SetupGetByOrderId(order.Id, payment);
        _verifyPaymentFactoryMock.SetupVerifyAsync();

        var command = new AdminVerifyPaymentCommand(
            OrderId: order.Id.ToString(),
            ReceiptUrl: TestConstants.Content.Commerce.ValidReceiptUrl,
            AdminUserId: Guid.NewGuid()
        );

        // Act
        AdminVerifyPaymentResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _verifyPaymentFactoryMock.VerifyVerifyCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenOrderNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        _orderRepositoryMock.SetupGetByIdWithItems(null);

        var command = new AdminVerifyPaymentCommand(
            OrderId: Guid.NewGuid().ToString(),
            ReceiptUrl: TestConstants.Content.Commerce.ValidReceiptUrl,
            AdminUserId: Guid.NewGuid()
        );

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenPaymentNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.CreateSubmitted();
        _orderRepositoryMock.SetupGetByIdWithItems(order);
        _orderPaymentFactoryMock.SetupGetByOrderIdNotFound(order.Id);

        var command = new AdminVerifyPaymentCommand(
            OrderId: order.Id.ToString(),
            ReceiptUrl: TestConstants.Content.Commerce.ValidReceiptUrl,
            AdminUserId: Guid.NewGuid()
        );

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
