using _116.Identity.Application.Auth.UseCases.Admin.Commands.VerifyOtp.V1;
using _116.Identity.Domain.Enums;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using Bogus;

namespace _116.Tests.Fixtures.Builders.Requests.Identity;

/// <summary>
/// Fluent builder for creating <see cref="AdminVerifyOtpRequest"/> instances in tests
/// with valid defaults that satisfy the verify-OTP validator (email format, 6-digit numeric
/// code, valid OTP purpose enum member).
/// </summary>
public class AdminVerifyOtpRequestBuilder
{
    private readonly Faker _faker = TestFaker.Create();

    private string _email;
    private string _code;
    private string _purpose;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminVerifyOtpRequestBuilder"/> class
    /// with a valid random email, a valid 6-digit OTP code, and the email-verification purpose.
    /// </summary>
    public AdminVerifyOtpRequestBuilder()
    {
        _email = _faker.Internet.Email();
        _code = TestConstants.Otp.ValidCode;
        _purpose = nameof(EnumOtpPurpose.EmailVerification);
    }

    /// <summary>
    /// Sets the admin email address.
    /// </summary>
    /// <param name="email">The admin user's email address.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminVerifyOtpRequestBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    /// <summary>
    /// Sets the OTP code to verify.
    /// </summary>
    /// <param name="code">The OTP code to verify.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminVerifyOtpRequestBuilder WithCode(string code)
    {
        _code = code;
        return this;
    }

    /// <summary>
    /// Sets the OTP purpose.
    /// </summary>
    /// <param name="purpose">The purpose for which the OTP is being verified.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminVerifyOtpRequestBuilder WithPurpose(string purpose)
    {
        _purpose = purpose;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminVerifyOtpRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminVerifyOtpRequest instance.</returns>
    public AdminVerifyOtpRequest Build()
    {
        return new AdminVerifyOtpRequest(Email: _email, Code: _code, Purpose: _purpose);
    }
}
