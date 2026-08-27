namespace _116.Shared.Application.Configurations;

/// <summary>
/// Contract every declared environment variable satisfies, so boot validation can walk all
/// declarations without knowing their value types.
/// </summary>
public interface IEnvVar
{
    /// <summary>
    /// The environment variable name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Evaluates the variable, returning an error message or null when valid.
    /// </summary>
    string? Error();
}

/// <summary>
/// A single environment variable: name, parser, default and validators. Construction registers
/// the instance with <see cref="EnvSchema" />, so a declared variable can never be missed by
/// boot validation.
/// </summary>
/// <typeparam name="T">The parsed value type.</typeparam>
public sealed class EnvVar<T> : IEnvVar
{
    private readonly Func<string, (T Value, string? Error)> _parse;
    private readonly List<Func<T, string?>> _validators = [];
    private readonly bool _required;
    private readonly T _default;

    internal EnvVar(string name, bool required, T @default, Func<string, (T Value, string? Error)> parse)
    {
        Name = name;
        _required = required;
        _default = @default;
        _parse = parse;
        EnvSchema.Register(envVar: this);
    }

    /// <inheritdoc />
    public string Name { get; }

    /// <summary>
    /// The parsed value, read from the environment on every access so test hosts that swap
    /// variables between fixtures observe the change. Guaranteed valid and non-null for
    /// required variables once <see cref="EnvSchema.ValidateAtBoot" /> has passed.
    /// </summary>
    public T Value
    {
        get
        {
            string? raw = Environment.GetEnvironmentVariable(Name);
            if (string.IsNullOrWhiteSpace(raw))
            {
                return _default;
            }

            (T value, string? parseError) = _parse(raw);
            return parseError is null ? value : _default;
        }
    }

    /// <summary>
    /// Adds a validator returning an error message, or null when the value passes.
    /// </summary>
    /// <param name="validator">The validator to run against a present value.</param>
    /// <returns>This descriptor, for fluent declaration.</returns>
    public EnvVar<T> Rule(Func<T, string?> validator)
    {
        _validators.Add(validator);
        return this;
    }

    /// <inheritdoc />
    public string? Error()
    {
        string? raw = Environment.GetEnvironmentVariable(Name);

        if (string.IsNullOrWhiteSpace(raw))
        {
            return _required ? $"{Name} is missing or empty." : null;
        }

        (T value, string? parseError) = _parse(raw);
        if (parseError is not null)
        {
            return $"{Name} {parseError}";
        }

        string? ruleError = _validators
            .Select(validator => validator(value))
            .FirstOrDefault(error => error is not null);

        return ruleError is null ? null : $"{Name} {ruleError}";
    }
}

/// <summary>
/// Factory methods for the common environment variable shapes.
/// </summary>
public static class EnvVar
{
    /// <summary>
    /// Declares a string variable that must be present and non-empty at boot.
    /// </summary>
    /// <param name="name">The environment variable name.</param>
    /// <returns>The registered descriptor.</returns>
    public static EnvVar<string> Required(string name)
    {
        return new EnvVar<string>(name, required: true, @default: string.Empty, parse: raw => (raw, null));
    }

    /// <summary>
    /// Declares a string variable that may be absent, in which case the value is null.
    /// </summary>
    /// <param name="name">The environment variable name.</param>
    /// <returns>The registered descriptor.</returns>
    public static EnvVar<string?> Optional(string name)
    {
        return new EnvVar<string?>(name, required: false, @default: null, parse: raw => (raw, null));
    }

    /// <summary>
    /// Declares a string variable that may be absent, in which case the default is used.
    /// </summary>
    /// <param name="name">The environment variable name.</param>
    /// <param name="default">The value used when the variable is absent.</param>
    /// <returns>The registered descriptor.</returns>
    public static EnvVar<string> Optional(string name, string @default)
    {
        return new EnvVar<string>(name, required: false, @default: @default, parse: raw => (raw, null));
    }

    /// <summary>
    /// Declares an integer variable that may be absent, in which case the default is used.
    /// A present but unparsable value is a boot error.
    /// </summary>
    /// <param name="name">The environment variable name.</param>
    /// <param name="default">The value used when the variable is absent.</param>
    /// <returns>The registered descriptor.</returns>
    public static EnvVar<int> Int(string name, int @default)
    {
        return new EnvVar<int>(
            name,
            required: false,
            @default: @default,
            parse: raw => int.TryParse(s: raw, out int parsed) ? (parsed, null) : (0, "must be an integer.")
        );
    }
}
