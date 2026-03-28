using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Application.User.UseCases.Admin.Commands.UpdateAvatar.V1;
using _116.Identity.Domain.Enums;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.User.UseCases.Admin.Commands.UpdateAvatar.V1;

public class AdminUpdateAvatarEndpointV1Tests
{
    private static UserResponseDto CreateUserResponseDto() =>
        new(
            Guid.NewGuid(),
            "user@example.com",
            "testuser",
            [],
            [],
            EnumAuthProvider.Local,
            true,
            true,
            null,
            null,
            null,
            null,
            null,
            null
        );

    [Fact]
    public void AdminUpdateAvatarResponse_ShouldConstructCorrectly()
    {
        UserResponseDto user = CreateUserResponseDto();

        var response = new AdminUpdateAvatarResponse(User: user);

        response.Should().NotBeNull();
        response.User.Should().NotBeNull();
    }
}
