using _116.Identity.Application.Auth.UseCases.Public.Commands.ResetPassword.V1;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using Bogus;

namespace _116.Tests.Fixtures.Builders.Requests.Identity;

/// <summary>
/// Fluent builder for creating <see cref="PublicResetPasswordRequest"/> instances in tests
/// with valid defaults that satisfy the reset-password validator (email format, 6-digit
/// numeric OTP code, strong new password).
/// </summary>
public class PublicResetPasswordRequestBuilder
{
    private readonly Faker _faker = TestFaker.Create();

    private string _email;
    private string _code;
    private string _newPassword;

    /// <summary>
    /// Initializes a new instance of the <see cref="PublicResetPasswordRequestBuilder"/> class
    /// with a valid random email, a valid 6-digit OTP code, and a strong new password.
    /// </summary>
    public PublicResetPasswordRequestBuilder()
    {
        _email = _faker.Internet.Email();
        _code = TestConstants.Otp.ValidCode;
        _newPassword = TestConstants.Auth.ResetNewPassword;
    }

    /// <summary>
    /// Sets the user email address.
    /// </summary>
    /// <param name="email">The user's registered email address.</param>
    /// <returns>The builder instance for chaining.</returns>
    public PublicResetPasswordRequestBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    /// <summary>
    /// Sets the OTP code used for password reset.
    /// </summary>
    /// <param name="code">The OTP code received for password reset.</param>
    /// <returns>The builder instance for chaining.</returns>
    public PublicResetPasswordRequestBuilder WithCode(string code)
    {
        _code = code;
        return this;
    }

    /// <summary>
    /// Sets the new password to apply.
    /// </summary>
    /// <param name="newPassword">The new password to set for the user.</param>
    /// <returns>The builder instance for chaining.</returns>
    public PublicResetPasswordRequestBuilder WithNewPassword(string newPassword)
    {
        _newPassword = newPassword;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="PublicResetPasswordRequest"/> instance.
    /// </summary>
    /// <returns>A configured PublicResetPasswordRequest instance.</returns>
    public PublicResetPasswordRequest Build()
    {
        return new PublicResetPasswordRequest(Email: _email, Code: _code, NewPassword: _newPassword);
    }
}
