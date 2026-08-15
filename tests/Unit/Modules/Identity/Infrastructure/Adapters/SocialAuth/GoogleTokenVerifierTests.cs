using _116.Identity.Application.Adapters.SocialAuth;
using _116.Identity.Application.Auth.Exceptions;
using _116.Identity.Infrastructure.Adapters.SocialAuth;
using AwesomeAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Infrastructure.Adapters.SocialAuth;

/// <summary>
/// Unit tests for <see cref="GoogleTokenVerifier"/>. A token that does not parse as a valid Google JWT
/// is rejected as <see cref="SocialTokenVerificationException"/> — the adapter never leaks the raw
/// <c>InvalidJwtException</c>. The happy path needs a Google-signed token and is covered by the
/// provider-side contract, not a unit test.
/// </summary>
public class GoogleTokenVerifierTests
{
    private static GoogleTokenVerifier Verifier()
    {
        IOptions<SocialAuthOptions> options = Options.Create(new SocialAuthOptions { GoogleClientId = "client-id" });
        return new GoogleTokenVerifier(options);
    }

    [Theory]
    [InlineData("not-a-jwt")]
    [InlineData("header.payload")]
    [InlineData("")]
    public async Task VerifyAsync_WithMalformedToken_ThrowsSocialTokenVerification(string idToken)
    {
        // Act
        Func<Task> act = async () => await Verifier().VerifyAsync(idToken, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<SocialTokenVerificationException>();
    }
}
