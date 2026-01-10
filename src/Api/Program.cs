using System.Reflection;
using _116.Shared.Application.Extensions;
using Asp.Versioning;
using Carter;
using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) => config.ReadFrom.Configuration(context.Configuration));

DotNetEnv.Env.Load();
DotNetEnv.Env.TraversePath().Load();

// Load Cloudinary configuration from environment variables
builder.Services.AddCloudinaryConfiguration();

Assembly coreAssembly = typeof(CoreModule).Assembly;
Assembly identityAssembly = typeof(IdentityModule).Assembly;

builder.Services.AddCarterWithAssemblies(identityAssembly, coreAssembly);

builder.Services.AddCqrsWithAssemblies(identityAssembly, coreAssembly);

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

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        //TODO: should update this to only allow frontend domain name
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

builder
    .Services.AddIdentityModule()
    .AddCoreModule()
    .AddEndpointsApiExplorer()
    .AddSwaggerGen(c => c.AddSwaggerOptions());

builder.Services.AddAppExceptionHandler();

WebApplication app = builder.Build();

app.UseSwaggerFormatting();
app.UseSwagger();
app.UseSwaggerUI();

app.UseSerilogRequestLogging();
app.UseAppExceptionHandler();
app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.UseApiVersioning();

app.MapCarter();
app.UseResourceNotFoundHandler();

app.UseIdentityModule().UseCoreModule();

app.Run();
