using System.Text.RegularExpressions;
using _116.Content.Domain.Exceptions;
using _116.Content.Domain.StateMachines;

namespace _116.Content.Domain.ValueObjects;

/// <summary>
/// A URL-safe slug: lowercase alphanumeric words joined by single hyphens. Validates format
/// only — per-entity maximum lengths stay with the validators and the EF configurations.
/// </summary>
public partial record Slug
{
    /// <summary>
    /// Initializes a new <see cref="Slug" />, rejecting anything that is not a well-formed slug.
    /// </summary>
    /// <param name="value">The candidate slug.</param>
    /// <exception cref="ContentRuleException">Thrown when the value is not a well-formed slug.</exception>
    public Slug(string value)
    {
        if (string.IsNullOrWhiteSpace(value: value) || !SlugRegex().IsMatch(value))
        {
            throw new ContentRuleException(ContentRuleCodes.InvalidSlug, value ?? string.Empty);
        }

        Value = value;
    }

    /// <summary>
    /// The validated slug.
    /// </summary>
    public string Value { get; init; }

    /// <summary>
    /// Parses a candidate that may not be a slug at all, returning null instead of throwing.
    /// </summary>
    /// <param name="value">The candidate slug, possibly absent or malformed.</param>
    /// <returns>The parsed slug, or null when the candidate is not well-formed.</returns>
    public static Slug? TryFrom(string? value)
    {
        return string.IsNullOrWhiteSpace(value: value) || !SlugRegex().IsMatch(value) ? null : new Slug(value);
    }

    /// <summary>
    /// Reports whether a candidate is a well-formed slug, for validators that surface a
    /// field-level message rather than a domain rule failure.
    /// </summary>
    /// <param name="value">The candidate slug.</param>
    /// <returns><c>true</c> when the value is well-formed.</returns>
    public static bool IsWellFormed(string? value)
    {
        return !string.IsNullOrWhiteSpace(value: value) && SlugRegex().IsMatch(value);
    }

    /// <summary>
    /// Returns the underlying slug.
    /// </summary>
    /// <returns>The slug value.</returns>
    public override string ToString()
    {
        return Value;
    }

    /// <summary>
    /// Implicitly converts a <see cref="Slug" /> to a <see cref="string" />.
    /// </summary>
    /// <param name="slug">The slug instance.</param>
    public static implicit operator string(Slug slug)
    {
        return slug.Value;
    }

    /// <summary>
    /// Implicitly converts a <see cref="string" /> to a <see cref="Slug" />.
    /// </summary>
    /// <param name="value">The slug string to convert.</param>
    /// <exception cref="ContentRuleException">Thrown when the value is not a well-formed slug.</exception>
    public static implicit operator Slug(string value)
    {
        return new Slug(value: value);
    }

    /// <summary>
    /// Lowercase alphanumeric words joined by single hyphens, with no leading or trailing hyphen.
    /// </summary>
    [GeneratedRegex(@"^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    private static partial Regex SlugRegex();
}
