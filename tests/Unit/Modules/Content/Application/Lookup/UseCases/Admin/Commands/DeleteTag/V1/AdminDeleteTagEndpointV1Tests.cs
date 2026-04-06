using _116.Content.Application.Lookup.UseCases.Admin.Commands.DeleteTag.V1;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.DeleteTag.V1;

/// <summary>
/// Unit tests for <see cref="AdminDeleteTagResponse"/>.
/// </summary>
public class AdminDeleteTagEndpointV1Tests
{
    [Fact]
    public void AdminDeleteTagResponse_ShouldConstructCorrectly()
    {
        // Arrange & Act
        var response = new AdminDeleteTagResponse(IsSuccess: true);

        // Assert
        response.IsSuccess.Should().BeTrue();
    }
}
