using _116.Identity.Application.Roles.UseCases.Admin.Queries.GetOwnRoles.V1;
using _116.Identity.Application.Shared.DTOs;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Queries.GetOwnRoles.V1;

/// <summary>
/// Unit tests for <see cref="AdminGetOwnRolesResponse"/>.
/// </summary>
public class AdminGetOwnRolesEndpointV1Tests
{
    private static RoleWithPermissionsDto CreateRoleWithPermissionsDto() =>
        new(Guid.NewGuid(), "Admin", "Admin role", true, false, null, []);

    [Fact]
    public void AdminGetOwnRolesResponse_ShouldConstructCorrectly()
    {
        // Arrange
        IReadOnlyList<RoleWithPermissionsDto> roles = [CreateRoleWithPermissionsDto()];

        // Act
        var response = new AdminGetOwnRolesResponse(Roles: roles);

        // Assert
        response.Should().NotBeNull();
        response.Roles.Should().HaveCount(1);
    }
}
