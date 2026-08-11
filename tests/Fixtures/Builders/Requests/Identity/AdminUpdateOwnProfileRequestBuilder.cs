using _116.Identity.Application.User.UseCases.Admin.Commands.UpdateOwnProfile.V1;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using Bogus;

namespace _116.Tests.Fixtures.Builders.Requests.Identity;

/// <summary>
/// Fluent builder for creating <see cref="AdminUpdateOwnProfileRequest"/> instances in tests.
/// </summary>
/// <remarks>
/// Country fields only persist alongside a phone-number update, so the valid default
/// produces a coherent set (username, partial phone number, country name, ISO code and
/// dial code together) that exercises the country update path and satisfies the validator.
/// </remarks>
public class AdminUpdateOwnProfileRequestBuilder
{
    private readonly Faker _faker = TestFaker.Create();

    private string? _userName;
    private string? _countryName;
    private string? _partialPhoneNumber;
    private string? _countryIsoCode;
    private string? _countryDialCode;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminUpdateOwnProfileRequestBuilder"/> class
    /// with a coherent set of valid random profile values that satisfy the validator.
    /// </summary>
    public AdminUpdateOwnProfileRequestBuilder()
    {
        _userName = _faker.Random.AlphaNumeric(length: TestConstants.User.UserNameMinLength + 5);
        _countryName = TestConstants.User.ValidCountry;
        _partialPhoneNumber = _faker.Random.ReplaceNumbers("##########");
        _countryIsoCode = "US";
        _countryDialCode = "+1";
    }

    /// <summary>
    /// Builds the <see cref="AdminUpdateOwnProfileRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminUpdateOwnProfileRequest instance.</returns>
    public AdminUpdateOwnProfileRequest Build()
    {
        return new AdminUpdateOwnProfileRequest(
            UserName: _userName,
            CountryName: _countryName,
            PartialPhoneNumber: _partialPhoneNumber,
            CountryIsoCode: _countryIsoCode,
            CountryDialCode: _countryDialCode
        );
    }
}
