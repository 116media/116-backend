using _116.Tests.Fixtures.Helpers;
using Bogus;

namespace _116.Tests.Fixtures.Builders.Helpers;

/// <summary>
/// Fluent builder for constructing raw input strings used to test slug generation.
/// Uses Bogus to generate realistic random inputs where exact output is unpredictable.
/// </summary>
public class SlugInputBuilder
{
    private readonly Faker _faker = TestFaker.Create();
    private string _value;

    /// <summary>
    /// Initializes a new instance of the <see cref="SlugInputBuilder"/> class
    /// with a random multi-word phrase as the default input.
    /// </summary>
    public SlugInputBuilder()
    {
        _value = _faker.Lorem.Sentence(wordCount: 2).TrimEnd('.');
    }

    /// <summary>
    /// Sets a fixed display name as input.
    /// </summary>
    /// <param name="name">The display name.</param>
    /// <returns>
    /// The builder instance for chaining.
    /// </returns>
    public SlugInputBuilder WithName(string name)
    {
        _value = name;
        return this;
    }

    /// <summary>
    /// Sets the input to a randomly generated all-uppercase word.
    /// </summary>
    /// <returns>
    /// The builder instance for chaining.
    /// </returns>
    public SlugInputBuilder AsUppercase()
    {
        _value = _faker.Lorem.Word().ToUpperInvariant();
        return this;
    }

    /// <summary>
    /// Sets the input to a randomly generated name containing underscores.
    /// </summary>
    /// <returns>
    /// The builder instance for chaining.
    /// </returns>
    public SlugInputBuilder WithUnderscores()
    {
        _value = $"{_faker.Lorem.Word()}_{_faker.Lorem.Word()}";
        return this;
    }

    /// <summary>
    /// Sets the input to a randomly generated name with multiple consecutive spaces.
    /// </summary>
    /// <returns>
    /// The builder instance for chaining.
    /// </returns>
    public SlugInputBuilder WithMultipleConsecutiveSpaces()
    {
        _value = $"{_faker.Lorem.Word()}   {_faker.Lorem.Word()}";
        return this;
    }

    /// <summary>
    /// Sets the input to a randomly generated name with special characters appended.
    /// </summary>
    /// <returns>
    /// The builder instance for chaining.
    /// </returns>
    public SlugInputBuilder WithSpecialCharacters()
    {
        _value = $"{_faker.Lorem.Word()}! #{_faker.Lorem.Word()}@";
        return this;
    }

    /// <summary>
    /// Builds and returns the raw input string.
    /// </summary>
    /// <returns>
    /// The configured input string.
    /// </returns>
    public string Build() => _value;
}
