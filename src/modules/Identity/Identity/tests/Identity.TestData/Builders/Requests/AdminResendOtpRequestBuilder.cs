using _116.Identity.Application.Auth.UseCases.Admin.Commands.ResendOtp.V1;
using _116.Identity.Domain.Enums;
using _116.Identity.TestData.Factories;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Identity.TestData.Mocks.Repositories;
using _116.Identity.TestData.Mocks.Services;
using _116.Tests.TestData.Helpers;
using _116.Tests.TestData.Mocks;
using Bogus;

namespace _116.Identity.TestData.Builders.Requests;

/// <summary>
/// Fluent builder for creating <see cref="AdminResendOtpRequest"/> instances in tests
/// with valid defaults that satisfy the resend-OTP validator (email format, valid OTP
/// purpose enum member).
/// </summary>
public class AdminResendOtpRequestBuilder
{
    private readonly Faker _faker = TestFaker.Create();

    private string _email;
    private string _purpose;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminResendOtpRequestBuilder"/> class
    /// with a valid random email and the email-verification OTP purpose.
    /// </summary>
    public AdminResendOtpRequestBuilder()
    {
        _email = _faker.Internet.Email();
        _purpose = nameof(EnumOtpPurpose.EmailVerification);
    }

    /// <summary>
    /// Sets the admin email address.
    /// </summary>
    /// <param name="email">The admin user's email address.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminResendOtpRequestBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminResendOtpRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminResendOtpRequest instance.</returns>
    public AdminResendOtpRequest Build()
    {
        return new AdminResendOtpRequest(Email: _email, Purpose: _purpose);
    }
}
