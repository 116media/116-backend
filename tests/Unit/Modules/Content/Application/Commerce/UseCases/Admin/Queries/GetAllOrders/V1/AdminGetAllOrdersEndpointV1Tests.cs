using _116.Content.Application.Commerce.UseCases.Admin.Queries.GetAllOrders.V1;
using _116.Content.Application.Shared.DTOs;
using _116.Shared.Application.Pagination;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Queries.GetAllOrders.V1;

/// <summary>
/// Unit tests for <see cref="AdminGetAllOrdersResponse"/>.
/// </summary>
public class AdminGetAllOrdersEndpointV1Tests
{
    [Fact]
    public void AdminGetAllOrdersResponse_ShouldConstructCorrectly()
    {
        // Arrange
        var paginatedResult = new PaginatedResult<ContentOrderSummaryDto>(0, 10, 0, []);

        // Act
        var response = new AdminGetAllOrdersResponse(Orders: paginatedResult);

        // Assert
        response.Orders.Should().NotBeNull();
        response.Orders.Should().Be(paginatedResult);
    }
}
