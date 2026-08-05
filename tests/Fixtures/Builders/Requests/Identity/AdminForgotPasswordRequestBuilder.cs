using _116.Identity.Application.Auth.UseCases.Admin.Commands.ForgotPassword.V1;
using _116.Tests.Fixtures.Helpers;
using Bogus;

namespace _116.Tests.Fixtures.Builders.Requests.Identity;

/// <summary>
/// Fluent builder for creating <see cref="AdminForgotPasswordRequest"/> instances in tests
/// with a valid default email that satisfies the forgot-password validator.
/// </summary>
public class AdminForgotPasswordRequestBuilder
{
    private readonly Faker _faker = TestFaker.Create();

    private string _email;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminForgotPasswordRequestBuilder"/> class
    /// with a valid random email address.
    /// </summary>
    public AdminForgotPasswordRequestBuilder()
    {
        _email = _faker.Internet.Email();
    }

    /// <summary>
    /// Sets the admin email address.
    /// </summary>
    /// <param name="email">The admin user's registered email address.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminForgotPasswordRequestBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminForgotPasswordRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminForgotPasswordRequest instance.</returns>
    public AdminForgotPasswordRequest Build()
    {
        return new AdminForgotPasswordRequest(Email: _email);
    }
}
