using _116.Content.Application.Lookup.UseCases.Admin.Commands.CreateTag.V1;
using _116.Content.Application.Shared.DTOs;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.CreateTag.V1;

/// <summary>
/// Unit tests for <see cref="CreateTagResponse"/>.
/// </summary>
public class CreateTagEndpointV1Tests
{
    [Fact]
    public void CreateTagResponse_ShouldConstructCorrectly()
    {
        // Arrange
        TagDto tag = CreateTagDto();

        // Act
        var response = new CreateTagResponse(Tag: tag);

        // Assert
        response.Tag.Should().NotBeNull();
        response.Tag.Should().Be(tag);
    }

    private static TagDto CreateTagDto() => new(Guid.NewGuid(), "Technology", "technology");
}
