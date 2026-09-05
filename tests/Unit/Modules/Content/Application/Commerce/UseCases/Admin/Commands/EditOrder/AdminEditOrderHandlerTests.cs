using _116.Content.Application.Commerce.UseCases.Admin.Commands.EditOrder;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Domain.Exceptions;
using _116.Content.Domain.StateMachines;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Builders.Entities.Content;
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
            TestErrorsFactory.CreateContentI18n()
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenDraftOrder_ShouldReassignCustomerAndReturnSummary()
    {
        // Arrange
        CustomerEntity customer = CustomerFactory.Create();
        ContentOrderEntity order = new ContentOrderBuilder()
            .WithCustomer(customer)
            .WithPackageId(Guid.NewGuid())
            .Build();

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
        order.CustomerId.Should().Be(newCustomer.Id);
        order.PackageId.Should().BeNull();
        order.Status.Should().Be(EnumOrderStatus.Draft);
        result.Order.Id.Should().Be(order.Id);
        result.Order.Status.Should().Be(EnumOrderStatus.Draft);
        result.Order.TotalAmountUsd.Should().Be(order.TotalAmountUsd);
        _orderRepositoryMock.VerifyUpdateCalled(order);
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
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task Handle_WhenNotDraft_ShouldThrowBadRequestException()
    {
        // Arrange
        CustomerEntity customer = CustomerFactory.Create();
        ContentOrderEntity order = new ContentOrderBuilder().AsSubmitted().WithCustomer(customer).Build();
        order.ClearDomainEvents();

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
        (await act.Should().ThrowAsync<ContentRuleException>())
            .Which.Code.Should()
            .Be(ContentRuleCodes.CannotAddItemToNonDraftOrder);
        order.CustomerId.Should().Be(customer.Id);
        order.Status.Should().Be(EnumOrderStatus.PendingPayment);
        order.DomainEvents.Should().BeEmpty();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task Handle_WhenCustomerNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        CustomerEntity customer = CustomerFactory.Create();
        ContentOrderEntity order = new ContentOrderBuilder().WithCustomer(customer).Build();

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
        order.CustomerId.Should().Be(customer.Id);
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    #endregion
}
