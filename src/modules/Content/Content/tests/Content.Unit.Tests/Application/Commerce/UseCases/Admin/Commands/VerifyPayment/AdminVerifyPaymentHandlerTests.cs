using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Commerce.Services;
using _116.Content.Application.Commerce.UseCases.Admin.Commands.VerifyPayment;
using _116.Content.Application.Commerce.UseCases.Admin.Commands.VerifyPayment.Contracts;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Mocks.Repositories;
using _116.Content.TestData.Mocks.Services;
using _116.Tests.TestData.Constants;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Commerce.UseCases.Admin.Commands.VerifyPayment;

/// <summary>
/// Unit tests for <see cref="AdminVerifyPaymentHandler"/>.
/// </summary>
public class AdminVerifyPaymentHandlerTests
{
    private readonly Mock<IContentOrderRepository> _orderRepositoryMock;
    private readonly Mock<IOrderPaymentService> _orderPaymentServiceMock;
    private readonly Mock<IVerifyPaymentService> _verifyPaymentServiceMock;
    private readonly AdminVerifyPaymentHandler _handler;

    public AdminVerifyPaymentHandlerTests()
    {
        _orderRepositoryMock = MockContentOrderRepository.Create();
        _orderPaymentServiceMock = MockOrderPaymentService.Create();
        _verifyPaymentServiceMock = MockVerifyPaymentService.Create();
        _handler = new AdminVerifyPaymentHandler(
            _orderRepositoryMock.Object,
            _orderPaymentServiceMock.Object,
            _verifyPaymentServiceMock.Object,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenOrderAndPaymentFound_ShouldDelegateVerificationForArrangedOrderAndPayment()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.CreateSubmitted();
        ContentPaymentEntity payment = ContentPaymentFactory.Create(order.Id);
        Guid adminUserId = Guid.NewGuid();

        _orderRepositoryMock.SetupGetByIdWithItems(order);
        _orderPaymentServiceMock.SetupGetByOrderId(order.Id, payment);
        _verifyPaymentServiceMock.SetupVerifyAsync();

        var command = new AdminVerifyPaymentCommand(
            OrderId: order.Id.ToString(),
            ReceiptUrl: TestConstants.Commerce.ValidReceiptUrl,
            AdminUserId: adminUserId
        );

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _verifyPaymentServiceMock.Verify(
            x =>
                x.VerifyAsync(
                    order,
                    payment,
                    adminUserId,
                    TestConstants.Commerce.ValidReceiptUrl,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
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
            ReceiptUrl: TestConstants.Commerce.ValidReceiptUrl,
            AdminUserId: Guid.NewGuid()
        );

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _verifyPaymentServiceMock.Verify(
            x =>
                x.VerifyAsync(
                    It.IsAny<ContentOrderEntity>(),
                    It.IsAny<ContentPaymentEntity>(),
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_WhenPaymentNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.CreateSubmitted();
        _orderRepositoryMock.SetupGetByIdWithItems(order);
        _orderPaymentServiceMock.SetupGetByOrderIdNotFound(order.Id);

        var command = new AdminVerifyPaymentCommand(
            OrderId: order.Id.ToString(),
            ReceiptUrl: TestConstants.Commerce.ValidReceiptUrl,
            AdminUserId: Guid.NewGuid()
        );

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _verifyPaymentServiceMock.Verify(
            x =>
                x.VerifyAsync(
                    It.IsAny<ContentOrderEntity>(),
                    It.IsAny<ContentPaymentEntity>(),
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    #endregion
}
