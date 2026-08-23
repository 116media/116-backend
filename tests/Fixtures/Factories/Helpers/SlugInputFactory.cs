using _116.Tests.Fixtures.Builders.Helpers;

namespace _116.Tests.Fixtures.Factories.Helpers;

/// <summary>
/// Named aliases for <see cref="SlugInputBuilder" /> chains that three or more tests share
/// verbatim. A shape fewer tests need belongs at the call site as a builder chain, not here —
/// factory names carry the combinatorics, and combinatorics multiply.
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
