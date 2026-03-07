using _116.Identity.Application.Roles.UseCases.Admin.Commands.DeactivateRole.V1;
using _116.Identity.Application.Shared.DTOs;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Commands.DeactivateRole.V1;

/// <summary>
/// Unit tests for <see cref="AdminDeactivateRoleResponse"/>.
/// </summary>
public class AdminDeactivateRoleEndpointV1Tests
{
    private static RoleDto CreateRoleDto() => new(Guid.NewGuid(), "Admin", "Admin role", true, false, null);

    [Fact]
    public void AdminDeactivateRoleResponse_ShouldConstructCorrectly()
    {
        // Arrange
        RoleDto roleDto = CreateRoleDto();

        // Act
        var response = new AdminDeactivateRoleResponse(Role: roleDto);

        // Assert
        response.Should().NotBeNull();
        response.Role.Should().NotBeNull();
        response.Role.Should().Be(roleDto);
    }
}
