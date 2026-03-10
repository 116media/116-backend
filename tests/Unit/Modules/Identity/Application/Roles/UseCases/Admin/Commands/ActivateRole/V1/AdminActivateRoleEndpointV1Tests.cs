using _116.Identity.Application.Roles.UseCases.Admin.Commands.ActivateRole.V1;
using _116.Identity.Application.Shared.DTOs;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Commands.ActivateRole.V1;

/// <summary>
/// Unit tests for <see cref="AdminActivateRoleResponse"/>.
/// </summary>
public class AdminActivateRoleEndpointV1Tests
{
    private static RoleDto CreateRoleDto() => new(Guid.NewGuid(), "Admin", "Admin role", true, false, null);

    [Fact]
    public void AdminActivateRoleResponse_ShouldConstructCorrectly()
    {
        // Arrange
        RoleDto roleDto = CreateRoleDto();

        // Act
        var response = new AdminActivateRoleResponse(Role: roleDto);

        // Assert
        response.Should().NotBeNull();
        response.Role.Should().NotBeNull();
        response.Role.Should().Be(roleDto);
    }
}
