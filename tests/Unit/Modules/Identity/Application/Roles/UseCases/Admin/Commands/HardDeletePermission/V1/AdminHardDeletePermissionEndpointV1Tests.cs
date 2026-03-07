using _116.Identity.Application.Roles.UseCases.Admin.Commands.HardDeletePermission.V1;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Commands.HardDeletePermission.V1;

/// <summary>
/// Unit tests for <see cref="AdminHardDeletePermissionResponse"/>.
/// </summary>
public class AdminHardDeletePermissionEndpointV1Tests
{
    [Fact]
    public void AdminHardDeletePermissionResponse_ShouldConstructCorrectly()
    {
        // Act
        var response = new AdminHardDeletePermissionResponse(IsSuccess: true);

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
    }
}
