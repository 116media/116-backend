using _116.BuildingBlocks.Application.Pagination;
using _116.Content.Application.Commerce.UseCases.Admin.Queries.GetPendingPaymentOrders;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.TestData;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Commerce.UseCases.Admin.Queries.GetPendingPaymentOrders;

/// <summary>
/// Unit tests for <see cref="AdminGetPendingPaymentOrdersHandler"/>.
/// </summary>
public class AdminGetPendingPaymentOrdersHandlerTests : BaseContentHandlerTest
{
    private readonly CustomerEntity _customer = CustomerFactory.Create();
    private readonly Mock<IContentOrderRepository> _orderRepositoryMock;
    private readonly AdminGetPendingPaymentOrdersHandler _handler;

    public AdminGetPendingPaymentOrdersHandlerTests()
    {
        _orderRepositoryMock = MockContentOrderRepository.Create();
        _handler = new AdminGetPendingPaymentOrdersHandler(
            _orderRepositoryMock.Object,
            CreateOrderDtoService(_customer)
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_ShouldReturnPaginatedPendingPaymentOrders()
    {
        // Arrange
        List<ContentOrderEntity> orders = Enumerable
            .Range(0, 2)
            .Select(_ => ContentOrderFactory.CreateForCustomer(_customer.Id))
            .ToList();

        _orderRepositoryMock.SetupGetAllAsync(orders, orders.Count);

        var query = new AdminGetPendingPaymentOrdersQuery(PaginatedRequest: new PaginatedRequest());

        // Act
        AdminGetPendingPaymentOrdersResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Orders.Items.Should().HaveCount(orders.Count);
        result.Orders.Count.Should().Be(orders.Count);
    }

    #endregion
}
