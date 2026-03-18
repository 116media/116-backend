using _116.Content.Application.Commerce.UseCases.Admin.Queries.GetCustomerOrders.V1;
using _116.Content.Application.Shared.DTOs;
using _116.Shared.Application.Pagination;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Queries.GetCustomerOrders.V1;

/// <summary>
/// Unit tests for <see cref="AdminGetCustomerOrdersResponse"/>.
/// </summary>
public class AdminGetCustomerOrdersEndpointV1Tests
{
    [Fact]
    public void AdminGetCustomerOrdersResponse_ShouldConstructCorrectly()
    {
        // Arrange
        var paginatedResult = new PaginatedResult<ContentOrderSummaryDto>(0, 10, 0, []);

        // Act
        var response = new AdminGetCustomerOrdersResponse(Orders: paginatedResult);

        // Assert
        response.Orders.Should().NotBeNull();
        response.Orders.Should().Be(paginatedResult);
    }
}
