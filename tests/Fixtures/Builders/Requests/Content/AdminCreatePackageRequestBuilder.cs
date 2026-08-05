using _116.Content.Application.Catalog.UseCases.Admin.Commands.CreatePackage.V1;
using _116.Tests.Fixtures.Helpers;
using Bogus;

namespace _116.Tests.Fixtures.Builders.Requests.Content;

/// <summary>
/// Fluent builder for creating <see cref="AdminCreatePackageRequest"/> instances in tests.
/// Defaults produce values that satisfy <c>AdminCreatePackageValidator</c>.
/// </summary>
public class AdminCreatePackageRequestBuilder
{
    private readonly Faker _faker = TestFaker.Create();

    private string _name;
    private string _description;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminCreatePackageRequestBuilder"/> class
    /// with valid random values that satisfy the validator.
    /// </summary>
    public AdminCreatePackageRequestBuilder()
    {
        _name = _faker.Commerce.ProductName();
        _description = _faker.Lorem.Sentence(wordCount: 8);
    }

    /// <summary>
    /// Sets the display name of the package.
    /// </summary>
    /// <param name="name">The package name.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminCreatePackageRequestBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    /// <summary>
    /// Sets the description of what the package includes.
    /// </summary>
    /// <param name="description">The package description.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminCreatePackageRequestBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminCreatePackageRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminCreatePackageRequest instance.</returns>
    public AdminCreatePackageRequest Build()
    {
        return new AdminCreatePackageRequest(Name: _name, Description: _description);
    }
}
