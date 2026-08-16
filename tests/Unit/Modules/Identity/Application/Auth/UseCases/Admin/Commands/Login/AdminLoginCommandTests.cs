using _116.BuildingBlocks.Constants.RateLimit;
using _116.Identity.Application.Auth.UseCases.Admin.Commands.Login;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Admin.Commands.Login;

/// <summary>
/// Unit tests for <see cref="AdminLoginCommand"/> verifying it opts into per-account throttling under
/// the authentication policy, keyed by the admin email.
/// </summary>
public class AdminLoginCommandTests
{
    [Fact]
    public void OptsIntoAuthenticationThrottling_KeyedByEmail()
    {
        const string email = "admin@example.com";
        var command = new AdminLoginCommand(email, "password");

        command.RateLimitPolicy.Should().Be(RateLimitPolicies.Authentication);
        command.AccountKey.Should().Be(email);
    }
}
