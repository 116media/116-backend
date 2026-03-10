using _116.Content.Application.Lookup.UseCases.Admin.Queries.GetAllContentTypes.V1;
using _116.Content.Application.Shared.DTOs;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Queries.GetAllContentTypes.V1;

/// <summary>
/// Unit tests for <see cref="GetAllContentTypesResponse"/>.
/// </summary>
public class GetAllContentTypesEndpointV1Tests
{
    [Fact]
    public void GetAllContentTypesResponse_ShouldConstructCorrectly()
    {
        // Arrange
        IReadOnlyList<ContentTypeDto> contentTypes = [CreateContentTypeDto(), CreateContentTypeDto()];

        // Act
        var response = new GetAllContentTypesResponse(ContentTypes: contentTypes);

        // Assert
        response.ContentTypes.Should().NotBeNull();
        response.ContentTypes.Should().HaveCount(2);
    }

    private static ContentTypeDto CreateContentTypeDto() => new(Guid.NewGuid(), "Article", true);
}
