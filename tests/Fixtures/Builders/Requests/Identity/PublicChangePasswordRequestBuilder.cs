using _116.Identity.Application.Auth.UseCases.Public.Commands.ChangePassword.V1;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using Bogus;

namespace _116.Tests.Fixtures.Builders.Requests.Identity;

/// <summary>
/// Fluent builder for creating <see cref="PublicChangePasswordRequest"/> instances in tests
/// with valid defaults that satisfy the change-password validator (old password present,
/// strong new password).
/// </summary>
public class PublicChangePasswordRequestBuilder
{
    private readonly Faker _faker = TestFaker.Create();

    private string _oldPassword;
    private string _newPassword;

    /// <summary>
    /// Initializes a new instance of the <see cref="PublicChangePasswordRequestBuilder"/> class
    /// with a non-empty old password and a distinct strong new password.
    /// </summary>
    public PublicChangePasswordRequestBuilder()
    {
        _oldPassword = TestConstants.Auth.ValidPassword;
        _newPassword = TestConstants.Auth.ChangedPassword;
    }

    /// <summary>
    /// Sets the current password used for verification.
    /// </summary>
    /// <param name="oldPassword">The user's current password for verification.</param>
    /// <returns>The builder instance for chaining.</returns>
    public PublicChangePasswordRequestBuilder WithOldPassword(string oldPassword)
    {
        _oldPassword = oldPassword;
        return this;
    }

    /// <summary>
    /// Sets the new password to apply.
    /// </summary>
    /// <param name="newPassword">The new password to set for the user.</param>
    /// <returns>The builder instance for chaining.</returns>
    public PublicChangePasswordRequestBuilder WithNewPassword(string newPassword)
    {
        _newPassword = newPassword;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="PublicChangePasswordRequest"/> instance.
    /// </summary>
    /// <returns>A configured PublicChangePasswordRequest instance.</returns>
    public PublicChangePasswordRequest Build()
    {
        return new PublicChangePasswordRequest(OldPassword: _oldPassword, NewPassword: _newPassword);
    }
}
