using _116.Content.Application.Commerce.UseCases.Admin.Queries.GetOrderById.V1;
using _116.Content.Application.Shared.DTOs;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Queries.GetOrderById.V1;

/// <summary>
/// Unit tests for <see cref="AdminGetOrderByIdResponse"/>.
/// </summary>
public class AdminGetOrderByIdEndpointV1Tests
{
    [Fact]
    public void AdminGetOrderByIdResponse_ShouldConstructCorrectly()
    {
        // Arrange
        var order = new ContentOrderDetailDto(Guid.NewGuid(), "Acme Corp", "Draft", 0m, [], null);

        // Act
        var response = new AdminGetOrderByIdResponse(Order: order);

        // Assert
        response.Order.Should().NotBeNull();
        response.Order.Should().Be(order);
    }
}
