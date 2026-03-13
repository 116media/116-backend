using _116.Content.Application.Editorial.UseCases.Admin.Commands.ActivateShortVideo.V1;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.ActivateShortVideo.V1;

/// <summary>
/// Unit tests for <see cref="AdminActivateShortVideoResponse"/>.
/// </summary>
public class AdminActivateShortVideoEndpointV1Tests
{
    [Fact]
    public void AdminActivateShortVideoResponse_ShouldConstructCorrectly()
    {
        // Act
        var response = new AdminActivateShortVideoResponse(IsSuccess: true);

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
    }
}
