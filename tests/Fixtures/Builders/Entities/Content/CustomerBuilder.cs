using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Helpers;
using Bogus;

namespace _116.Tests.Fixtures.Builders.Entities.Content;

/// <summary>
/// Fluent builder for creating <see cref="CustomerEntity" /> instances in tests.
/// Drives the real domain transitions, so every state it produces is one the application can reach.
/// Use it for any shape a test needs; CustomerFactory only names chains three or more tests share.
/// </summary>
public class CustomerBuilder
{
    private readonly Faker _faker = TestFaker.Create();

    private Guid _id;
    private string _fullName;
    private string _email;
    private string? _company;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomerBuilder"/> class with random default values.
    /// </summary>
    public CustomerBuilder()
    {
        _id = Guid.NewGuid();
        _fullName = _faker.Name.FullName();
        _email = _faker.Internet.Email();
    }

    /// <summary>
    /// Sets the customer ID.
    /// </summary>
    public CustomerBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    /// <summary>
    /// Sets the customer full name.
    /// </summary>
    public CustomerBuilder WithFullName(string fullName)
    {
        _fullName = fullName;
        return this;
    }

    /// <summary>
    /// Sets the customer email.
    /// </summary>
    public CustomerBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    /// <summary>
    /// Sets the customer company.
    /// </summary>
    public CustomerBuilder WithCompany(string? company)
    {
        _company = company;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="CustomerEntity"/> instance.
    /// </summary>
    public CustomerEntity Build()
    {
        return CustomerEntity.Create(
            id: _id,
            fullName: _fullName,
            email: _email,
            phone: null,
            company: _company,
            notes: null,
            errors: TestErrorsFactory.CreateCustomerErrors()
        );
    }
}
