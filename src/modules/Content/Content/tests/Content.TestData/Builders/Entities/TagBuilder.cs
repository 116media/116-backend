using _116.Content.Domain.Entities;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Factories.Helpers;
using _116.Content.TestData.Mocks.Factories;
using _116.Content.TestData.Mocks.Infrastructure;
using _116.Content.TestData.Mocks.Repositories;
using _116.Content.TestData.Mocks.Services;
using _116.Tests.TestData.Constants;
using _116.Tests.TestData.Helpers;
using _116.Tests.TestData.Mocks;
using Bogus;

namespace _116.Content.TestData.Builders.Entities;

/// <summary>
/// Fluent builder for creating <see cref="TagEntity" /> instances in tests.
/// Drives the real domain transitions, so every state it produces is one the application can reach.
/// Use it for any shape a test needs; TagFactory only names chains three or more tests share.
/// </summary>
public class TagBuilder
{
    private readonly Faker _faker = TestFaker.Create();

    private Guid _id;
    private string _name;
    private string _slug;

    /// <summary>
    /// Initializes a new instance of the <see cref="TagBuilder"/> class with random default values.
    /// </summary>
    public TagBuilder()
    {
        _id = Guid.NewGuid();
        string suffix = Guid.NewGuid().ToString("N")[..8];
        string word = _faker.Lorem.Word().ToLower();
        string unique = $"{word}{suffix}";
        _name = unique[..Math.Min(TestConstants.Tag.NameMaxLength, unique.Length)];
        _slug = unique[..Math.Min(TestConstants.Tag.SlugMaxLength, unique.Length)];
    }

    /// <summary>
    /// Sets the tag name.
    /// </summary>
    public TagBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    /// <summary>
    /// Sets the tag slug.
    /// </summary>
    public TagBuilder WithSlug(string slug)
    {
        _slug = slug;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="TagEntity"/> instance.
    /// </summary>
    public TagEntity Build()
    {
        return TagEntity.Create(_id, _name, _slug);
    }
}
