using _116.Identity.Application.Roles.UseCases.Public.Queries.GetOwnRoles.V1;
using _116.Identity.Application.Shared.DTOs;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Roles.UseCases.Public.Queries.GetOwnRoles.V1;

/// <summary>
/// Unit tests for <see cref="PublicGetOwnRolesResponse"/>.
/// </summary>
public class PublicGetOwnRolesEndpointV1Tests
{
    private static RoleWithPermissionsDto CreateRoleWithPermissionsDto() =>
        new(Guid.NewGuid(), "Admin", "Admin role", true, false, null, []);

    [Fact]
    public void PublicGetOwnRolesResponse_ShouldConstructCorrectly()
    {
        // Arrange
        IReadOnlyList<RoleWithPermissionsDto> roles = [CreateRoleWithPermissionsDto()];

        // Act
        var response = new PublicGetOwnRolesResponse(Roles: roles);

        // Assert
        response.Should().NotBeNull();
        response.Roles.Should().HaveCount(1);
    }
}
