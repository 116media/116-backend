using _116.BuildingBlocks.Application.Pagination;
using _116.Content.Application.Commerce.UseCases.Admin.Queries.GetAllOrders;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.TestData;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Commerce.UseCases.Admin.Queries.GetAllOrders;

/// <summary>
/// Unit tests for <see cref="AdminGetAllOrdersHandler"/>.
/// </summary>
public class AdminGetAllOrdersHandlerTests : BaseContentHandlerTest
{
    private readonly CustomerEntity _customer = CustomerFactory.Create();
    private readonly Mock<IContentOrderRepository> _orderRepositoryMock;
    private readonly AdminGetAllOrdersHandler _handler;

    public AdminGetAllOrdersHandlerTests()
    {
        _orderRepositoryMock = MockContentOrderRepository.Create();
        _handler = new AdminGetAllOrdersHandler(_orderRepositoryMock.Object, CreateOrderDtoService(_customer));
    }

    #region Success Cases

    [Fact]
    public async Task Handle_ShouldReturnPaginatedResult()
    {
        // Arrange
        List<ContentOrderEntity> orders = Enumerable
            .Range(0, 3)
            .Select(_ => ContentOrderFactory.CreateForCustomer(_customer.Id))
            .ToList();

        _orderRepositoryMock.SetupGetAllAsync(orders, orders.Count);

        var query = new AdminGetAllOrdersQuery(
            PaginatedRequest: new PaginatedRequest(0, 10),
            Status: null,
            CustomerId: null,
            Search: null
        );

        // Act
        AdminGetAllOrdersResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Orders.Items.Should().HaveCount(orders.Count);
        result.Orders.Count.Should().Be(orders.Count);
    }

    #endregion
}
