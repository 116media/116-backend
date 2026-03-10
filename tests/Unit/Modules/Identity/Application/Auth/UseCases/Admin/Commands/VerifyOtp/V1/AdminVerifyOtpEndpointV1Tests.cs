using _116.Identity.Application.Auth.UseCases.Admin.Commands.VerifyOtp.V1;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Admin.Commands.VerifyOtp.V1;

public class AdminVerifyOtpEndpointV1Tests
{
    [Fact]
    public void AdminVerifyOtpResponse_ShouldConstructCorrectly()
    {
        var response = new AdminVerifyOtpResponse(IsSuccess: true);

        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
    }
}
