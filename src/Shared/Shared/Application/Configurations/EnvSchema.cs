using System.Runtime.CompilerServices;
using _116.Shared.Application.Configurations.Schemas;

namespace _116.Shared.Application.Configurations;

/// <summary>
/// Registry of every declared environment variable. Validation walks all declarations and
/// reports every failure at once, so a misconfigured instance refuses to boot with the complete
/// list instead of failing variable by variable.
/// </summary>
public static class EnvSchema
{
    private static readonly List<IEnvVar> Declared = [];

    /// <summary>
    /// Records a declared variable; called by every <see cref="EnvVar{T}" /> constructor.
    /// </summary>
    /// <param name="envVar">The descriptor being declared.</param>
    internal static void Register(IEnvVar envVar)
    {
        Declared.Add(envVar);
    }

    /// <summary>
    /// Forces every schema class to declare its variables, then validates all of them.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown listing every invalid variable.</exception>
    public static void ValidateAtBoot()
    {
        RuntimeHelpers.RunClassConstructor(typeof(DatabaseEnv).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(JwtEnv).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(CloudinaryEnv).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(SocialAuthEnv).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(WebEnv).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(MailEnv).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(SecurityEnv).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(ObservabilityEnv).TypeHandle);

        List<string> errors = [.. Declared.Select(envVar => envVar.Error()).OfType<string>()];

        if (errors.Count > 0)
        {
            throw new InvalidOperationException(
                $"Invalid environment configuration:{Environment.NewLine}  "
                    + string.Join($"{Environment.NewLine}  ", errors)
            );
        }
    }
}
