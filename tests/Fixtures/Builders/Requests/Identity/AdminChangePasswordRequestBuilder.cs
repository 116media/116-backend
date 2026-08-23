using _116.Identity.Application.Auth.UseCases.Admin.Commands.ChangePassword.V1;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using Bogus;

namespace _116.Tests.Fixtures.Builders.Requests.Identity;

/// <summary>
/// Fluent builder for creating <see cref="AdminChangePasswordRequest"/> instances in tests
/// with valid defaults that satisfy the change-password validator (old password present,
/// strong new password).
/// </summary>
public class AdminChangePasswordRequestBuilder
{
    private readonly Faker _faker = TestFaker.Create();

    private string _oldPassword;
    private string _newPassword;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminChangePasswordRequestBuilder"/> class
    /// with a non-empty old password and a distinct strong new password.
    /// </summary>
    public AdminChangePasswordRequestBuilder()
    {
        _oldPassword = TestConstants.Auth.ValidPassword;
        _newPassword = TestConstants.Auth.ChangedPassword;
    }

    /// <summary>
    /// Sets the current password used for verification.
    /// </summary>
    /// <param name="oldPassword">The admin user's current password for verification.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminChangePasswordRequestBuilder WithOldPassword(string oldPassword)
    {
        _oldPassword = oldPassword;
        return this;
    }

    /// <summary>
    /// Sets the new password to apply.
    /// </summary>
    /// <param name="newPassword">The new password to set for the admin user.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminChangePasswordRequestBuilder WithNewPassword(string newPassword)
    {
        _newPassword = newPassword;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminChangePasswordRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminChangePasswordRequest instance.</returns>
    public AdminChangePasswordRequest Build()
    {
        return new AdminChangePasswordRequest(OldPassword: _oldPassword, NewPassword: _newPassword);
    }
}
