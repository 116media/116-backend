using _116.Identity.Application.Auth.UseCases.Public.Commands.ResendOtp.V1;
using _116.Identity.Domain.Enums;
using _116.Tests.Fixtures.Helpers;
using Bogus;

namespace _116.Tests.Fixtures.Builders.Requests.Identity;

/// <summary>
/// Fluent builder for creating <see cref="PublicResendOtpRequest"/> instances in tests
/// with valid defaults that satisfy the resend-OTP validator (email format, valid OTP
/// purpose enum member).
/// </summary>
public class PublicResendOtpRequestBuilder
{
    private readonly Faker _faker = TestFaker.Create();

    private string _email;
    private string _purpose;

    /// <summary>
    /// Initializes a new instance of the <see cref="PublicResendOtpRequestBuilder"/> class
    /// with a valid random email and the email-verification OTP purpose.
    /// </summary>
    public PublicResendOtpRequestBuilder()
    {
        _email = _faker.Internet.Email();
        _purpose = nameof(EnumOtpPurpose.EmailVerification);
    }

    /// <summary>
    /// Sets the user email address.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <returns>The builder instance for chaining.</returns>
    public PublicResendOtpRequestBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    /// <summary>
    /// Sets the OTP purpose.
    /// </summary>
    /// <param name="purpose">The purpose for which the OTP is being resent.</param>
    /// <returns>The builder instance for chaining.</returns>
    public PublicResendOtpRequestBuilder WithPurpose(string purpose)
    {
        _purpose = purpose;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="PublicResendOtpRequest"/> instance.
    /// </summary>
    /// <returns>A configured PublicResendOtpRequest instance.</returns>
    public PublicResendOtpRequest Build()
    {
        return new PublicResendOtpRequest(Email: _email, Purpose: _purpose);
    }
}
