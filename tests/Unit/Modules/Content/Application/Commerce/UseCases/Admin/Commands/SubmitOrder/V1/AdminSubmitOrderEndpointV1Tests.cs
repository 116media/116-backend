using _116.Content.Application.Commerce.UseCases.Admin.Commands.SubmitOrder.V1;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Commands.SubmitOrder.V1;

/// <summary>
/// Unit tests for <see cref="AdminSubmitOrderResponse"/>.
/// </summary>
public class AdminSubmitOrderEndpointV1Tests
{
    [Fact]
    public void AdminSubmitOrderResponse_ShouldConstructCorrectly()
    {
        // Act
        var response = new AdminSubmitOrderResponse(IsSuccess: true);

        // Assert
        response.IsSuccess.Should().BeTrue();
    }
}
