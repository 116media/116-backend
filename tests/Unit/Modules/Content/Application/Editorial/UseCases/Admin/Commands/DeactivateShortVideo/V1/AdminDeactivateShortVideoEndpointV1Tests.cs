using _116.Content.Application.Editorial.UseCases.Admin.Commands.DeactivateShortVideo.V1;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.DeactivateShortVideo.V1;

/// <summary>
/// Unit tests for <see cref="AdminDeactivateShortVideoResponse"/>.
/// </summary>
public class AdminDeactivateShortVideoEndpointV1Tests
{
    [Fact]
    public void AdminDeactivateShortVideoResponse_ShouldConstructCorrectly()
    {
        // Act
        var response = new AdminDeactivateShortVideoResponse(IsSuccess: true);

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
    }
}
