using _116.Identity.Application.Roles.UseCases.Admin.Commands.SoftDeletePermission.V1;
using _116.Identity.Application.Shared.DTOs;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Commands.SoftDeletePermission.V1;

/// <summary>
/// Unit tests for <see cref="AdminSoftDeletePermissionResponse"/>.
/// </summary>
public class AdminSoftDeletePermissionEndpointV1Tests
{
    private static PermissionDto CreatePermissionDto() =>
        new(Guid.NewGuid(), "articles", "read", "Read articles", true, false, null);

    [Fact]
    public void AdminSoftDeletePermissionResponse_ShouldConstructCorrectly()
    {
        // Arrange
        PermissionDto permissionDto = CreatePermissionDto();

        // Act
        var response = new AdminSoftDeletePermissionResponse(Permission: permissionDto, IsSuccess: true);

        // Assert
        response.Should().NotBeNull();
        response.Permission.Should().NotBeNull();
        response.Permission.Should().Be(permissionDto);
        response.IsSuccess.Should().BeTrue();
    }
}
