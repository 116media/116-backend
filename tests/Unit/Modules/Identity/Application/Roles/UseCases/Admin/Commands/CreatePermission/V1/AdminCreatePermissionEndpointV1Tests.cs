using _116.Identity.Application.Roles.UseCases.Admin.Commands.CreatePermission.V1;
using _116.Identity.Application.Shared.DTOs;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Commands.CreatePermission.V1;

/// <summary>
/// Unit tests for <see cref="AdminCreatePermissionResponse"/>.
/// </summary>
public class AdminCreatePermissionEndpointV1Tests
{
    private static PermissionDto CreatePermissionDto() =>
        new(Guid.NewGuid(), "articles", "read", "Read articles", true, false, null);

    [Fact]
    public void AdminCreatePermissionResponse_ShouldConstructCorrectly()
    {
        // Arrange
        PermissionDto permissionDto = CreatePermissionDto();

        // Act
        var response = new AdminCreatePermissionResponse(Permission: permissionDto);

        // Assert
        response.Should().NotBeNull();
        response.Permission.Should().NotBeNull();
        response.Permission.Should().Be(permissionDto);
    }
}
