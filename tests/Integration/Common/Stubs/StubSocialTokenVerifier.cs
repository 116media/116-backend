using _116.Identity.Application.Adapters.SocialAuth;
using _116.Identity.Application.Auth.Exceptions;

namespace _116.Integration.Tests.Common.Stubs;

/// <summary>
/// In-memory stub replacing the real Google/Facebook token verifiers so integration tests never call
/// a provider. The payload is scriptable per test; the base classes reset it before each test.
/// </summary>
public class StubSocialTokenVerifier : ISocialTokenVerifier, IResettableStub
{
    /// <summary>
    /// The payload the next verification returns. Defaults to a verified, unique identity so the happy
    /// path needs no arrangement.
    /// </summary>
    public SocialTokenPayload NextPayload { get; set; } = DefaultPayload();

    /// <summary>
    /// When true, the next verification throws <see cref="SocialTokenVerificationException" /> (token
    /// could not be verified) instead of returning <see cref="NextPayload" />.
    /// </summary>
    public bool ThrowInvalid { get; set; }

    /// <inheritdoc />
    public void Reset()
    {
        NextPayload = DefaultPayload();
        ThrowInvalid = false;
    }

    /// <inheritdoc />
    public Task<SocialTokenPayload> VerifyAsync(string idToken, CancellationToken cancellationToken)
    {
        if (ThrowInvalid)
        {
            throw new SocialTokenVerificationException();
        }

        return Task.FromResult(NextPayload);
    }

    private static SocialTokenPayload DefaultPayload() =>
        new(
            ProviderSubjectId: $"sub-{Guid.NewGuid():N}",
            Email: $"social-{Guid.NewGuid():N}@test.com",
            EmailVerified: true,
            Name: "Social User",
            PictureUrl: null
        );
}
