using _116.Identity.Application.Auth.UseCases.Public.Commands.ForgotPassword.V1;
using _116.Tests.Fixtures.Helpers;
using Bogus;

namespace _116.Tests.Fixtures.Builders.Requests.Identity;

/// <summary>
/// Fluent builder for creating <see cref="PublicForgotPasswordRequest"/> instances in tests
/// with a valid default email that satisfies the forgot-password validator.
/// </summary>
public class PublicForgotPasswordRequestBuilder
{
    private readonly Faker _faker = TestFaker.Create();

    private string _email;

    /// <summary>
    /// Initializes a new instance of the <see cref="PublicForgotPasswordRequestBuilder"/> class
    /// with a valid random email address.
    /// </summary>
    public PublicForgotPasswordRequestBuilder()
    {
        _email = _faker.Internet.Email();
    }

    /// <summary>
    /// Sets the user email address.
    /// </summary>
    /// <param name="email">The user's registered email address.</param>
    /// <returns>The builder instance for chaining.</returns>
    public PublicForgotPasswordRequestBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="PublicForgotPasswordRequest"/> instance.
    /// </summary>
    /// <returns>A configured PublicForgotPasswordRequest instance.</returns>
    public PublicForgotPasswordRequest Build()
    {
        return new PublicForgotPasswordRequest(Email: _email);
    }
}
