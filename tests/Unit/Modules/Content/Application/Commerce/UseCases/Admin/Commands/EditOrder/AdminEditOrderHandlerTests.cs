using _116.Content.Application.Commerce.UseCases.Admin.Commands.EditOrder;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Commands.EditOrder;

/// <summary>
/// Unit tests for <see cref="AdminEditOrderHandler"/>.
/// </summary>
public class AdminEditOrderHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IContentOrderRepository> _orderRepositoryMock;
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminEditOrderHandler _handler;

    public AdminEditOrderHandlerTests()
    {
        _orderRepositoryMock = MockContentOrderRepository.Create();
        _customerRepositoryMock = MockCustomerRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminEditOrderHandler(
            _orderRepositoryMock.Object,
            _customerRepositoryMock.Object,
            _unitOfWorkMock.Object,
            Mapper,
            TestErrorsFactory.CreateContentOrderErrors()
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenDraftOrder_ShouldUpdateAndReturnResult()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.Create();
        CustomerEntity customer = CustomerFactory.Create();
        typeof(ContentOrderEntity).GetProperty(nameof(ContentOrderEntity.Customer))!.SetValue(order, customer);

        _orderRepositoryMock
            .Setup(x => x.GetByIdWithItemsAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        CustomerEntity newCustomer = CustomerFactory.Create();
        _customerRepositoryMock.SetupGetByIdOrThrow(newCustomer);

        var command = new AdminEditOrderCommand(
            OrderId: order.Id.ToString(),
            CustomerId: newCustomer.Id.ToString(),
            PackageId: null
        );

        // Act
        AdminEditOrderResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Order.Should().NotBeNull();
        _orderRepositoryMock.VerifyUpdateCalled();
        _unitOfWorkMock.VerifyCommitCalled();
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

        var command = new AdminEditOrderCommand(OrderId: orderId.ToString(), CustomerId: null, PackageId: null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenNotDraft_ShouldThrowBadRequestException()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.CreateSubmitted();
        CustomerEntity customer = CustomerFactory.Create();
        typeof(ContentOrderEntity).GetProperty(nameof(ContentOrderEntity.Customer))!.SetValue(order, customer);

        _orderRepositoryMock
            .Setup(x => x.GetByIdWithItemsAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var command = new AdminEditOrderCommand(
            OrderId: order.Id.ToString(),
            CustomerId: Guid.NewGuid().ToString(),
            PackageId: null
        );

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WhenCustomerNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.Create();
        CustomerEntity customer = CustomerFactory.Create();
        typeof(ContentOrderEntity).GetProperty(nameof(ContentOrderEntity.Customer))!.SetValue(order, customer);

        _orderRepositoryMock
            .Setup(x => x.GetByIdWithItemsAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        Guid unknownCustomerId = Guid.NewGuid();
        _customerRepositoryMock.SetupGetByIdOrThrowNotFound(unknownCustomerId);

        var command = new AdminEditOrderCommand(
            OrderId: order.Id.ToString(),
            CustomerId: unknownCustomerId.ToString(),
            PackageId: null
        );

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
