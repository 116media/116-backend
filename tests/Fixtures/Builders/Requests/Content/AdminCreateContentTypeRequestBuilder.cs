using _116.Content.Application.Lookup.UseCases.Admin.Commands.CreateContentType.V1;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using Bogus;

namespace _116.Tests.Fixtures.Builders.Requests.Content;

/// <summary>
/// Fluent builder for creating <see cref="AdminCreateContentTypeRequest"/> instances in tests.
/// </summary>
public class AdminCreateContentTypeRequestBuilder
{
    private readonly Faker _faker = TestFaker.Create();

    private string _name;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminCreateContentTypeRequestBuilder"/> class
    /// with valid random default values that satisfy the create content type validator.
    /// </summary>
    public AdminCreateContentTypeRequestBuilder()
    {
        string candidate = $"Type{_faker.Random.AlphaNumeric(length: 8)}";
        _name = candidate[..Math.Min(TestConstants.ContentType.NameMaxLength, candidate.Length)];
    }

    /// <summary>
    /// Sets the content type name.
    /// </summary>
    /// <param name="name">The content type name.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminCreateContentTypeRequestBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminCreateContentTypeRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminCreateContentTypeRequest instance.</returns>
    public AdminCreateContentTypeRequest Build()
    {
        return new AdminCreateContentTypeRequest(Name: _name);
    }
}
