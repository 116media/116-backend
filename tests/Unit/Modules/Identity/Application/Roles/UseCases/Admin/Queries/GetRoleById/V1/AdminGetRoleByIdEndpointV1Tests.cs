using _116.Identity.Application.Roles.UseCases.Admin.Queries.GetRoleById.V1;
using _116.Identity.Application.Shared.DTOs;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Queries.GetRoleById.V1;

/// <summary>
/// Unit tests for <see cref="AdminGetRoleByIdResponse"/>.
/// </summary>
public class AdminGetRoleByIdEndpointV1Tests
{
    private static RoleDto CreateRoleDto() => new(Guid.NewGuid(), "Admin", "Admin role", true, false, null);

    private static PermissionDto CreatePermissionDto() =>
        new(Guid.NewGuid(), "articles", "read", "Read articles", true, false, null);

    [Fact]
    public void AdminGetRoleByIdResponse_ShouldConstructCorrectly()
    {
        // Arrange
        RoleDto roleDto = CreateRoleDto();
        IReadOnlyCollection<PermissionDto> permissions = [CreatePermissionDto()];

        // Act
        var response = new AdminGetRoleByIdResponse(Role: roleDto, Permissions: permissions);

        // Assert
        response.Should().NotBeNull();
        response.Role.Should().NotBeNull();
        response.Role.Should().Be(roleDto);
        response.Permissions.Should().HaveCount(1);
    }
}
