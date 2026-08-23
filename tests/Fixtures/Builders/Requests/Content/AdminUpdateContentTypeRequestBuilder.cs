using _116.Content.Application.Lookup.UseCases.Admin.Commands.UpdateContentType.V1;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using Bogus;

namespace _116.Tests.Fixtures.Builders.Requests.Content;

/// <summary>
/// Fluent builder for creating <see cref="AdminUpdateContentTypeRequest"/> instances in tests.
/// </summary>
public class AdminUpdateContentTypeRequestBuilder
{
    private readonly Faker _faker = TestFaker.Create();

    private string _name;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminUpdateContentTypeRequestBuilder"/> class
    /// with valid random default values that satisfy the update content type validator.
    /// </summary>
    public AdminUpdateContentTypeRequestBuilder()
    {
        string candidate = $"Type{_faker.Random.AlphaNumeric(length: 8)}";
        _name = candidate[..Math.Min(TestConstants.ContentType.NameMaxLength, candidate.Length)];
    }

    /// <summary>
    /// Sets the content type name.
    /// </summary>
    /// <param name="name">The content type name.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminUpdateContentTypeRequestBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminUpdateContentTypeRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminUpdateContentTypeRequest instance.</returns>
    public AdminUpdateContentTypeRequest Build()
    {
        return new AdminUpdateContentTypeRequest(Name: _name);
    }
}
