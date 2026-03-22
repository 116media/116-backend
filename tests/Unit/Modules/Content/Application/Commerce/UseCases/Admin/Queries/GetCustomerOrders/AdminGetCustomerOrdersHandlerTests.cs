using _116.Content.Application.Commerce.UseCases.Admin.Queries.GetCustomerOrders;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Pagination;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Queries.GetCustomerOrders;

/// <summary>
/// Unit tests for <see cref="AdminGetCustomerOrdersHandler"/>.
/// </summary>
public class AdminGetCustomerOrdersHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IContentOrderRepository> _orderRepositoryMock;
    private readonly AdminGetCustomerOrdersHandler _handler;

    public AdminGetCustomerOrdersHandlerTests()
    {
        _orderRepositoryMock = MockContentOrderRepository.Create();
        _handler = new AdminGetCustomerOrdersHandler(_orderRepositoryMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_ShouldReturnPaginatedCustomerOrders()
    {
        // Arrange
        Guid customerId = Guid.NewGuid();
        List<ContentOrderEntity> orders = ContentOrderFactory.CreateMany(2);
        _orderRepositoryMock.SetupGetAllAsync(orders, orders.Count);

        var query = new AdminGetCustomerOrdersQuery(
            CustomerId: customerId,
            PaginatedRequest: new PaginatedRequest(0, 10)
        );

        // Act
        AdminGetCustomerOrdersResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Orders.Should().NotBeNull();
    }

    #endregion
}
