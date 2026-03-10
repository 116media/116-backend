using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Application.User.UseCases.Admin.Commands.UpdateOwnProfile.V1;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.User.UseCases.Admin.Commands.UpdateOwnProfile.V1;

public class AdminUpdateOwnProfileEndpointV1Tests
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
    public void AdminUpdateOwnProfileResponse_ShouldConstructCorrectly()
    {
        UserResponseDto user = CreateUserResponseDto();

        var response = new AdminUpdateOwnProfileResponse(User: user);

        response.Should().NotBeNull();
        response.User.Should().NotBeNull();
    }

    [Fact]
    public void AdminUpdateOwnProfileRequest_ShouldConstructCorrectly_WithValues()
    {
        var request = new AdminUpdateOwnProfileRequest(
            UserName: "adminuser",
            CountryName: "Rwanda",
            PartialPhoneNumber: "78000000",
            CountryIsoCode: "RW",
            CountryDialCode: "+250"
        );

        request.Should().NotBeNull();
        request.UserName.Should().Be("adminuser");
        request.CountryName.Should().Be("Rwanda");
        request.PartialPhoneNumber.Should().Be("78000000");
        request.CountryIsoCode.Should().Be("RW");
        request.CountryDialCode.Should().Be("+250");
    }

    [Fact]
    public void AdminUpdateOwnProfileRequest_ShouldConstructCorrectly_WithNulls()
    {
        var request = new AdminUpdateOwnProfileRequest(
            UserName: null,
            CountryName: null,
            PartialPhoneNumber: null,
            CountryIsoCode: null,
            CountryDialCode: null
        );

        request.Should().NotBeNull();
        request.UserName.Should().BeNull();
        request.CountryName.Should().BeNull();
        request.PartialPhoneNumber.Should().BeNull();
        request.CountryIsoCode.Should().BeNull();
        request.CountryDialCode.Should().BeNull();
    }
}
