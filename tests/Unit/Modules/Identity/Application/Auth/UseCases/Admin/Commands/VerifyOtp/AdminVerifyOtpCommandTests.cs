using _116.BuildingBlocks.Constants.RateLimit;
using _116.Identity.Application.Auth.UseCases.Admin.Commands.VerifyOtp;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Admin.Commands.VerifyOtp;

/// <summary>
/// Unit tests for <see cref="AdminVerifyOtpCommand"/> verifying it opts into per-account throttling
/// under the OTP policy, keyed by the email.
/// </summary>
public class AdminVerifyOtpCommandTests
{
    [Fact]
    public void OptsIntoOtpThrottling_KeyedByEmail()
    {
        const string email = "admin@example.com";
        var command = new AdminVerifyOtpCommand(email, "123456", "EmailVerification");

        command.RateLimitPolicy.Should().Be(RateLimitPolicies.Otp);
        command.AccountKey.Should().Be(email);
    }
}
