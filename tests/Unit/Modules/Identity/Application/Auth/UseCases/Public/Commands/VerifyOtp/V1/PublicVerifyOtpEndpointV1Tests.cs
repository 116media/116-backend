using _116.Identity.Application.Auth.UseCases.Public.Commands.VerifyOtp.V1;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Public.Commands.VerifyOtp.V1;

public class PublicVerifyOtpEndpointV1Tests
{
    [Fact]
    public void PublicVerifyOtpResponse_ShouldConstructCorrectly()
    {
        var response = new PublicVerifyOtpResponse(IsSuccess: true);

        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
    }
}
