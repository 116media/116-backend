using _116.Content.Application.Commerce.UseCases.Admin.Commands.VerifyPayment.V1;
using _116.Tests.Fixtures.Constants;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Commands.VerifyPayment.V1;

/// <summary>
/// Unit tests for <see cref="AdminVerifyPaymentRequest"/> and <see cref="AdminVerifyPaymentResponse"/>.
/// </summary>
public class AdminVerifyPaymentEndpointV1Tests
{
    [Fact]
    public void AdminVerifyPaymentResponse_ShouldConstructCorrectly()
    {
        // Act
        var response = new AdminVerifyPaymentResponse(IsSuccess: true);

        // Assert
        response.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void AdminVerifyPaymentRequest_ShouldConstructCorrectly()
    {
        // Act
        var request = new AdminVerifyPaymentRequest(ReceiptUrl: TestConstants.Content.Commerce.ValidReceiptUrl);

        // Assert
        request.ReceiptUrl.Should().Be(TestConstants.Content.Commerce.ValidReceiptUrl);
    }
}
