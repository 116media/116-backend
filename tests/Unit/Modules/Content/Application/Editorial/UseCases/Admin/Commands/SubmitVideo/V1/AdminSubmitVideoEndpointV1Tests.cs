using _116.Content.Application.Editorial.UseCases.Admin.Commands.SubmitVideo.V1;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.SubmitVideo.V1;

/// <summary>
/// Unit tests for <see cref="AdminSubmitVideoResponse"/>.
/// </summary>
public class AdminSubmitVideoEndpointV1Tests
{
    [Fact]
    public void AdminSubmitVideoResponse_ShouldConstructCorrectly()
    {
        // Act
        var response = new AdminSubmitVideoResponse(IsSuccess: true);

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
    }
}
