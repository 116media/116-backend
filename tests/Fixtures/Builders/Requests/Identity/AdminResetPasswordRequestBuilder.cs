using _116.Identity.Application.Auth.UseCases.Admin.Commands.ResetPassword.V1;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using Bogus;

namespace _116.Tests.Fixtures.Builders.Requests.Identity;

/// <summary>
/// Fluent builder for creating <see cref="AdminResetPasswordRequest"/> instances in tests
/// with valid defaults that satisfy the reset-password validator (email format, 6-digit
/// numeric OTP code, strong new password).
/// </summary>
public class AdminResetPasswordRequestBuilder
{
    private readonly Faker _faker = TestFaker.Create();

    private string _email;
    private string _code;
    private string _newPassword;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminResetPasswordRequestBuilder"/> class
    /// with a valid random email, a valid 6-digit OTP code, and a strong new password.
    /// </summary>
    public AdminResetPasswordRequestBuilder()
    {
        _email = _faker.Internet.Email();
        _code = TestConstants.Otp.ValidCode;
        _newPassword = TestConstants.Auth.ResetNewPassword;
    }

    /// <summary>
    /// Sets the admin email address.
    /// </summary>
    /// <param name="email">The admin user's registered email address.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminResetPasswordRequestBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    /// <summary>
    /// Sets the OTP code used for password reset.
    /// </summary>
    /// <param name="code">The OTP code received for password reset.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminResetPasswordRequestBuilder WithCode(string code)
    {
        _code = code;
        return this;
    }

    /// <summary>
    /// Sets the new password to apply.
    /// </summary>
    /// <param name="newPassword">The new password to set for the admin user.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminResetPasswordRequestBuilder WithNewPassword(string newPassword)
    {
        _newPassword = newPassword;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminResetPasswordRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminResetPasswordRequest instance.</returns>
    public AdminResetPasswordRequest Build()
    {
        return new AdminResetPasswordRequest(Email: _email, Code: _code, NewPassword: _newPassword);
    }
}
