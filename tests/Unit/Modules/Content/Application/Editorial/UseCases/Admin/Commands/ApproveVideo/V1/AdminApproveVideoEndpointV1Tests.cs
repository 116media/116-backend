using _116.Content.Application.Editorial.UseCases.Admin.Commands.ApproveVideo.V1;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.ApproveVideo.V1;

/// <summary>
/// Unit tests for <see cref="AdminApproveVideoResponse"/>.
/// </summary>
public class AdminApproveVideoEndpointV1Tests
{
    [Fact]
    public void AdminApproveVideoResponse_ShouldConstructCorrectly()
    {
        // Act
        var response = new AdminApproveVideoResponse(IsSuccess: true);

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
    }
}
