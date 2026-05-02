using _116.Content.Application.Commerce.UseCases.Admin.Queries.GetAllPayments;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Pagination;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Queries.GetAllPayments;

/// <summary>
/// Unit tests for <see cref="AdminGetAllPaymentsHandler"/>.
/// </summary>
public class AdminGetAllPaymentsHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IContentOrderRepository> _orderRepositoryMock;
    private readonly AdminGetAllPaymentsHandler _handler;

    public AdminGetAllPaymentsHandlerTests()
    {
        _orderRepositoryMock = MockContentOrderRepository.Create();
        _handler = new AdminGetAllPaymentsHandler(_orderRepositoryMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_ShouldReturnPaginatedResult()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        ContentOrderEntity order = ContentOrderFactory.CreateWithId(orderId);
        CustomerEntity customer = CustomerFactory.Create();
        typeof(ContentOrderEntity).GetProperty(nameof(ContentOrderEntity.Customer))!.SetValue(order, customer);

        ContentPaymentEntity payment = ContentPaymentFactory.Create(orderId);
        typeof(ContentPaymentEntity).GetProperty(nameof(ContentPaymentEntity.Order))!.SetValue(payment, order);

        List<ContentPaymentEntity> payments = [payment];
        _orderRepositoryMock.SetupGetAllPaymentsAsync(payments, payments.Count);

        var query = new AdminGetAllPaymentsQuery(
            PaginatedRequest: new PaginatedRequest(0, 10),
            Status: null,
            Method: null,
            Search: null
        );

        // Act
        AdminGetAllPaymentsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Payments.Should().NotBeNull();
        result.Payments.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_WithStatusFilter_ShouldPassToRepository()
    {
        // Arrange
        _orderRepositoryMock.SetupGetAllPaymentsAsync([], 0);

        var query = new AdminGetAllPaymentsQuery(
            PaginatedRequest: new PaginatedRequest(0, 10),
            Status: EnumPaymentStatus.Pending,
            Method: null,
            Search: null
        );

        // Act
        AdminGetAllPaymentsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Payments.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithMethodFilter_ShouldPassToRepository()
    {
        // Arrange
        _orderRepositoryMock.SetupGetAllPaymentsAsync([], 0);

        var query = new AdminGetAllPaymentsQuery(
            PaginatedRequest: new PaginatedRequest(0, 10),
            Status: null,
            Method: EnumPaymentMethod.BankTransfer,
            Search: null
        );

        // Act
        AdminGetAllPaymentsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Payments.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_EmptyResult_ShouldReturnEmptyPaginatedResult()
    {
        // Arrange
        _orderRepositoryMock.SetupGetAllPaymentsAsync([], 0);

        var query = new AdminGetAllPaymentsQuery(
            PaginatedRequest: new PaginatedRequest(0, 10),
            Status: null,
            Method: null,
            Search: null
        );

        // Act
        AdminGetAllPaymentsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Payments.Items.Should().BeEmpty();
        result.Payments.Count.Should().Be(0);
    }

    #endregion
}
