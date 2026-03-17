using _116.Content.Application.Commerce.UseCases.Admin.Commands.CreateOrder.V1;
using _116.Content.Application.Shared.DTOs;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Commands.CreateOrder.V1;

/// <summary>
/// Unit tests for <see cref="AdminCreateOrderRequest"/> and <see cref="AdminCreateOrderResponse"/>.
/// </summary>
public class AdminCreateOrderEndpointV1Tests
{
    [Fact]
    public void AdminCreateOrderResponse_ShouldConstructCorrectly()
    {
        // Arrange
        var order = new ContentOrderSummaryDto(Guid.NewGuid(), "Acme Corp", "Draft", 0m, null, 0);

        // Act
        var response = new AdminCreateOrderResponse(Order: order);

        // Assert
        response.Order.Should().NotBeNull();
        response.Order.Should().Be(order);
    }

    [Fact]
    public void AdminCreateOrderRequest_WithoutPackage_ShouldConstructCorrectly()
    {
        // Act
        var request = new AdminCreateOrderRequest(CustomerId: Guid.NewGuid().ToString(), PackageId: null);

        // Assert
        request.CustomerId.Should().NotBeNullOrEmpty();
        request.PackageId.Should().BeNull();
    }

    [Fact]
    public void AdminCreateOrderRequest_WithPackage_ShouldConstructCorrectly()
    {
        // Arrange
        Guid packageId = Guid.NewGuid();

        // Act
        var request = new AdminCreateOrderRequest(CustomerId: Guid.NewGuid().ToString(), PackageId: packageId);

        // Assert
        request.PackageId.Should().Be(packageId);
    }
}
