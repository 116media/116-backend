using System.Reflection;
using System.Text.Json.Serialization;
using _116.Shared.Application.Configurations;
using _116.Shared.Application.Extensions;
using Asp.Versioning;
using Carter;
using DotNetEnv;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) => config.ReadFrom.Configuration(context.Configuration));

Env.Load();
Env.TraversePath().Load();

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

builder.Services.AddAppLocalization();

string[] allowedOrigins = AppEnvironment.CorsAllowedOrigins();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
        }
        else
        {
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        }
    });
});

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter())
);

builder
    .Services.AddIdentityModule()
    .AddCoreModule()
    .AddContentModule()
    .AddMailerModule()
    .AddEndpointsApiExplorer()
    .AddSwaggerGen(c => c.AddSwaggerOptions());

builder.Services.AddAppExceptionHandler();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

WebApplication app = builder.Build();

app.UseForwardedHeaders();
app.UseSwaggerFormatting();
app.UseSwagger();
app.UseSwaggerUI();

app.UseSerilogRequestLogging();
app.UseAppLocalization();
app.UseAppExceptionHandler();
app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.UseApiVersioning();

app.MapCarter();
app.UseResourceNotFoundHandler();

app.UseIdentityModule().UseCoreModule().UseContentModule().UseMailerModule();

app.Run();

// Required for WebApplicationFactory<Program> in integration tests
public partial class Program;
