using _116.Identity.Application.Auth.UseCases.Admin.Commands.Login.V1;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using Bogus;

namespace _116.Tests.Fixtures.Builders.Requests.Identity;

/// <summary>
/// Fluent builder for creating <see cref="AdminLoginRequest"/> instances in tests
/// with valid defaults that satisfy the admin login validator.
/// </summary>
public class AdminLoginRequestBuilder
{
    private readonly Faker _faker = TestFaker.Create();

    private string _email;
    private string _password;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminLoginRequestBuilder"/> class with
    /// a valid random email and a password that passes presence validation.
    /// </summary>
    public AdminLoginRequestBuilder()
    {
        _email = _faker.Internet.Email();
        _password = TestConstants.Auth.ValidPassword;
    }

    /// <summary>
    /// Sets the admin email address.
    /// </summary>
    /// <param name="email">The admin's email address.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminLoginRequestBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    /// <summary>
    /// Sets the admin password.
    /// </summary>
    /// <param name="password">The admin's password.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminLoginRequestBuilder WithPassword(string password)
    {
        _password = password;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminLoginRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminLoginRequest instance.</returns>
    public AdminLoginRequest Build()
    {
        return new AdminLoginRequest(Email: _email, Password: _password);
    }
}
