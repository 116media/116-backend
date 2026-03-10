using _116.Identity.Application.Auth.UseCases.Public.Commands.ResendOtp.V1;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Public.Commands.ResendOtp.V1;

public class PublicResendOtpEndpointV1Tests
{
    [Fact]
    public void PublicResendOtpResponse_ShouldConstructCorrectly()
    {
        var response = new PublicResendOtpResponse(IsSuccess: true);

        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
    }
}
