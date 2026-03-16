using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideoTags.V1;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideoTags.V1;

/// <summary>
/// Unit tests for <see cref="AdminUpdateVideoTagsResponse"/>.
/// </summary>
public class AdminUpdateVideoTagsEndpointV1Tests
{
    [Fact]
    public void AdminUpdateVideoTagsResponse_ShouldConstructCorrectly()
    {
        // Act
        var response = new AdminUpdateVideoTagsResponse(IsSuccess: true);

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
    }
}
