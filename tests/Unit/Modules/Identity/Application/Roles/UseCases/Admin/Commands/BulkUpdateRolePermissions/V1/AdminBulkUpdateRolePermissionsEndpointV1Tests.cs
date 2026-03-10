using _116.Identity.Application.Roles.UseCases.Admin.Commands.BulkUpdateRolePermissions.V1;
using _116.Identity.Application.Shared.DTOs;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Commands.BulkUpdateRolePermissions.V1;

/// <summary>
/// Unit tests for <see cref="AdminBulkUpdateRolePermissionsResponse"/>.
/// </summary>
public class AdminBulkUpdateRolePermissionsEndpointV1Tests
{
    private static RoleWithPermissionsDto CreateRoleWithPermissionsDto() =>
        new(Guid.NewGuid(), "Admin", "Admin role", true, false, null, []);

    [Fact]
    public void AdminBulkUpdateRolePermissionsResponse_ShouldConstructCorrectly()
    {
        // Arrange
        RoleWithPermissionsDto roleDto = CreateRoleWithPermissionsDto();

        // Act
        var response = new AdminBulkUpdateRolePermissionsResponse(Role: roleDto);

        // Assert
        response.Should().NotBeNull();
        response.Role.Should().NotBeNull();
        response.Role.Should().Be(roleDto);
    }
}
