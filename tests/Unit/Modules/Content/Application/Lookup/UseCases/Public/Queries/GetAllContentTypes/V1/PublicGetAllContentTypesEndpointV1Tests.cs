using _116.Content.Application.Lookup.UseCases.Public.Queries.GetAllContentTypes.V1;
using _116.Content.Application.Shared.DTOs;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.UseCases.Public.Queries.GetAllContentTypes.V1;

/// <summary>
/// Unit tests for <see cref="PublicGetAllContentTypesResponse"/>.
/// </summary>
public class PublicGetAllContentTypesEndpointV1Tests
{
    [Fact]
    public void PublicGetAllContentTypesResponse_ShouldConstructCorrectly()
    {
        // Arrange
        IReadOnlyList<ContentTypeDto> contentTypes = [CreateContentTypeDto(), CreateContentTypeDto()];

        // Act
        var response = new PublicGetAllContentTypesResponse(ContentTypes: contentTypes);

        // Assert
        response.ContentTypes.Should().NotBeNull();
        response.ContentTypes.Should().HaveCount(2);
    }

    [Fact]
    public void PublicGetAllContentTypesResponse_WithEmptyList_ShouldBeEmpty()
    {
        // Arrange
        IReadOnlyList<ContentTypeDto> contentTypes = [];

        // Act
        var response = new PublicGetAllContentTypesResponse(ContentTypes: contentTypes);

        // Assert
        response.ContentTypes.Should().NotBeNull();
        response.ContentTypes.Should().BeEmpty();
    }

    private static ContentTypeDto CreateContentTypeDto() => new(Guid.NewGuid(), "Article", true);
}
