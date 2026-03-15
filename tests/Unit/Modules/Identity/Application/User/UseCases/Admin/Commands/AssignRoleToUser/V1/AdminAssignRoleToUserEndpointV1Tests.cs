using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Application.User.UseCases.Admin.Commands.AssignRoleToUser.V1;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.User.UseCases.Admin.Commands.AssignRoleToUser.V1;

public class AdminAssignRoleToUserEndpointV1Tests
{
    private static RoleDto CreateRoleDto() => new(Guid.NewGuid(), "Admin", "Admin role", true, false, null);

    [Fact]
    public void AdminAssignRoleToUserResponse_ShouldConstructCorrectly()
    {
        IReadOnlyCollection<RoleDto> roles = [CreateRoleDto()];

        var response = new AdminAssignRoleToUserResponse(Roles: roles);

        response.Should().NotBeNull();
        response.Roles.Should().ContainSingle();
    }
}
