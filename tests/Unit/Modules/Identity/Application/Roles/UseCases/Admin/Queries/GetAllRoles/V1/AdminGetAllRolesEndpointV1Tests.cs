using _116.Identity.Application.Roles.UseCases.Admin.Queries.GetAllRoles.V1;
using _116.Identity.Application.Shared.DTOs;
using _116.Shared.Application.Pagination;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Queries.GetAllRoles.V1;

/// <summary>
/// Unit tests for <see cref="AdminGetAllRolesResponse"/>.
/// </summary>
public class AdminGetAllRolesEndpointV1Tests
{
    private static RoleDto CreateRoleDto() => new(Guid.NewGuid(), "Admin", "Admin role", true, false, null);

    [Fact]
    public void AdminGetAllRolesResponse_ShouldConstructCorrectly()
    {
        // Arrange
        var paginated = new PaginatedResult<RoleDto>(1, 10, 1, [CreateRoleDto()]);

        // Act
        var response = new AdminGetAllRolesResponse(Roles: paginated);

        // Assert
        response.Should().NotBeNull();
        response.Roles.Should().Be(paginated);
    }
}
