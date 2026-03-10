using _116.Identity.Application.Roles.UseCases.Admin.Commands.RestoreRole.V1;
using _116.Identity.Application.Shared.DTOs;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Commands.RestoreRole.V1;

/// <summary>
/// Unit tests for <see cref="AdminRestoreRoleResponse"/>.
/// </summary>
public class AdminRestoreRoleEndpointV1Tests
{
    private static RoleDto CreateRoleDto() => new(Guid.NewGuid(), "Admin", "Admin role", true, false, null);

    [Fact]
    public void AdminRestoreRoleResponse_ShouldConstructCorrectly()
    {
        // Arrange
        RoleDto roleDto = CreateRoleDto();

        // Act
        var response = new AdminRestoreRoleResponse(Role: roleDto);

        // Assert
        response.Should().NotBeNull();
        response.Role.Should().NotBeNull();
        response.Role.Should().Be(roleDto);
    }
}
