using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Application.User.UseCases.Public.Commands.UpdateOwnProfile.V1;
using _116.Identity.Domain.Enums;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.User.UseCases.Public.Commands.UpdateOwnProfile.V1;

public class PublicUpdateOwnProfileEndpointV1Tests
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
    public void PublicUpdateOwnProfileResponse_ShouldConstructCorrectly()
    {
        UserResponseDto user = CreateUserResponseDto();

        var response = new PublicUpdateOwnProfileResponse(User: user);

        response.Should().NotBeNull();
        response.User.Should().NotBeNull();
    }

    [Fact]
    public void PublicUpdateOwnProfileRequest_ShouldConstructCorrectly_WithValues()
    {
        var request = new PublicUpdateOwnProfileRequest(
            Email: "new@example.com",
            UserName: "newuser",
            CountryName: "Rwanda",
            PartialPhoneNumber: "78000000",
            CountryIsoCode: "RW",
            CountryDialCode: "+250"
        );

        request.Should().NotBeNull();
        request.Email.Should().Be("new@example.com");
        request.UserName.Should().Be("newuser");
        request.CountryName.Should().Be("Rwanda");
        request.PartialPhoneNumber.Should().Be("78000000");
        request.CountryIsoCode.Should().Be("RW");
        request.CountryDialCode.Should().Be("+250");
    }

    [Fact]
    public void PublicUpdateOwnProfileRequest_ShouldConstructCorrectly_WithNulls()
    {
        var request = new PublicUpdateOwnProfileRequest(
            Email: null,
            UserName: null,
            CountryName: null,
            PartialPhoneNumber: null,
            CountryIsoCode: null,
            CountryDialCode: null
        );

        request.Should().NotBeNull();
        request.Email.Should().BeNull();
        request.UserName.Should().BeNull();
        request.CountryName.Should().BeNull();
        request.PartialPhoneNumber.Should().BeNull();
        request.CountryIsoCode.Should().BeNull();
        request.CountryDialCode.Should().BeNull();
    }
}
