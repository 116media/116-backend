namespace _116.Shared.Application.Configurations;

/// <summary>
/// Provides access to environment-specific configuration values used in the application.
/// </summary>
public class AppEnvironment
{
    /// <summary>
    /// Retrieves postgresql database configuration values from environment variables.
    /// </summary>
    /// <remarks>
    /// This method is intended for development environments where connection details
    /// are provided via environment variables (e.g., from a .env file or local shell).
    ///
    /// Expected environment variables:
    /// - POSTGRES_HOST
    /// - POSTGRES_PORT
    /// - POSTGRES_DB
    /// - POSTGRES_USER
    /// - POSTGRES_PASSWORD
    /// </remarks>
    /// <returns>
    /// A tuple containing:
    /// - <c>host</c>: The database host
    /// - <c>port</c>: The postgresql port (e.g., "5432")
    /// - <c>db</c>: The database name
    /// - <c>user</c>: The database username
    /// - <c>pass</c>: The database password
    /// </returns>
    public static (string? host, string? port, string? db, string? user, string? pass) Database()
    {
        string? host = Environment.GetEnvironmentVariable("POSTGRES_HOST");
        string? port = Environment.GetEnvironmentVariable("POSTGRES_PORT");
        string? db = Environment.GetEnvironmentVariable("POSTGRES_DB");
        string? user = Environment.GetEnvironmentVariable("POSTGRES_USER");
        string? pass = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");

        return (host, port, db, user, pass);
    }

    /// <summary>
    /// Retrieves the default password used for seeding or initializing user accounts.
    /// </summary>
    /// <remarks>
    /// The value is fetched from the <c>DEFAULT_USER_PASSWORD</c> environment variable.
    /// This is typically set in development or deployment environments to provide
    /// a consistent default password for newly created accounts during application setup.
    /// </remarks>
    /// <returns>
    /// The default password string, or <c>null</c> if the environment variable is not set.
    /// </returns>
    public static string? DefaultPassword()
    {
        string? defaultPassword = Environment.GetEnvironmentVariable("DEFAULT_USER_PASSWORD");
        return defaultPassword;
    }

    /// <summary>
    /// Retrieves JWT configuration values from environment variables.
    /// </summary>
    /// <remarks>
    /// Expected environment variables:
    /// - JWT_SECRET: The secret key used to sign and verify JWT tokens
    /// - JWT_ISSUER: The issuer claim for JWT tokens
    /// - JWT_AUDIENCE: The audience claim for JWT tokens
    /// - JWT_EXPIRATION: The token expiration time (e.g., "1h", "30m")
    /// </remarks>
    /// <returns>
    /// A tuple containing:
    /// - <c>secret</c>: The JWT secret key
    /// - <c>issuer</c>: The JWT issuer
    /// - <c>audience</c>: The JWT audience
    /// - <c>expiration</c>: The JWT expiration duration
    /// </returns>
    public static (string? secret, string? issuer, string? audience, string? expiration) Jwt()
    {
        string? secret = Environment.GetEnvironmentVariable("JWT_SECRET");
        string? issuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
        string? audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");
        string? expiration = Environment.GetEnvironmentVariable("JWT_EXPIRATION");

        return (secret, issuer, audience, expiration);
    }
}
