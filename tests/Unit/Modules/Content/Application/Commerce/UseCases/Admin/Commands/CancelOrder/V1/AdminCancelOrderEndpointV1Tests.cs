using _116.Content.Application.Commerce.UseCases.Admin.Commands.CancelOrder.V1;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Commands.CancelOrder.V1;

/// <summary>
/// Unit tests for <see cref="AdminCancelOrderResponse"/>.
/// </summary>
public class AdminCancelOrderEndpointV1Tests
{
    [Fact]
    public void AdminCancelOrderResponse_ShouldConstructCorrectly()
    {
        // Act
        var response = new AdminCancelOrderResponse(IsSuccess: true);

        // Assert
        response.IsSuccess.Should().BeTrue();
    }
}
