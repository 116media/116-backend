using _116.Tests.Fixtures.Builders.Helpers;

namespace _116.Tests.Fixtures.Factories.Helpers;

/// <summary>
/// Factory for quickly creating raw input strings used to test slug generation.
/// </summary>
public static class SlugInputFactory
{
    /// <summary>
    /// Creates a random multi-word phrase input.
    /// </summary>
    /// <returns>
    /// A random multi-word input string.
    /// </returns>
    public static string Create() => new SlugInputBuilder().Build();

    /// <summary>
    /// Creates a fixed display name input.
    /// </summary>
    /// <param name="name">The display name.</param>
    /// <returns>
    /// The provided display name.
    /// </returns>
    public static string Create(string name) => new SlugInputBuilder().WithName(name).Build();

    /// <summary>
    /// Creates a random input with leading and trailing whitespace.
    /// </summary>
    /// <returns>
    /// A random input string padded with spaces.
    /// </returns>
    public static string CreateWithSpaces() => new SlugInputBuilder().WithLeadingAndTrailingSpaces().Build();

    /// <summary>
    /// Creates a random all-uppercase input.
    /// </summary>
    /// <returns>
    /// A random uppercase input string.
    /// </returns>
    public static string CreateUppercase() => new SlugInputBuilder().AsUppercase().Build();

    /// <summary>
    /// Creates a random input containing underscores.
    /// </summary>
    /// <returns>
    /// A random input string with underscores.
    /// </returns>
    public static string CreateWithUnderscores() => new SlugInputBuilder().WithUnderscores().Build();

    /// <summary>
    /// Creates a random input with multiple consecutive spaces between words.
    /// </summary>
    /// <returns>
    /// A random input string with multiple spaces.
    /// </returns>
    public static string CreateWithMultipleSpaces() => new SlugInputBuilder().WithMultipleConsecutiveSpaces().Build();

    /// <summary>
    /// Creates a random input containing special characters.
    /// </summary>
    /// <returns>
    /// A random input string with special characters.
    /// </returns>
    public static string CreateWithSpecialCharacters() => new SlugInputBuilder().WithSpecialCharacters().Build();
}
