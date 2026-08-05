using _116.Identity.Application.Session.UseCases.Public.Commands.RefreshToken.V1;
using _116.Tests.Fixtures.Helpers;
using Bogus;

namespace _116.Tests.Fixtures.Builders.Requests.Identity;

/// <summary>
/// Fluent builder for creating <see cref="PublicRefreshTokenRequest"/> instances in tests.
/// </summary>
public class PublicRefreshTokenRequestBuilder
{
    private readonly Faker _faker = TestFaker.Create();

    private string _refreshToken;

    /// <summary>
    /// Initializes a new instance of the <see cref="PublicRefreshTokenRequestBuilder"/> class
    /// with a valid, non-empty random refresh token that satisfies the validator.
    /// </summary>
    public PublicRefreshTokenRequestBuilder()
    {
        _refreshToken = _faker.Random.AlphaNumeric(length: 64);
    }

    /// <summary>
    /// Sets the refresh token value.
    /// </summary>
    /// <param name="refreshToken">The refresh token to validate and rotate.</param>
    /// <returns>The builder instance for chaining.</returns>
    public PublicRefreshTokenRequestBuilder WithRefreshToken(string refreshToken)
    {
        _refreshToken = refreshToken;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="PublicRefreshTokenRequest"/> instance.
    /// </summary>
    /// <returns>A configured PublicRefreshTokenRequest instance.</returns>
    public PublicRefreshTokenRequest Build()
    {
        return new PublicRefreshTokenRequest(RefreshToken: _refreshToken);
    }
}
