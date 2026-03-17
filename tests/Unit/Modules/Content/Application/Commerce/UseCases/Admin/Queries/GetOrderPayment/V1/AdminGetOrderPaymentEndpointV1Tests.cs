using _116.Content.Application.Commerce.UseCases.Admin.Queries.GetOrderPayment.V1;
using _116.Content.Application.Shared.DTOs;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Queries.GetOrderPayment.V1;

/// <summary>
/// Unit tests for <see cref="AdminGetOrderPaymentResponse"/>.
/// </summary>
public class AdminGetOrderPaymentEndpointV1Tests
{
    [Fact]
    public void AdminGetOrderPaymentResponse_ShouldConstructCorrectly()
    {
        // Arrange
        var payment = new PaymentDto(Guid.NewGuid(), 100m, null, null, "Pending", null, null, null);

        // Act
        var response = new AdminGetOrderPaymentResponse(Payment: payment);

        // Assert
        response.Payment.Should().NotBeNull();
        response.Payment.Should().Be(payment);
    }
}
