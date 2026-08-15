using System.Threading.RateLimiting;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Editorial.Services;
using _116.Content.Application.Shared.Services;
using _116.Content.Infrastructure.Persistence;
using _116.Core.Application.Shared.Services;
using _116.Core.Infrastructure.Persistence;
using _116.Core.Infrastructure.Services;
using _116.Identity.Application.Adapters.SocialAuth;
using _116.Identity.Domain.Enums;
using _116.Identity.Infrastructure.Persistence;
using _116.Integration.Tests.Common.Stubs;
using _116.Mailer.Application.Shared.Services;
using _116.Mailer.Infrastructure.Persistence;
using _116.Shared.Application.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Hosting;
using Quartz;

namespace _116.Integration.Tests.Common.Fixtures;

/// <summary>
/// Wraps <see cref="WebApplicationFactory{TEntryPoint}" /> for integration tests.
/// Replaces database connections and external services; the JWT validation parameters
/// the application configured from the environment are left untouched.
/// </summary>
public class ApiFixture(PostgresFixture db) : WebApplicationFactory<Program>
{
    private readonly PostgresFixture _db = db;

    /// <summary>
    /// Controls whether the production rate limit policies are replaced with no-op limiters.
    /// Defaults to <c>true</c>; fixtures asserting real rate limiting override it to
    /// <c>false</c>.
    /// </summary>
    protected virtual bool DisableRateLimits => true;

    /// <summary>
    /// The environment the host boots in. Defaults to <c>Testing</c>, which disables module
    /// migrations and seeding. Derived fixtures override it to boot a non-Testing host.
    /// </summary>
    protected virtual string EnvironmentName => "Testing";

    /// <inheritdoc />
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        ConfigureEnvironment();

        builder.UseEnvironment(EnvironmentName);

        builder.ConfigureTestServices(services =>
        {
            ReplaceDbContexts(services);
            StubExternalServices(services);
            DisableScheduledJobs(services);

            if (DisableRateLimits)
            {
                DisableRateLimiting(services);
            }
        });
    }

    /// <summary>
    /// Removes the Quartz hosted service so no trigger fires during a test.
    /// Job and trigger definitions stay registered.
    /// </summary>
    /// <param name="services">The test host's service collection.</param>
    private static void DisableScheduledJobs(IServiceCollection services)
    {
        List<ServiceDescriptor> hostedServices = services
            .Where(descriptor =>
                descriptor.ServiceType == typeof(IHostedService)
                && descriptor.ImplementationType == typeof(QuartzHostedService)
            )
            .ToList();

        foreach (ServiceDescriptor descriptor in hostedServices)
        {
            services.Remove(descriptor);
        }
    }

    /// <summary>
    /// Sets all environment variables consumed by <c>AppEnvironment</c>
    /// so that the real modules boot with test-safe values. Derived fixtures override it to
    /// vary a single variable for the host they own, calling the base implementation first.
    /// </summary>
    protected virtual void ConfigureEnvironment()
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

        Environment.SetEnvironmentVariable("EMAIL_PROVIDER", "smtp");
        Environment.SetEnvironmentVariable("EMAIL_FROM_ADDRESS", "no-reply@test.116");
        Environment.SetEnvironmentVariable("EMAIL_FROM_NAME", "116 Tests");
        Environment.SetEnvironmentVariable("FRONTEND_BASE_URL", "http://localhost:3000");
    }

    /// <summary>
    /// Replaces all module DbContexts to point at the Testcontainer database.
    /// </summary>
    private void ReplaceDbContexts(IServiceCollection services)
    {
        ReplaceDbContext<IdentityDbContext>(services);
        ReplaceDbContext<CoreDbContext>(services);
        ReplaceDbContext<ContentDbContext>(services);
        ReplaceDbContext<MailerDbContext>(services);
    }

    /// <summary>
    /// Points a module DbContext at the Testcontainer database, removing every pooled-context
    /// descriptor first and re-attaching the save-changes interceptors.
    /// </summary>
    /// <typeparam name="TDbContext">The module context to re-register.</typeparam>
    /// <param name="services">The test host's service collection.</param>
    private void ReplaceDbContext<TDbContext>(IServiceCollection services)
        where TDbContext : DbContext
    {
        Type[] pooledDescriptorTypes =
        [
            typeof(DbContextOptions<TDbContext>),
            typeof(TDbContext),
            typeof(IDbContextPool<TDbContext>),
            typeof(IScopedDbContextLease<TDbContext>),
        ];

        List<ServiceDescriptor> existing = services
            .Where(descriptor => pooledDescriptorTypes.Contains(descriptor.ServiceType))
            .ToList();

        foreach (ServiceDescriptor descriptor in existing)
        {
            services.Remove(descriptor);
        }

        services.AddDbContextPool<TDbContext>(
            (serviceProvider, options) =>
            {
                options.AddInterceptors(serviceProvider.GetServices<ISaveChangesInterceptor>());
                options.UseNpgsql(_db.ConnectionString).UseSnakeCaseNamingConvention();
            }
        );
    }

    /// <summary>
    /// Disables all production rate limit policies so tests never receive 429 responses.
    /// Removes the original <c>RateLimiterOptions</c> configure actions and registers
    /// a fresh configuration with no-op policies for every policy name, keeping the production
    /// rejection status code and <c>OnRejected</c> handler so the host never diverges on the
    /// contract a rejected client would receive.
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
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = RateLimitingExtension.OnRateLimitRejected;

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
        ReplaceCloudinaryService(services);
        Replace<IYoutubeThumbnailService, StubYoutubeThumbnailService>(services);
        ReplaceStreamingLinkResolutionService(services);
        ReplaceEmailSender(services);
        ReplaceSocialTokenVerifier(services);
        StubRemoteFileTransport(services);
    }

    /// <summary>
    /// Replaces the remote-file HTTP transport with a stub handler so <see cref="FileService" />'s
    /// download path runs end-to-end — through the real SSRF guard and metadata resolution — without
    /// any real outbound request. The guard still runs against the URL, so a blocked address is
    /// rejected before the handler is reached.
    /// </summary>
    private static void StubRemoteFileTransport(IServiceCollection services)
    {
        services
            .AddHttpClient<IFileService, FileService>(client => client.Timeout = TimeSpan.FromSeconds(10))
            .ConfigurePrimaryHttpMessageHandler(() => new StubRemoteFileHandler());
    }

    /// <summary>
    /// Replaces the real keyed provider verifiers with a single scriptable stub, so social-login is
    /// driven through the real pipeline — including the real <see cref="ISocialTokenVerifierFactory" />
    /// and its keyed resolution — without calling Google or Facebook. Only Google is registered:
    /// Facebook is deliberately left without a verifier so the factory's unsupported-provider path is
    /// exercised end-to-end through the real endpoint.
    /// </summary>
    private static void ReplaceSocialTokenVerifier(IServiceCollection services)
    {
        RemoveAll<ISocialTokenVerifier>(services);

        services.AddSingleton<StubSocialTokenVerifier>();
        services.AddKeyedSingleton<ISocialTokenVerifier>(
            EnumAuthProvider.Google,
            (sp, _) => sp.GetRequiredService<StubSocialTokenVerifier>()
        );
        services.AddSingleton<IResettableStub>(sp => sp.GetRequiredService<StubSocialTokenVerifier>());
    }

    /// <summary>
    /// Replaces the Cloudinary adapter with the stub, registered as a singleton and
    /// under <see cref="IResettableStub" /> so tests share and reset one instance.
    /// </summary>
    private static void ReplaceCloudinaryService(IServiceCollection services)
    {
        RemoveAll<ICloudinaryService>(services);

        services.AddSingleton<StubCloudinaryService>();
        services.AddSingleton<ICloudinaryService>(sp => sp.GetRequiredService<StubCloudinaryService>());
        services.AddSingleton<IResettableStub>(sp => sp.GetRequiredService<StubCloudinaryService>());
    }

    /// <summary>
    /// Replaces the Odesli-backed resolution adapter with the stub, registered as a singleton
    /// and under <see cref="IResettableStub" /> so tests share and reset one instance.
    /// </summary>
    private static void ReplaceStreamingLinkResolutionService(IServiceCollection services)
    {
        RemoveAll<IStreamingLinkResolutionService>(services);

        services.AddSingleton<StubStreamingLinkResolutionService>();
        services.AddSingleton<IStreamingLinkResolutionService>(sp =>
            sp.GetRequiredService<StubStreamingLinkResolutionService>()
        );
        services.AddSingleton<IResettableStub>(sp => sp.GetRequiredService<StubStreamingLinkResolutionService>());
    }

    /// <summary>
    /// Replaces the SMTP adapter with the recording stub, registered as a singleton
    /// and under <see cref="IResettableStub" /> so tests share and reset one instance.
    /// </summary>
    private static void ReplaceEmailSender(IServiceCollection services)
    {
        RemoveAll<IEmailSender>(services);

        services.AddSingleton<StubEmailSender>();
        services.AddSingleton<IEmailSender>(sp => sp.GetRequiredService<StubEmailSender>());
        services.AddSingleton<IResettableStub>(sp => sp.GetRequiredService<StubEmailSender>());
    }

    /// <summary>
    /// Removes all registrations of <typeparamref name="TService" /> and adds
    /// a scoped <typeparamref name="TImpl" />. Reserved for stateless stubs.
    /// </summary>
    private static void Replace<TService, TImpl>(IServiceCollection services)
        where TService : class
        where TImpl : class, TService
    {
        RemoveAll<TService>(services);

        services.AddScoped<TService, TImpl>();
    }

    /// <summary>
    /// Removes every registration of <typeparamref name="TService" /> from the collection.
    /// </summary>
    private static void RemoveAll<TService>(IServiceCollection services)
        where TService : class
    {
        List<ServiceDescriptor> descriptors = services.Where(d => d.ServiceType == typeof(TService)).ToList();

        foreach (ServiceDescriptor descriptor in descriptors)
        {
            services.Remove(descriptor);
        }
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
