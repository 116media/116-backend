using _116.Content.Application.Editorial.UseCases.Admin.Commands.DeleteLyrics.V1;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.DeleteLyrics.V1;

/// <summary>
/// Unit tests for <see cref="AdminDeleteLyricsResponse"/>.
/// </summary>
public class AdminDeleteLyricsEndpointV1Tests
{
    [Fact]
    public void AdminDeleteLyricsResponse_ShouldConstructCorrectly()
    {
        // Act
        var response = new AdminDeleteLyricsResponse(IsSuccess: true);

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
    }
}
