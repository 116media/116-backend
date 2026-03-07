using _116.Identity.Application.Roles.UseCases.Admin.Commands.HardDeleteRole.V1;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Commands.HardDeleteRole.V1;

/// <summary>
/// Unit tests for <see cref="AdminHardDeleteRoleResponse"/>.
/// </summary>
public class AdminHardDeleteRoleEndpointV1Tests
{
    [Fact]
    public void AdminHardDeleteRoleResponse_ShouldConstructCorrectly()
    {
        // Act
        var response = new AdminHardDeleteRoleResponse(IsSuccess: true);

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
    }
}
