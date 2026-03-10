using _116.Identity.Application.Roles.UseCases.Admin.Commands.SoftDeleteRole.V1;
using _116.Identity.Application.Shared.DTOs;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Commands.SoftDeleteRole.V1;

/// <summary>
/// Unit tests for <see cref="AdminSoftDeleteRoleResponse"/>.
/// </summary>
public class AdminSoftDeleteRoleEndpointV1Tests
{
    private static RoleDto CreateRoleDto() => new(Guid.NewGuid(), "Admin", "Admin role", true, false, null);

    [Fact]
    public void AdminSoftDeleteRoleResponse_ShouldConstructCorrectly()
    {
        // Arrange
        RoleDto roleDto = CreateRoleDto();

        // Act
        var response = new AdminSoftDeleteRoleResponse(Role: roleDto, IsSuccess: true);

        // Assert
        response.Should().NotBeNull();
        response.Role.Should().NotBeNull();
        response.Role.Should().Be(roleDto);
        response.IsSuccess.Should().BeTrue();
    }
}
