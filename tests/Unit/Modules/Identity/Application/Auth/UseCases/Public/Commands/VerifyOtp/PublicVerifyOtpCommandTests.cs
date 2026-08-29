using _116.BuildingBlocks.Constants.RateLimit;
using _116.Identity.Application.Auth.UseCases.Public.Commands.VerifyOtp;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Public.Commands.VerifyOtp;

/// <summary>
/// Unit tests for <see cref="PublicVerifyOtpCommand"/> verifying it opts into per-account throttling
/// under the OTP policy, keyed by the email.
/// </summary>
public class PublicVerifyOtpCommandTests
{
    [Fact]
    public void OptsIntoOtpThrottling_KeyedByEmail()
    {
        const string email = "user@example.com";
        var command = new PublicVerifyOtpCommand(email, "123456", "EmailVerification");

        command.RateLimitPolicy.Should().Be(RateLimitPolicies.Otp);
        command.AccountKey.Should().Be(email);
    }
}
