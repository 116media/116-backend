using _116.Content.Application.Commerce.UseCases.Admin.Commands.RejectPayment.V1;
using _116.Tests.Fixtures.Constants;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Commands.RejectPayment.V1;

/// <summary>
/// Unit tests for <see cref="AdminRejectPaymentRequest"/> and <see cref="AdminRejectPaymentResponse"/>.
/// </summary>
public class AdminRejectPaymentEndpointV1Tests
{
    [Fact]
    public void AdminRejectPaymentResponse_ShouldConstructCorrectly()
    {
        // Act
        var response = new AdminRejectPaymentResponse(IsSuccess: true);

        // Assert
        response.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void AdminRejectPaymentRequest_WithNotes_ShouldConstructCorrectly()
    {
        // Act
        var request = new AdminRejectPaymentRequest(Notes: TestConstants.Content.Commerce.ValidRejectionNotes);

        // Assert
        request.Notes.Should().Be(TestConstants.Content.Commerce.ValidRejectionNotes);
    }

    [Fact]
    public void AdminRejectPaymentRequest_WithNullNotes_ShouldConstructCorrectly()
    {
        // Act
        var request = new AdminRejectPaymentRequest(Notes: null);

        // Assert
        request.Notes.Should().BeNull();
    }
}
