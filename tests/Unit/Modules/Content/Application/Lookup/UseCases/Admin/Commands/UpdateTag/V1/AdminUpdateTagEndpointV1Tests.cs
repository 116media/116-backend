using _116.Content.Application.Lookup.UseCases.Admin.Commands.UpdateTag.V1;
using _116.Content.Application.Shared.DTOs;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.UpdateTag.V1;

/// <summary>
/// Unit tests for <see cref="AdminUpdateTagResponse"/>.
/// </summary>
public class AdminUpdateTagEndpointV1Tests
{
    [Fact]
    public void AdminUpdateTagResponse_ShouldConstructCorrectly()
    {
        // Arrange
        TagDto tag = CreateTagDto();

        // Act
        var response = new AdminUpdateTagResponse(Tag: tag);

        // Assert
        response.Tag.Should().NotBeNull();
        response.Tag.Should().Be(tag);
    }

    private static TagDto CreateTagDto() => new(Guid.NewGuid(), "Technology", "technology");
}
