using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Application.User.UseCases.Admin.Queries.GetOwnProfile.V1;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.User.UseCases.Admin.Queries.GetOwnProfile.V1;

public class AdminGetOwnProfileEndpointV1Tests
{
    private static UserResponseDto CreateUserResponseDto() =>
        new(
            Guid.NewGuid(),
            "user@example.com",
            "testuser",
            [],
            [],
            "Local",
            true,
            true,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null
        );

    [Fact]
    public void AdminGetOwnProfileResponse_ShouldConstructCorrectly()
    {
        UserResponseDto user = CreateUserResponseDto();

        var response = new AdminGetOwnProfileResponse(User: user);

        response.Should().NotBeNull();
        response.User.Should().NotBeNull();
    }
}
