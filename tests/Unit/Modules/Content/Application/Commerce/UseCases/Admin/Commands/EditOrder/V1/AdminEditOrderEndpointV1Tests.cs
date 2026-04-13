using _116.Content.Application.Commerce.UseCases.Admin.Commands.EditOrder.V1;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Pagination;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Commands.EditOrder.V1;

/// <summary>
/// Unit tests for <see cref="AdminEditOrderRequest"/> and <see cref="AdminEditOrderResponse"/>.
/// </summary>
public class AdminEditOrderEndpointV1Tests
{
    [Fact]
    public void AdminEditOrderRequest_ShouldConstructCorrectly()
    {
        // Arrange
        string customerId = Guid.NewGuid().ToString();
        Guid packageId = Guid.NewGuid();

        // Act
        var request = new AdminEditOrderRequest(CustomerId: customerId, PackageId: packageId);

        // Assert
        request.CustomerId.Should().Be(customerId);
        request.PackageId.Should().Be(packageId);
    }

    [Fact]
    public void AdminEditOrderRequest_WithNulls_ShouldConstructCorrectly()
    {
        // Arrange & Act
        var request = new AdminEditOrderRequest(CustomerId: null, PackageId: null);

        // Assert
        request.CustomerId.Should().BeNull();
        request.PackageId.Should().BeNull();
    }

    [Fact]
    public void AdminEditOrderResponse_ShouldConstructCorrectly()
    {
        // Arrange
        var order = new ContentOrderSummaryDto(
            Id: Guid.NewGuid(),
            CustomerName: "Test Customer",
            Status: EnumOrderStatus.Draft,
            TotalAmountUsd: 100m,
            ItemCount: 2
        );

        // Act
        var response = new AdminEditOrderResponse(Order: order);

        // Assert
        response.Order.Should().NotBeNull();
        response.Order.Should().Be(order);
    }
}
