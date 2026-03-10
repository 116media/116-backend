using _116.Identity.Application.Auth.UseCases.Admin.Commands.ResendOtp.V1;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Admin.Commands.ResendOtp.V1;

public class AdminResendOtpEndpointV1Tests
{
    [Fact]
    public void AdminResendOtpResponse_ShouldConstructCorrectly()
    {
        var response = new AdminResendOtpResponse(IsSuccess: true);

        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
    }
}
