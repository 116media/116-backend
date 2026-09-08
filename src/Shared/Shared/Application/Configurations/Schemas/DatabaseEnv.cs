namespace _116.Shared.Application.Configurations.Schemas;

/// <summary>
/// Postgres connection variables.
/// </summary>
public static class DatabaseEnv
{
    public static readonly EnvVar<string> Host = EnvVar.Required("POSTGRES_HOST");

    public static readonly EnvVar<int> Port = EnvVar.Int("POSTGRES_PORT", @default: 5432).InRange(min: 1, max: 65535);

    public static readonly EnvVar<string> Name = EnvVar.Required("POSTGRES_DB");

    public static readonly EnvVar<string> User = EnvVar.Required("POSTGRES_USER");

    public static readonly EnvVar<string> Password = EnvVar.Required("POSTGRES_PASSWORD");

    /// <summary>
    /// Builds the base Npgsql connection string; callers append connection-specific options.
    /// </summary>
    /// <returns>The connection string, terminated with a semicolon.</returns>
    public static string ConnectionString()
    {
        return $"Host={Host.Value};Port={Port.Value};Database={Name.Value};"
            + $"Username={User.Value};Password={Password.Value};";
    }
}
