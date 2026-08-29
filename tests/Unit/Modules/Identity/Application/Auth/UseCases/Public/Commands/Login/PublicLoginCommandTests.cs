using _116.BuildingBlocks.Constants.RateLimit;
using _116.Identity.Application.Auth.UseCases.Public.Commands.Login;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Public.Commands.Login;

/// <summary>
/// Unit tests for <see cref="PublicLoginCommand"/> verifying it opts into per-account throttling
/// under the authentication policy, keyed by the login credential.
/// </summary>
public class PublicLoginCommandTests
{
    [Fact]
    public void OptsIntoAuthenticationThrottling_KeyedByCredentials()
    {
        const string credentials = "user@example.com";
        var command = new PublicLoginCommand(credentials, "password");

        command.RateLimitPolicy.Should().Be(RateLimitPolicies.Authentication);
        command.AccountKey.Should().Be(credentials);
    }
}
