using _116.Identity.Application.Auth.UseCases.Public.Commands.SignOut.V1;
using _116.Identity.TestData.Factories;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Identity.TestData.Mocks.Repositories;
using _116.Identity.TestData.Mocks.Services;
using _116.Tests.TestData.Helpers;
using _116.Tests.TestData.Mocks;
using Bogus;

namespace _116.Identity.TestData.Builders.Requests;

/// <summary>
/// Fluent builder for creating <see cref="PublicSignOutRequest"/> instances in tests
/// with a non-empty default refresh token that satisfies the sign-out validator.
/// </summary>
public class PublicSignOutRequestBuilder
{
    private readonly Faker _faker = TestFaker.Create();

    private string? _refreshToken;

    /// <summary>
    /// Initializes a new instance of the <see cref="PublicSignOutRequestBuilder"/> class
    /// with a non-empty random refresh token.
    /// </summary>
    public PublicSignOutRequestBuilder()
    {
        _refreshToken = _faker.Random.AlphaNumeric(length: 64);
    }

    /// <summary>
    /// Sets the refresh token to revoke.
    /// </summary>
    /// <param name="refreshToken">The refresh token to revoke (mobile clients send it in the body).</param>
    /// <returns>The builder instance for chaining.</returns>
    public PublicSignOutRequestBuilder WithRefreshToken(string? refreshToken)
    {
        _refreshToken = refreshToken;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="PublicSignOutRequest"/> instance.
    /// </summary>
    /// <returns>A configured PublicSignOutRequest instance.</returns>
    public PublicSignOutRequest Build()
    {
        return new PublicSignOutRequest(RefreshToken: _refreshToken);
    }
}
