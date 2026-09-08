namespace _116.Shared.Application.Configurations;

/// <summary>
/// Reusable validators for environment variable declarations.
/// </summary>
public static class EnvVarValidators
{
    /// <summary>
    /// Requires the value to be at least the supplied number of characters.
    /// </summary>
    /// <param name="envVar">The descriptor being declared.</param>
    /// <param name="length">The minimum length.</param>
    /// <returns>The descriptor, for fluent declaration.</returns>
    public static EnvVar<string> MinLength(this EnvVar<string> envVar, int length)
    {
        return envVar.Rule(validator: value =>
            value.Length >= length ? null : $"must be at least {length} characters."
        );
    }

    /// <summary>
    /// Requires the value to fall within the supplied inclusive range.
    /// </summary>
    /// <param name="envVar">The descriptor being declared.</param>
    /// <param name="min">The inclusive lower bound.</param>
    /// <param name="max">The inclusive upper bound.</param>
    /// <returns>The descriptor, for fluent declaration.</returns>
    public static EnvVar<int> InRange(this EnvVar<int> envVar, int min, int max)
    {
        return envVar.Rule(validator: value =>
            value >= min && value <= max ? null : $"must be between {min} and {max}."
        );
    }

    /// <summary>
    /// Requires the value to be an absolute URL.
    /// </summary>
    /// <param name="envVar">The descriptor being declared.</param>
    /// <returns>The descriptor, for fluent declaration.</returns>
    public static EnvVar<string> AbsoluteUrl(this EnvVar<string> envVar)
    {
        return envVar.Rule(validator: value =>
            Uri.IsWellFormedUriString(value, UriKind.Absolute) ? null : "must be an absolute URL."
        );
    }

    /// <summary>
    /// Requires every entry of a comma-separated value to be an absolute URL, so origin
    /// variables accept either a single origin or a list.
    /// </summary>
    /// <param name="envVar">The descriptor being declared.</param>
    /// <returns>The descriptor, for fluent declaration.</returns>
    public static EnvVar<string> AbsoluteUrlList(this EnvVar<string> envVar)
    {
        return envVar.Rule(validator: value =>
            value
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .All(origin => Uri.IsWellFormedUriString(origin, UriKind.Absolute))
                ? null
                : "must be an absolute URL or a comma-separated list of absolute URLs."
        );
    }
}
