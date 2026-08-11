using _116.Content.Application.Catalog.UseCases.Admin.Commands.UpdateCustomer.V1;
using _116.Tests.Fixtures.Helpers;
using Bogus;

namespace _116.Tests.Fixtures.Builders.Requests.Content;

/// <summary>
/// Fluent builder for creating <see cref="AdminUpdateCustomerRequest"/> instances in tests.
/// Defaults produce values that satisfy <c>AdminUpdateCustomerValidator</c>.
/// </summary>
public class AdminUpdateCustomerRequestBuilder
{
    private readonly Faker _faker = TestFaker.Create();

    private string _fullName;
    private string _email;
    private string? _phone;
    private string? _company;
    private string? _notes;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminUpdateCustomerRequestBuilder"/> class
    /// with valid random values that satisfy the validator.
    /// </summary>
    public AdminUpdateCustomerRequestBuilder()
    {
        _fullName = _faker.Name.FullName();
        _email = _faker.Internet.Email();
        _phone = _faker.Phone.PhoneNumber("+243#########");
        _company = _faker.Company.CompanyName();
        _notes = _faker.Lorem.Sentence(wordCount: 6);
    }

    /// <summary>
    /// Sets the new full name of the customer.
    /// </summary>
    /// <param name="fullName">The customer full name.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminUpdateCustomerRequestBuilder WithFullName(string fullName)
    {
        _fullName = fullName;
        return this;
    }

    /// <summary>
    /// Sets the new email address of the customer.
    /// </summary>
    /// <param name="email">The customer email address.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminUpdateCustomerRequestBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminUpdateCustomerRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminUpdateCustomerRequest instance.</returns>
    public AdminUpdateCustomerRequest Build()
    {
        return new AdminUpdateCustomerRequest(
            FullName: _fullName,
            Email: _email,
            Phone: _phone,
            Company: _company,
            Notes: _notes
        );
    }
}
