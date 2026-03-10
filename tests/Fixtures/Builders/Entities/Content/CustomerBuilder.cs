using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Constants;
using Bogus;

namespace _116.Tests.Fixtures.Builders.Entities.Content;

/// <summary>
/// Fluent builder for creating <see cref="CustomerEntity"/> instances in tests.
/// For test code, prefer using CustomerFactory instead of direct Builder usage.
/// </summary>
internal class CustomerBuilder
{
    private readonly Faker _faker = new();

    private Guid _id;
    private string _fullName;
    private string _email;
    private string? _phone;
    private string? _company;
    private string? _notes;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomerBuilder"/> class with random default values.
    /// </summary>
    public CustomerBuilder()
    {
        _id = Guid.NewGuid();
        _fullName = _faker.Name.FullName();
        _email = _faker.Internet.Email();
    }

    /// <summary>Sets the customer ID.</summary>
    public CustomerBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    /// <summary>Sets the customer full name.</summary>
    public CustomerBuilder WithFullName(string fullName)
    {
        _fullName = fullName;
        return this;
    }

    /// <summary>Sets the customer email.</summary>
    public CustomerBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    /// <summary>Sets the customer phone.</summary>
    public CustomerBuilder WithPhone(string? phone)
    {
        _phone = phone;
        return this;
    }

    /// <summary>Sets the customer company.</summary>
    public CustomerBuilder WithCompany(string? company)
    {
        _company = company;
        return this;
    }

    /// <summary>Sets the customer notes.</summary>
    public CustomerBuilder WithNotes(string? notes)
    {
        _notes = notes;
        return this;
    }

    /// <summary>Builds the <see cref="CustomerEntity"/> instance.</summary>
    public CustomerEntity Build()
    {
        return CustomerEntity.Create(_id, _fullName, _email, _phone, _company, _notes);
    }
}
