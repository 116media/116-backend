using _116.Identity.Application.Roles.UseCases.Admin.Queries.GetAllPermissions.V1;
using _116.Identity.Application.Shared.DTOs;
using _116.Shared.Application.Pagination;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Queries.GetAllPermissions.V1;

/// <summary>
/// Unit tests for <see cref="AdminGetAllPermissionsResponse"/>.
/// </summary>
public class AdminGetAllPermissionsEndpointV1Tests
{
    private static PermissionDto CreatePermissionDto() =>
        new(Guid.NewGuid(), "articles", "read", "Read articles", true, false, null);

    [Fact]
    public void AdminGetAllPermissionsResponse_ShouldConstructCorrectly()
    {
        // Arrange
        var paginated = new PaginatedResult<PermissionDto>(1, 10, 1, [CreatePermissionDto()]);

        // Act
        var response = new AdminGetAllPermissionsResponse(Permissions: paginated);

        // Assert
        response.Should().NotBeNull();
        response.Permissions.Should().Be(paginated);
    }
}
