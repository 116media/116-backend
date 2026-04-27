using _116.Content.Application.Lookup.UseCases.Admin.Queries.GetAllTags.V1;
using _116.Content.Application.Shared.DTOs;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Queries.GetAllTags.V1;

/// <summary>
/// Unit tests for <see cref="AdminGetAllTagsResponse"/>.
/// </summary>
public class AdminGetAllTagsEndpointV1Tests
{
    [Fact]
    public void AdminGetAllTagsResponse_ShouldConstructCorrectly()
    {
        // Arrange
        IReadOnlyList<TagDto> tags = [CreateTagDto(), CreateTagDto()];

        // Act
        var response = new AdminGetAllTagsResponse(Tags: tags);

        // Assert
        response.Tags.Should().NotBeNull();
        response.Tags.Should().HaveCount(2);
    }

    private static TagDto CreateTagDto() => new(Guid.NewGuid(), "Technology", "technology");
}
