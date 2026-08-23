using _116.Identity.Application.Auth.UseCases.Public.Commands.SetPassword.V1;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using Bogus;

namespace _116.Tests.Fixtures.Builders.Requests.Identity;

/// <summary>
/// Fluent builder for creating <see cref="PublicSetPasswordRequest"/> instances in tests
/// with a strong default password that satisfies the set-password validator.
/// </summary>
public class PublicSetPasswordRequestBuilder
{
    private readonly Faker _faker = TestFaker.Create();

    private string _password;

    /// <summary>
    /// Initializes a new instance of the <see cref="PublicSetPasswordRequestBuilder"/> class
    /// with a strong password meeting complexity rules.
    /// </summary>
    public PublicSetPasswordRequestBuilder()
    {
        _password = TestConstants.Auth.ValidPassword;
    }

    /// <summary>
    /// Sets the password to apply.
    /// </summary>
    /// <param name="password">The password to set for the user.</param>
    /// <returns>The builder instance for chaining.</returns>
    public PublicSetPasswordRequestBuilder WithPassword(string password)
    {
        _password = password;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="PublicSetPasswordRequest"/> instance.
    /// </summary>
    /// <returns>A configured PublicSetPasswordRequest instance.</returns>
    public PublicSetPasswordRequest Build()
    {
        return new PublicSetPasswordRequest(Password: _password);
    }
}
