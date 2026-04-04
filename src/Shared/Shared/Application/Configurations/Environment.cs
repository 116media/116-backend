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
    /// - JWT_ACCESS_TOKEN_EXPIRATION: The access token expiration time in minutes (e.g., "60" for 1 hour)
    /// - JWT_REFRESH_TOKEN_EXPIRATION: The refresh token expiration time in minutes (e.g., "43200" for 30 days)
    /// </remarks>
    /// <returns>
    /// A tuple containing:
    /// - <c>secret</c>: The JWT secret key
    /// - <c>issuer</c>: The JWT issuer
    /// - <c>audience</c>: The JWT audience
    /// - <c>accessTokenExpiration</c>: The JWT access token expiration duration in minutes
    /// - <c>refreshTokenExpiration</c>: The refresh token expiration duration in minutes
    /// </returns>
    public static (
        string? secret,
        string? issuer,
        string? audience,
        string? accessTokenExpiration,
        string? refreshTokenExpiration
    ) Jwt()
    {
        string? secret = Environment.GetEnvironmentVariable("JWT_SECRET");
        string? issuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
        string? audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");
        string? accessTokenExpiration = Environment.GetEnvironmentVariable("JWT_ACCESS_TOKEN_EXPIRATION");
        string? refreshTokenExpiration = Environment.GetEnvironmentVariable("JWT_REFRESH_TOKEN_EXPIRATION");

        return (secret, issuer, audience, accessTokenExpiration, refreshTokenExpiration);
    }

    /// <summary>
    /// Retrieves Cloudinary configuration values from environment variables.
    /// </summary>
    /// <remarks>
    /// Expected environment variables:
    /// - CLOUDINARY_CLOUD_NAME: The Cloudinary cloud name from account settings
    /// - CLOUDINARY_API_KEY: The Cloudinary API key for authentication
    /// - CLOUDINARY_API_SECRET: The Cloudinary API secret for signing requests
    /// </remarks>
    /// <returns>
    /// A tuple containing:
    /// - <c>cloudName</c>: The Cloudinary cloud name
    /// - <c>apiKey</c>: The Cloudinary API key
    /// - <c>apiSecret</c>: The Cloudinary API secret
    /// </returns>
    public static (string? cloudName, string? apiKey, string? apiSecret) Cloudinary()
    {
        string? cloudName = Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME");
        string? apiKey = Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY");
        string? apiSecret = Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET");

        return (cloudName, apiKey, apiSecret);
    }

    /// <summary>
    /// Retrieves the allowed CORS origins from the DASHBOARD_ORIGIN and WEBAPP_ORIGIN
    /// environment variables.
    /// </summary>
    /// <returns>
    /// An array of allowed origin strings. Returns an empty array if no origins are configured.
    /// </returns>
    public static string[] CorsAllowedOrigins()
    {
        string? dashboardOrigin = Environment.GetEnvironmentVariable("DASHBOARD_ORIGIN");
        string? webAppOrigin = Environment.GetEnvironmentVariable("WEBAPP_ORIGIN");

        return [.. new[] { dashboardOrigin, webAppOrigin }.Where(o => !string.IsNullOrWhiteSpace(o)).Select(o => o!)];
    }
}
