using _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadVideoThumbnail.V1;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UploadVideoThumbnail.V1;

/// <summary>
/// Unit tests for <see cref="AdminUploadVideoThumbnailResponse"/>.
/// </summary>
public class AdminUploadVideoThumbnailEndpointV1Tests
{
    [Fact]
    public void AdminUploadVideoThumbnailResponse_ShouldConstructCorrectly()
    {
        // Act
        var response = new AdminUploadVideoThumbnailResponse(ThumbnailUrl: "Test", ThumbnailStorageKey: "Test");

        // Assert
        response.Should().NotBeNull();
        response.ThumbnailUrl.Should().Be("Test");
        response.ThumbnailStorageKey.Should().Be("Test");
    }
}
