using _116.Content.Application.Commerce.UseCases.Admin.Commands.AttachPaymentProof.V1;
using _116.Content.Application.Shared.DTOs;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Commands.AttachPaymentProof.V1;

/// <summary>
/// Unit tests for <see cref="AdminAttachPaymentProofResponse"/>.
/// </summary>
public class AdminAttachPaymentProofEndpointV1Tests
{
    [Fact]
    public void AdminAttachPaymentProofResponse_ShouldConstructCorrectly()
    {
        // Arrange
        var proof = new FileDto(Guid.NewGuid(), "https://cdn.example.com/proof.jpg", "image/jpeg", "proof.jpg", 12345);

        // Act
        var response = new AdminAttachPaymentProofResponse(Proof: proof);

        // Assert
        response.Proof.Should().NotBeNull();
        response.Proof.Should().Be(proof);
    }
}
