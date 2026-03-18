using _116.Content.Application.Commerce.UseCases.Admin.Queries.GetAllOrders;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Pagination;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Queries.GetAllOrders;

/// <summary>
/// Unit tests for <see cref="AdminGetAllOrdersHandler"/>.
/// </summary>
public class AdminGetAllOrdersHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IContentOrderRepository> _orderRepositoryMock;
    private readonly AdminGetAllOrdersHandler _handler;

    public AdminGetAllOrdersHandlerTests()
    {
        _orderRepositoryMock = MockContentOrderRepository.Create();
        _handler = new AdminGetAllOrdersHandler(_orderRepositoryMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_ShouldReturnPaginatedResult()
    {
        // Arrange
        List<ContentOrderEntity> orders = ContentOrderFactory.CreateMany(3);
        _orderRepositoryMock.SetupGetAllAsync(orders, orders.Count);

        var query = new AdminGetAllOrdersQuery(
            PaginatedRequest: new PaginatedRequest(0, 10),
            Status: null,
            CustomerId: null
        );

        // Act
        AdminGetAllOrdersResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Orders.Should().NotBeNull();
    }

    #endregion
}
