using _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadShortVideoThumbnail.V1;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UploadShortVideoThumbnail.V1;

/// <summary>
/// Unit tests for <see cref="AdminUploadShortVideoThumbnailResponse"/>.
/// </summary>
public class AdminUploadShortVideoThumbnailEndpointV1Tests
{
    [Fact]
    public void AdminUploadShortVideoThumbnailResponse_ShouldConstructCorrectly()
    {
        // Act
        var response = new AdminUploadShortVideoThumbnailResponse(ThumbnailUrl: "Test", ThumbnailStorageKey: "Test");

        // Assert
        response.Should().NotBeNull();
        response.ThumbnailUrl.Should().Be("Test");
        response.ThumbnailStorageKey.Should().Be("Test");
    }
}
