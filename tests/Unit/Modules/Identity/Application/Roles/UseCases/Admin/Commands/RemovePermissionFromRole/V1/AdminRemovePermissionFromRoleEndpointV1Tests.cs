using _116.Identity.Application.Roles.UseCases.Admin.Commands.RemovePermissionFromRole.V1;
using _116.Identity.Application.Shared.DTOs;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Commands.RemovePermissionFromRole.V1;

/// <summary>
/// Unit tests for <see cref="AdminRemovePermissionFromRoleResponse"/>.
/// </summary>
public class AdminRemovePermissionFromRoleEndpointV1Tests
{
    private static RoleWithPermissionsDto CreateRoleWithPermissionsDto() =>
        new(Guid.NewGuid(), "Admin", "Admin role", true, false, null, []);

    [Fact]
    public void AdminRemovePermissionFromRoleResponse_ShouldConstructCorrectly()
    {
        // Arrange
        RoleWithPermissionsDto roleDto = CreateRoleWithPermissionsDto();

        // Act
        var response = new AdminRemovePermissionFromRoleResponse(Role: roleDto, IsSuccess: true);

        // Assert
        response.Should().NotBeNull();
        response.Role.Should().NotBeNull();
        response.Role.Should().Be(roleDto);
        response.IsSuccess.Should().BeTrue();
    }
}
