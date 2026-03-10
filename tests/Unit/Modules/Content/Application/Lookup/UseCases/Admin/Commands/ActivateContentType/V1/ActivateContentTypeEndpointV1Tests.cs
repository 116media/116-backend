using _116.Content.Application.Lookup.UseCases.Admin.Commands.ActivateContentType.V1;
using _116.Content.Application.Shared.DTOs;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.ActivateContentType.V1;

/// <summary>
/// Unit tests for <see cref="ActivateContentTypeResponse"/>.
/// </summary>
public class ActivateContentTypeEndpointV1Tests
{
    [Fact]
    public void ActivateContentTypeResponse_ShouldConstructCorrectly()
    {
        // Arrange
        ContentTypeDto contentType = CreateContentTypeDto();

        // Act
        var response = new ActivateContentTypeResponse(ContentType: contentType);

        // Assert
        response.ContentType.Should().NotBeNull();
        response.ContentType.Should().Be(contentType);
    }

    private static ContentTypeDto CreateContentTypeDto() => new(Guid.NewGuid(), "Article", true);
}
