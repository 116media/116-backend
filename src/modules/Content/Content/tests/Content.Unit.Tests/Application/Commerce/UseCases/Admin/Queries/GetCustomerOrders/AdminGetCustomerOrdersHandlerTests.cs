using _116.BuildingBlocks.Application.Pagination;
using _116.Content.Application.Commerce.UseCases.Admin.Queries.GetCustomerOrders;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.TestData;
using _116.Content.TestData.Builders.Entities;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Factories.Helpers;
using _116.Content.TestData.Mocks.Factories;
using _116.Content.TestData.Mocks.Infrastructure;
using _116.Content.TestData.Mocks.Repositories;
using _116.Content.TestData.Mocks.Services;
using _116.Identity.TestData.Factories;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Identity.TestData.Mocks.Repositories;
using _116.Identity.TestData.Mocks.Services;
using _116.Storage.TestData.Factories;
using _116.Storage.TestData.Mocks.Infrastructure;
using _116.Storage.TestData.Mocks.Services;
using _116.Tests.TestData;
using _116.Tests.TestData.Mocks;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Commerce.UseCases.Admin.Queries.GetCustomerOrders;

/// <summary>
/// Unit tests for <see cref="AdminGetCustomerOrdersHandler"/>.
/// </summary>
public class AdminGetCustomerOrdersHandlerTests : BaseContentHandlerTest
{
    private readonly CustomerEntity _customer = CustomerFactory.Create();
    private readonly Mock<IContentOrderRepository> _orderRepositoryMock;
    private readonly AdminGetCustomerOrdersHandler _handler;

    public AdminGetCustomerOrdersHandlerTests()
    {
        _orderRepositoryMock = MockContentOrderRepository.Create();
        _handler = new AdminGetCustomerOrdersHandler(_orderRepositoryMock.Object, CreateOrderDtoFactory(_customer));
    }

    #region Success Cases

    [Fact]
    public async Task Handle_ShouldReturnPaginatedCustomerOrders()
    {
        // Arrange
        Guid customerId = Guid.NewGuid();
        List<ContentOrderEntity> orders = Enumerable
            .Range(0, 2)
            .Select(_ => new ContentOrderBuilder().WithCustomer(_customer).Build())
            .ToList();

        _orderRepositoryMock.SetupGetAllAsync(orders, orders.Count);

        var query = new AdminGetCustomerOrdersQuery(
            CustomerId: customerId,
            PaginatedRequest: new PaginatedRequest(0, 10)
        );

        // Act
        AdminGetCustomerOrdersResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Orders.Items.Should().HaveCount(orders.Count);
        result.Orders.Count.Should().Be(orders.Count);
    }

    #endregion
}
