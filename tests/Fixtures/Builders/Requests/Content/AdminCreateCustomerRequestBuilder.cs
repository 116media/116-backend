using _116.Content.Application.Catalog.UseCases.Admin.Commands.CreateCustomer.V1;
using _116.Tests.Fixtures.Helpers;
using Bogus;

namespace _116.Tests.Fixtures.Builders.Requests.Content;

/// <summary>
/// Fluent builder for creating <see cref="AdminCreateCustomerRequest"/> instances in tests.
/// Defaults produce values that satisfy <c>AdminCreateCustomerValidator</c>.
/// </summary>
public class AdminCreateCustomerRequestBuilder
{
    private readonly Faker _faker = TestFaker.Create();

    private string _fullName;
    private string _email;
    private string? _phone;
    private string? _company;
    private string? _notes;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminCreateCustomerRequestBuilder"/> class
    /// with valid random values that satisfy the validator.
    /// </summary>
    public AdminCreateCustomerRequestBuilder()
    {
        _fullName = _faker.Name.FullName();
        _email = _faker.Internet.Email();
        _phone = _faker.Phone.PhoneNumber("+243#########");
        _company = _faker.Company.CompanyName();
        _notes = _faker.Lorem.Sentence(wordCount: 6);
    }

    /// <summary>
    /// Sets the full name of the customer or contact person.
    /// </summary>
    /// <param name="fullName">The customer full name.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminCreateCustomerRequestBuilder WithFullName(string fullName)
    {
        _fullName = fullName;
        return this;
    }

    /// <summary>
    /// Sets the email address of the customer.
    /// </summary>
    /// <param name="email">The customer email address.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminCreateCustomerRequestBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminCreateCustomerRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminCreateCustomerRequest instance.</returns>
    public AdminCreateCustomerRequest Build()
    {
        return new AdminCreateCustomerRequest(
            FullName: _fullName,
            Email: _email,
            Phone: _phone,
            Company: _company,
            Notes: _notes
        );
    }
}
