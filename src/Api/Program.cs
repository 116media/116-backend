using System.Reflection;
using System.Text.Json.Serialization;
using _116.Api;
using _116.Shared.Application.Configurations;
using _116.Shared.Application.Extensions;
using _116.Shared.Infrastructure.Cache;
using _116.Shared.Infrastructure.Seed;
using Asp.Versioning;
using Carter;
using DotNetEnv;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Caching.Hybrid;
using Serilog;
using StackExchange.Redis;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) => config.ReadFrom.Configuration(context.Configuration));

// The .env file supplies defaults only; variables already present in the process win.
Env.NoClobber().Load();
Env.NoClobber().TraversePath().Load();

// Load Cloudinary configuration from environment variables
builder.Services.AddCloudinaryConfiguration();

Assembly coreAssembly = typeof(CoreModule).Assembly;
Assembly identityAssembly = typeof(IdentityModule).Assembly;
Assembly contentAssembly = typeof(ContentModule).Assembly;
Assembly mailerAssembly = typeof(MailerModule).Assembly;

builder.Services.AddCarterWithAssemblies(identityAssembly, coreAssembly, contentAssembly, mailerAssembly);
builder.Services.AddCqrsWithAssemblies(identityAssembly, coreAssembly, contentAssembly, mailerAssembly);

builder
    .Services.AddApiVersioning(options =>
    {
        options.ReportApiVersions = true;
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.ApiVersionReader = ApiVersionReader.Combine(
            new UrlSegmentApiVersionReader(),
            new HeaderApiVersionReader("X-Api-Version")
        );
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'V";
        options.SubstituteApiVersionInUrl = true;
    });

builder.Services.AddAuthorization();

builder.Services.AddRateLimiting();

builder.Services.AddMemoryCache();

// With REDIS_URL set the hybrid cache gains Redis as its distributed layer plus an eviction
// backplane, making tag eviction visible across instances. Without it the cache runs
// in-process only.
string? redisUrl = AppEnvironment.RedisUrl();
if (!string.IsNullOrWhiteSpace(redisUrl))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisUrl;
        options.InstanceName = "116:";
    });
    builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisUrl));
}

// Entry options are per-request via ICacheableRequest; these are defaults.
builder.Services.AddHybridCache(options =>
{
    options.DefaultEntryOptions = new HybridCacheEntryOptions
    {
        Expiration = TimeSpan.FromMinutes(10),
        LocalCacheExpiration = TimeSpan.FromMinutes(10),
    };
});

if (!string.IsNullOrWhiteSpace(redisUrl))
{
    builder.Services.Decorate<HybridCache, BackplaneHybridCache>();
}

builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddAppLocalization();

string[] allowedOrigins = AppEnvironment.CorsAllowedOrigins();
bool isDevelopment = builder.Environment.IsDevelopment();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
        }
        else if (isDevelopment)
        {
            // Local convenience only: no origins configured in Development means allow any.
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        }

        // Outside Development with no configured origins the policy is left empty — CORS fails closed,
        // so a misconfigured deploy rejects cross-origin calls instead of allowing every origin.
    });
});

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter())
);

// The clustered job store needs real Postgres and its quartz schema; Testing hosts remove the
// scheduler entirely and keep the default in-memory store registration.
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddClusteredQuartzStore();
}

// Startup order matters: migrations (Development only) run before seeding, and both before the
// Quartz hosted service the modules register below, whose persistent store needs the schema.
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddHostedService<DevelopmentMigrationHostedService>();
}

builder.Services.AddHostedService<DataSeedingHostedService>();

builder
    .Services.AddIdentityModule(builder.Environment)
    .AddCoreModule(builder.Environment)
    .AddContentModule(builder.Environment)
    .AddMailerModule(builder.Environment)
    .AddEndpointsApiExplorer()
    .AddSwaggerGen(c => c.AddSwaggerOptions());

builder.Services.AddAppExceptionHandler();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
    options.ForwardLimit = 1;

    foreach (var network in AppEnvironment.TrustedProxyNetworks())
    {
        options.KnownNetworks.Add(network);
    }
});

WebApplication app = builder.Build();

// Explicit migration mode: `dotnet run --project src/Api -- migrate` applies every module's
// pending migrations and exits. Deploys run this before rolling instances, which is also the
// only order under which CONCURRENTLY index builds can run out of band.
if (args.Contains("migrate"))
{
    await DatabaseMigrator.MigrateAllAsync(app.Services);
    return;
}

if (!app.Environment.IsDevelopment() && allowedOrigins.Length == 0)
{
    app.Logger.LogWarning(
        "CORS: no allowed origins configured outside Development — cross-origin browser requests are "
            + "blocked (fail-closed). Set WEBAPP_ORIGIN / DASHBOARD_ORIGIN."
    );
}

app.UseForwardedHeaders();
app.UseSwaggerFormatting();
app.UseSwagger();
app.UseSwaggerUI();

app.UseSerilogRequestLogging();
app.UseAppLocalization();
app.UseCors();
app.UseAppExceptionHandler();
app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();

app.UseApiVersioning();

app.MapCarter();
app.UseResourceNotFoundHandler();

app.Run();

// Required for WebApplicationFactory<Program> in integration tests
public partial class Program;
