using _116.Content.Application.Lookup.UseCases.Admin.Commands.CreateContentType.V1;
using _116.Content.Application.Shared.DTOs;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.CreateContentType.V1;

/// <summary>
/// Unit tests for <see cref="AdminCreateContentTypeResponse"/>.
/// </summary>
public class AdminCreateContentTypeEndpointV1Tests
{
    [Fact]
    public void AdminCreateContentTypeResponse_ShouldConstructCorrectly()
    {
        // Arrange
        ContentTypeDto contentType = CreateContentTypeDto();

        // Act
        var response = new AdminCreateContentTypeResponse(ContentType: contentType);

        // Assert
        response.ContentType.Should().NotBeNull();
        response.ContentType.Should().Be(contentType);
    }

    private static ContentTypeDto CreateContentTypeDto() => new(Guid.NewGuid(), "Article", true);
}
