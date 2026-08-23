using _116.Identity.Application.Auth.UseCases.Public.Commands.SignUp.V1;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using Bogus;

namespace _116.Tests.Fixtures.Builders.Requests.Identity;

/// <summary>
/// Fluent builder for creating <see cref="PublicSignUpRequest"/> instances in tests
/// with valid defaults that satisfy the signup validator (email format, username length
/// and allowed characters, strong password).
/// </summary>
public class PublicSignUpRequestBuilder
{
    private readonly Faker _faker = TestFaker.Create();

    private string _email;
    private string _userName;
    private string _password;

    /// <summary>
    /// Initializes a new instance of the <see cref="PublicSignUpRequestBuilder"/> class with
    /// a unique valid email, an alphanumeric username within the allowed length range,
    /// and a strong password meeting complexity rules.
    /// </summary>
    public PublicSignUpRequestBuilder()
    {
        _email = $"s{Guid.NewGuid():N}@test.com";
        _userName = $"u{Guid.NewGuid():N}"[..10];
        _password = TestConstants.Auth.ValidPassword;
    }

    /// <summary>
    /// Sets the signup email address.
    /// </summary>
    /// <param name="email">The user's email address for account verification.</param>
    /// <returns>The builder instance for chaining.</returns>
    public PublicSignUpRequestBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    /// <summary>
    /// Sets the desired username.
    /// </summary>
    /// <param name="userName">The username (alphanumeric with spaces and hyphens allowed).</param>
    /// <returns>The builder instance for chaining.</returns>
    public PublicSignUpRequestBuilder WithUserName(string userName)
    {
        _userName = userName;
        return this;
    }

    /// <summary>
    /// Sets the signup password.
    /// </summary>
    /// <param name="password">The user's plain-text password (will be hashed).</param>
    /// <returns>The builder instance for chaining.</returns>
    public PublicSignUpRequestBuilder WithPassword(string password)
    {
        _password = password;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="PublicSignUpRequest"/> instance.
    /// </summary>
    /// <returns>A configured PublicSignUpRequest instance.</returns>
    public PublicSignUpRequest Build()
    {
        return new PublicSignUpRequest(Email: _email, UserName: _userName, Password: _password);
    }
}
