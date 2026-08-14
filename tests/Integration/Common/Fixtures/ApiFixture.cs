using System.Text;
using System.Threading.RateLimiting;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Editorial.Services;
using _116.Content.Application.Shared.Services;
using _116.Content.Infrastructure.Persistence;
using _116.Core.Application.Shared.Services;
using _116.Core.Infrastructure.Persistence;
using _116.Identity.Infrastructure.Persistence;
using _116.Integration.Tests.Common.Stubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.IdentityModel.Tokens;

namespace _116.Integration.Tests.Common.Fixtures;

/// <summary>
/// Wraps <see cref="WebApplicationFactory{TEntryPoint}" /> for integration tests.
/// Replaces database connections, JWT configuration, and external service stubs.
/// </summary>
/// <remarks>
/// Creates a new <see cref="ApiFixture" /> backed by the given Testcontainer database.
/// </remarks>
public class ApiFixture(PostgresFixture db) : WebApplicationFactory<Program>
{
    private readonly PostgresFixture _db = db;

    /// <summary>
    /// Controls whether the production rate limit policies are replaced with no-op limiters.
    /// Defaults to <c>true</c> so that the shared test host never returns 429 responses and
    /// tests may issue as many requests as they need.
    /// Derived fixtures that assert real rate limiting behaviour override this to <c>false</c>,
    /// which leaves the production <c>AddRateLimiting</c> configuration untouched.
    /// </summary>
    protected virtual bool DisableRateLimits => true;

    /// <inheritdoc />
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        SetEnvironmentVariables();

        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            ReplaceDbContexts(services);
            StubExternalServices(services);
            OverrideJwtAuthentication(services);

            if (DisableRateLimits)
            {
                DisableRateLimiting(services);
            }
        });
    }

    /// <summary>
    /// Sets all environment variables consumed by <c>AppEnvironment</c>
    /// so that the real modules boot with test-safe values.
    /// </summary>
    private void SetEnvironmentVariables()
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");

        var connParts = ParseConnectionString(_db.ConnectionString);
        Environment.SetEnvironmentVariable("POSTGRES_HOST", connParts.host);
        Environment.SetEnvironmentVariable("POSTGRES_PORT", connParts.port);
        Environment.SetEnvironmentVariable("POSTGRES_DB", connParts.db);
        Environment.SetEnvironmentVariable("POSTGRES_USER", connParts.user);
        Environment.SetEnvironmentVariable("POSTGRES_PASSWORD", connParts.password);

        Environment.SetEnvironmentVariable("JWT_SECRET", "ThisIsAVerySecureSecretKeyForTesting123!@#");
        Environment.SetEnvironmentVariable("JWT_ISSUER", "116_test");
        Environment.SetEnvironmentVariable("JWT_AUDIENCE", "116_test_client");
        Environment.SetEnvironmentVariable("JWT_ACCESS_TOKEN_EXPIRATION", "60");
        Environment.SetEnvironmentVariable("JWT_REFRESH_TOKEN_EXPIRATION", "43200");

        Environment.SetEnvironmentVariable("DEFAULT_USER_PASSWORD", "Test@12345");

        Environment.SetEnvironmentVariable("CLOUDINARY_CLOUD_NAME", "test-cloud");
        Environment.SetEnvironmentVariable("CLOUDINARY_API_KEY", "test-api-key");
        Environment.SetEnvironmentVariable("CLOUDINARY_API_SECRET", "test-api-secret");
    }

    /// <summary>
    /// Replaces all module DbContexts to point at the Testcontainer database.
    /// </summary>
    private void ReplaceDbContexts(IServiceCollection services)
    {
        ReplaceDbContext<IdentityDbContext>(services);
        ReplaceDbContext<CoreDbContext>(services);
        ReplaceDbContext<ContentDbContext>(services);
    }

    /// <summary>
    /// Removes the existing DbContext registration and re-registers it
    /// against the Testcontainer connection string.
    /// </summary>
    private void ReplaceDbContext<TDbContext>(IServiceCollection services)
        where TDbContext : DbContext
    {
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<TDbContext>));

        if (descriptor is not null)
        {
            services.Remove(descriptor);
        }

        var poolDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(TDbContext));

        if (poolDescriptor is not null)
        {
            services.Remove(poolDescriptor);
        }

        services.AddDbContext<TDbContext>(options =>
        {
            options.UseNpgsql(_db.ConnectionString).UseSnakeCaseNamingConvention();
        });
    }

    /// <summary>
    /// Overrides JWT Bearer token validation parameters to use test constants,
    /// because the production module captures env vars before test env vars are set.
    /// </summary>
    private static void OverrideJwtAuthentication(IServiceCollection services)
    {
        services.PostConfigure<JwtBearerOptions>(
            JwtBearerDefaults.AuthenticationScheme,
            options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = Jwt.ValidIssuer,
                    ValidAudience = Jwt.ValidAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Jwt.ValidSecret)),
                    ClockSkew = TimeSpan.Zero,
                };
            }
        );
    }

    /// <summary>
    /// Disables all production rate limit policies so tests never receive 429 responses.
    /// Removes the original <c>RateLimiterOptions</c> configure actions and registers
    /// a fresh configuration with no-op policies for every policy name.
    /// </summary>
    private static void DisableRateLimiting(IServiceCollection services)
    {
        var optionsType = typeof(RateLimiterOptions);

        var configureDescriptors = services
            .Where(d =>
                d.ServiceType.IsGenericType
                && d.ServiceType.GenericTypeArguments.Length == 1
                && d.ServiceType.GenericTypeArguments[0] == optionsType
                && d.ServiceType.GetGenericTypeDefinition().Name.StartsWith("IConfigureOptions")
            )
            .ToList();

        foreach (var d in configureDescriptors)
        {
            services.Remove(d);
        }

        services.Configure<RateLimiterOptions>(options =>
        {
            options.RejectionStatusCode = 429;

            string[] policies =
            [
                RateLimitPolicies.Authentication,
                RateLimitPolicies.Otp,
                RateLimitPolicies.PasswordManagement,
                RateLimitPolicies.FileUpload,
                RateLimitPolicies.DataExport,
                RateLimitPolicies.ContentBrowsing,
                RateLimitPolicies.UserProfile,
                RateLimitPolicies.SessionManagement,
                RateLimitPolicies.AdminMetrics,
                RateLimitPolicies.ContentContribution,
            ];

            foreach (var policy in policies)
            {
                options.AddPolicy(policy, _ => RateLimitPartition.GetNoLimiter("test"));
            }
        });
    }

    /// <summary>
    /// Replaces external service implementations with in-memory stubs.
    /// </summary>
    private static void StubExternalServices(IServiceCollection services)
    {
        Replace<ICloudinaryService, StubCloudinaryService>(services);
        Replace<IYoutubeThumbnailService, StubYoutubeThumbnailService>(services);
        Replace<IStreamingLinkResolutionService, StubStreamingLinkResolutionService>(services);
    }

    /// <summary>
    /// Removes all registrations of <typeparamref name="TService" /> and adds
    /// a scoped <typeparamref name="TImpl" />.
    /// </summary>
    private static void Replace<TService, TImpl>(IServiceCollection services)
        where TService : class
        where TImpl : class, TService
    {
        var descriptors = services.Where(d => d.ServiceType == typeof(TService)).ToList();
        foreach (var d in descriptors)
        {
            services.Remove(d);
        }

        services.AddScoped<TService, TImpl>();
    }

    /// <summary>
    /// Parses a Npgsql-style connection string into its components.
    /// </summary>
    private static (string host, string port, string db, string user, string password) ParseConnectionString(
        string connectionString
    )
    {
        var parts = connectionString
            .Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Split('=', 2))
            .Where(p => p.Length == 2)
            .ToDictionary(p => p[0].Trim(), p => p[1].Trim(), StringComparer.OrdinalIgnoreCase);

        return (
            host: parts.GetValueOrDefault("Host", "localhost"),
            port: parts.GetValueOrDefault("Port", "5432"),
            db: parts.GetValueOrDefault("Database", "test_116_db"),
            user: parts.GetValueOrDefault("Username", "test_user"),
            password: parts.GetValueOrDefault("Password", "test_password")
        );
    }
}
