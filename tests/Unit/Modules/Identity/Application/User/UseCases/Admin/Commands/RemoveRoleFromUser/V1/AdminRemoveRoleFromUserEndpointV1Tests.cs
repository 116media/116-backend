using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Application.User.UseCases.Admin.Commands.RemoveRoleFromUser.V1;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.User.UseCases.Admin.Commands.RemoveRoleFromUser.V1;

public class AdminRemoveRoleFromUserEndpointV1Tests
{
    [Fact]
    public void AdminRemoveRoleFromUserResponse_ShouldConstructCorrectly()
    {
        IReadOnlyCollection<RoleDto> roles = [];

        var response = new AdminRemoveRoleFromUserResponse(Roles: roles, IsSuccess: true);

        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
    }
}
