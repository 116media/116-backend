using _116.Content.Application.Editorial.UseCases.Admin.Commands.ArchiveVideo.V1;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.ArchiveVideo.V1;

/// <summary>
/// Unit tests for <see cref="AdminArchiveVideoResponse"/>.
/// </summary>
public class AdminArchiveVideoEndpointV1Tests
{
    [Fact]
    public void AdminArchiveVideoResponse_ShouldConstructCorrectly()
    {
        // Act
        var response = new AdminArchiveVideoResponse(IsSuccess: true);

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
    }
}
