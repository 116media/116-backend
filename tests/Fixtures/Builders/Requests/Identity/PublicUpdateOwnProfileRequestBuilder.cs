using _116.Identity.Application.User.UseCases.Public.Commands.UpdateOwnProfile.V1;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using Bogus;

namespace _116.Tests.Fixtures.Builders.Requests.Identity;

/// <summary>
/// Fluent builder for creating <see cref="PublicUpdateOwnProfileRequest"/> instances in tests.
/// </summary>
/// <remarks>
/// Country fields only persist alongside a phone-number update, so the valid default
/// produces a coherent set (username, partial phone number, country name, ISO code and
/// dial code together) that exercises the country update path and satisfies the validator.
/// Email defaults to null so the happy path does not trigger re-verification side effects.
/// </remarks>
public class PublicUpdateOwnProfileRequestBuilder
{
    private readonly Faker _faker = TestFaker.Create();

    private string? _email;
    private string? _userName;
    private string? _countryName;
    private string? _partialPhoneNumber;
    private string? _countryIsoCode;
    private string? _countryDialCode;

    /// <summary>
    /// Initializes a new instance of the <see cref="PublicUpdateOwnProfileRequestBuilder"/> class
    /// with a coherent set of valid random profile values that satisfy the validator.
    /// </summary>
    public PublicUpdateOwnProfileRequestBuilder()
    {
        _email = null;
        _userName = _faker.Random.AlphaNumeric(length: TestConstants.User.UserNameMinLength + 5);
        _countryName = TestConstants.User.ValidCountry;
        _partialPhoneNumber = _faker.Random.ReplaceNumbers("##########");
        _countryIsoCode = "US";
        _countryDialCode = "+1";
    }

    /// <summary>
    /// Sets the email address to update.
    /// </summary>
    /// <param name="email">The new email address.</param>
    /// <returns>The builder instance for chaining.</returns>
    public PublicUpdateOwnProfileRequestBuilder WithEmail(string? email)
    {
        _email = email;
        return this;
    }

    /// <summary>
    /// Sets the username to update.
    /// </summary>
    /// <param name="userName">The new username.</param>
    /// <returns>The builder instance for chaining.</returns>
    public PublicUpdateOwnProfileRequestBuilder WithUserName(string? userName)
    {
        _userName = userName;
        return this;
    }

    /// <summary>
    /// Sets the partial phone number to update.
    /// </summary>
    /// <param name="partialPhoneNumber">The new partial phone number.</param>
    /// <returns>The builder instance for chaining.</returns>
    public PublicUpdateOwnProfileRequestBuilder WithPartialPhoneNumber(string? partialPhoneNumber)
    {
        _partialPhoneNumber = partialPhoneNumber;
        return this;
    }

    /// <summary>
    /// Sets the country dial code to update.
    /// </summary>
    /// <param name="countryDialCode">The new country dial code.</param>
    /// <returns>The builder instance for chaining.</returns>
    public PublicUpdateOwnProfileRequestBuilder WithCountryDialCode(string? countryDialCode)
    {
        _countryDialCode = countryDialCode;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="PublicUpdateOwnProfileRequest"/> instance.
    /// </summary>
    /// <returns>A configured PublicUpdateOwnProfileRequest instance.</returns>
    public PublicUpdateOwnProfileRequest Build()
    {
        return new PublicUpdateOwnProfileRequest(
            Email: _email,
            UserName: _userName,
            CountryName: _countryName,
            PartialPhoneNumber: _partialPhoneNumber,
            CountryIsoCode: _countryIsoCode,
            CountryDialCode: _countryDialCode
        );
    }
}
