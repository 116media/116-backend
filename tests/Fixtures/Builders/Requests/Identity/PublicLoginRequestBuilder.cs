using _116.Identity.Application.Auth.UseCases.Public.Commands.Login.V1;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using Bogus;

namespace _116.Tests.Fixtures.Builders.Requests.Identity;

/// <summary>
/// Fluent builder for creating <see cref="PublicLoginRequest"/> instances in tests
/// with valid defaults that satisfy the public login validator.
/// </summary>
public class PublicLoginRequestBuilder
{
    private readonly Faker _faker = TestFaker.Create();

    private string _credentials;
    private string _password;

    /// <summary>
    /// Initializes a new instance of the <see cref="PublicLoginRequestBuilder"/> class with
    /// valid random credentials (an email) and a password that passes presence validation.
    /// </summary>
    public PublicLoginRequestBuilder()
    {
        _credentials = _faker.Internet.Email();
        _password = TestConstants.Auth.ValidPassword;
    }

    /// <summary>
    /// Sets the login credentials (email or username).
    /// </summary>
    /// <param name="credentials">The user's email or username.</param>
    /// <returns>The builder instance for chaining.</returns>
    public PublicLoginRequestBuilder WithCredentials(string credentials)
    {
        _credentials = credentials;
        return this;
    }

    /// <summary>
    /// Sets the user password.
    /// </summary>
    /// <param name="password">The user's password.</param>
    /// <returns>The builder instance for chaining.</returns>
    public PublicLoginRequestBuilder WithPassword(string password)
    {
        _password = password;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="PublicLoginRequest"/> instance.
    /// </summary>
    /// <returns>A configured PublicLoginRequest instance.</returns>
    public PublicLoginRequest Build()
    {
        return new PublicLoginRequest(Credentials: _credentials, Password: _password);
    }
}
