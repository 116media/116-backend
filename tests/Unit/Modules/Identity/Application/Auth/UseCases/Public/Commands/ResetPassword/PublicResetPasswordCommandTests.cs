using _116.BuildingBlocks.Constants.RateLimit;
using _116.Identity.Application.Auth.UseCases.Public.Commands.ResetPassword;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Public.Commands.ResetPassword;

/// <summary>
/// Unit tests for <see cref="PublicResetPasswordCommand"/> verifying it opts into per-account
/// throttling under the password-management policy, keyed by the email.
/// </summary>
public class PublicResetPasswordCommandTests
{
    [Fact]
    public void OptsIntoPasswordManagementThrottling_KeyedByEmail()
    {
        const string email = "user@example.com";
        var command = new PublicResetPasswordCommand(email, "123456", "NewPassword1!");

        command.RateLimitPolicy.Should().Be(RateLimitPolicies.PasswordManagement);
        command.AccountKey.Should().Be(email);
    }
}
