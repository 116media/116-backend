using System.Reflection;
using _116.Shared.Application.Extensions;
using Asp.Versioning;
using Carter;
using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration)
);

// Load environments variables from .env file
DotNetEnv.Env.Load();
DotNetEnv.Env.TraversePath().Load();

// Add services to the container.
// Register Carter and CQRS Assemblies
Assembly coreAssembly = typeof(CoreModule).Assembly;
Assembly authAssembly = typeof(AuthModule).Assembly;

builder.Services.AddCarterWithAssemblies(
    authAssembly,
    coreAssembly
);

builder.Services.AddCqrsWithAssemblies(
    authAssembly,
    coreAssembly
);

// Configure API Versioning
builder.Services.AddApiVersioning(options =>
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

builder.Services
    .AddAuthModule()
    .AddCoreModule()
    .AddEndpointsApiExplorer()
    .AddSwaggerGen(c => c.AddSwaggerOptions());

builder.Services.AddAppExceptionHandler();

WebApplication app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseSerilogRequestLogging();
app.UseAppExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();

app.UseApiVersioning();

app.MapCarter();
app.UseResourceNotFoundHandler();

// Configure middleware extensions  modules.
app
    .UseAuthModule()
    .UseCoreModule();

app.Run();

