using _116.Content.Application.Editorial.UseCases.Admin.Commands.DeleteShortVideo.V1;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.DeleteShortVideo.V1;

/// <summary>
/// Unit tests for <see cref="AdminDeleteShortVideoResponse"/>.
/// </summary>
public class AdminDeleteShortVideoEndpointV1Tests
{
    [Fact]
    public void AdminDeleteShortVideoResponse_ShouldConstructCorrectly()
    {
        // Act
        var response = new AdminDeleteShortVideoResponse(IsSuccess: true);

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
    }
}
