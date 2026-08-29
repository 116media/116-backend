using _116.Identity.Application.Adapters.SocialAuth;
using _116.Identity.Application.Auth.Exceptions;
using _116.Identity.Domain.Enums;
using _116.Identity.Infrastructure.Adapters.SocialAuth;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Infrastructure.Adapters.SocialAuth;

/// <summary>
/// Unit tests for <see cref="SocialTokenVerifierFactory"/>. The factory resolves the keyed verifier
/// registered for a provider and refuses a provider that has none.
/// </summary>
public class SocialTokenVerifierFactoryTests
{
    private static SocialTokenVerifierFactory Factory(EnumAuthProvider registeredFor)
    {
        ServiceProvider provider = new ServiceCollection()
            .AddKeyedScoped<ISocialTokenVerifier, FakeVerifier>(registeredFor)
            .BuildServiceProvider();

        return new SocialTokenVerifierFactory(provider);
    }

    [Fact]
    public void For_WithRegisteredProvider_ReturnsKeyedVerifier()
    {
        // Arrange
        SocialTokenVerifierFactory factory = Factory(EnumAuthProvider.Google);

        // Act
        ISocialTokenVerifier verifier = factory.For(EnumAuthProvider.Google);

        // Assert
        verifier.Should().BeOfType<FakeVerifier>();
    }

    [Fact]
    public void For_WithUnregisteredProvider_ThrowsUnsupportedProvider()
    {
        // Arrange — only Google is registered, so Facebook has no verifier
        SocialTokenVerifierFactory factory = Factory(EnumAuthProvider.Google);

        // Act
        Action act = () => factory.For(EnumAuthProvider.Facebook);

        // Assert
        act.Should().Throw<UnsupportedProviderException>().Which.Provider.Should().Be(EnumAuthProvider.Facebook);
    }

    private sealed class FakeVerifier : ISocialTokenVerifier
    {
        public Task<SocialTokenPayload> VerifyAsync(string idToken, CancellationToken cancellationToken) =>
            throw new NotSupportedException();
    }
}
