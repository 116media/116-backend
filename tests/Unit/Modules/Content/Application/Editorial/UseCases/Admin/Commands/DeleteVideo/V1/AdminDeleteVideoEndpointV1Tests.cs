using _116.Content.Application.Editorial.UseCases.Admin.Commands.DeleteVideo.V1;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.DeleteVideo.V1;

/// <summary>
/// Unit tests for <see cref="AdminDeleteVideoResponse"/>.
/// </summary>
public class AdminDeleteVideoEndpointV1Tests
{
    [Fact]
    public void AdminDeleteVideoResponse_ShouldConstructCorrectly()
    {
        // Act
        var response = new AdminDeleteVideoResponse(IsSuccess: true);

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
    }
}
