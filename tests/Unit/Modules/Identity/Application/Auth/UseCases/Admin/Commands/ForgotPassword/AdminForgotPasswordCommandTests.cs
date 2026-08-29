using _116.BuildingBlocks.Constants.RateLimit;
using _116.Identity.Application.Auth.UseCases.Admin.Commands.ForgotPassword;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Admin.Commands.ForgotPassword;

/// <summary>
/// Unit tests for <see cref="AdminForgotPasswordCommand"/> verifying it opts into per-account
/// throttling under the password-management policy, keyed by the email.
/// </summary>
public class AdminForgotPasswordCommandTests
{
    [Fact]
    public void OptsIntoPasswordManagementThrottling_KeyedByEmail()
    {
        const string email = "admin@example.com";
        var command = new AdminForgotPasswordCommand(email);

        command.RateLimitPolicy.Should().Be(RateLimitPolicies.PasswordManagement);
        command.AccountKey.Should().Be(email);
    }
}
