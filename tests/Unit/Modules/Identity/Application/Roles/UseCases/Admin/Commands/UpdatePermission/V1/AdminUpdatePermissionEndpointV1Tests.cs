using _116.Identity.Application.Roles.UseCases.Admin.Commands.UpdatePermission.V1;
using _116.Identity.Application.Shared.DTOs;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Commands.UpdatePermission.V1;

/// <summary>
/// Unit tests for <see cref="AdminUpdatePermissionResponse"/>.
/// </summary>
public class AdminUpdatePermissionEndpointV1Tests
{
    private static PermissionDto CreatePermissionDto() =>
        new(Guid.NewGuid(), "articles", "read", "Read articles", true, false, null);

    [Fact]
    public void AdminUpdatePermissionResponse_ShouldConstructCorrectly()
    {
        // Arrange
        PermissionDto permissionDto = CreatePermissionDto();

        // Act
        var response = new AdminUpdatePermissionResponse(Permission: permissionDto);

        // Assert
        response.Should().NotBeNull();
        response.Permission.Should().NotBeNull();
        response.Permission.Should().Be(permissionDto);
    }
}
