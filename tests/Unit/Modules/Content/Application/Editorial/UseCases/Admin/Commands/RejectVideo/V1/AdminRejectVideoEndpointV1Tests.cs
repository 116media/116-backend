using _116.Content.Application.Editorial.UseCases.Admin.Commands.RejectVideo.V1;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.RejectVideo.V1;

/// <summary>
/// Unit tests for <see cref="AdminRejectVideoResponse"/>.
/// </summary>
public class AdminRejectVideoEndpointV1Tests
{
    [Fact]
    public void AdminRejectVideoResponse_ShouldConstructCorrectly()
    {
        // Act
        var response = new AdminRejectVideoResponse(IsSuccess: true);

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
    }
}
