using _116.Content.Application.Lookup.UseCases.Admin.Commands.UpdateTag.V1;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using Bogus;

namespace _116.Tests.Fixtures.Builders.Requests.Content;

/// <summary>
/// Fluent builder for creating <see cref="AdminUpdateTagRequest"/> instances in tests.
/// </summary>
public class AdminUpdateTagRequestBuilder
{
    private readonly Faker _faker = TestFaker.Create();

    private string _name;
    private string _slug;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminUpdateTagRequestBuilder"/> class
    /// with valid random default values that satisfy the update tag validator.
    /// </summary>
    public AdminUpdateTagRequestBuilder()
    {
        string token = _faker.Random.AlphaNumeric(length: 8).ToLowerInvariant();
        string nameCandidate = $"Tag {token}";
        _name = nameCandidate[..Math.Min(TestConstants.Tag.NameMaxLength, nameCandidate.Length)];

        string slugCandidate = $"tag-{token}";
        _slug = slugCandidate[..Math.Min(TestConstants.Tag.SlugMaxLength, slugCandidate.Length)];
    }

    /// <summary>
    /// Sets the tag display name.
    /// </summary>
    /// <param name="name">The tag name.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminUpdateTagRequestBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    /// <summary>
    /// Sets the URL-safe tag slug.
    /// </summary>
    /// <param name="slug">The tag slug.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminUpdateTagRequestBuilder WithSlug(string slug)
    {
        _slug = slug;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminUpdateTagRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminUpdateTagRequest instance.</returns>
    public AdminUpdateTagRequest Build()
    {
        return new AdminUpdateTagRequest(Name: _name, Slug: _slug);
    }
}
