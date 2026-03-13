using _116.Content.Application.Editorial.UseCases.Admin.Commands.PublishVideo.V1;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.PublishVideo.V1;

/// <summary>
/// Unit tests for <see cref="AdminPublishVideoResponse"/>.
/// </summary>
public class AdminPublishVideoEndpointV1Tests
{
    [Fact]
    public void AdminPublishVideoResponse_ShouldConstructCorrectly()
    {
        // Act
        var response = new AdminPublishVideoResponse(IsSuccess: true);

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
    }
}
