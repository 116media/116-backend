using _116.Content.Application.Lookup.UseCases.Public.Queries.GetPopularTags.V1;
using _116.Content.Application.Shared.DTOs;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.UseCases.Public.Queries.GetPopularTags.V1;

/// <summary>
/// Unit tests for <see cref="PublicGetPopularTagsResponse"/>.
/// </summary>
public class PublicGetPopularTagsEndpointV1Tests
{
    [Fact]
    public void PublicGetPopularTagsResponse_ShouldConstructCorrectly()
    {
        // Arrange
        IReadOnlyList<TagDto> tags = [CreateTagDto(), CreateTagDto(), CreateTagDto()];

        // Act
        var response = new PublicGetPopularTagsResponse(Tags: tags);

        // Assert
        response.Tags.Should().NotBeNull();
        response.Tags.Should().HaveCount(3);
    }

    [Fact]
    public void PublicGetPopularTagsResponse_WithEmptyList_ShouldBeEmpty()
    {
        // Arrange & Act
        var response = new PublicGetPopularTagsResponse(Tags: []);

        // Assert
        response.Tags.Should().NotBeNull();
        response.Tags.Should().BeEmpty();
    }

    private static TagDto CreateTagDto() => new(Guid.NewGuid(), "Fally Ipupa", "fally-ipupa");
}
