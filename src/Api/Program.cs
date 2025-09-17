using System.Reflection;
using _116.Shared.Application.Extensions;
using _116.Core;
using Carter;
using Microsoft.OpenApi.Models;
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
Assembly userAssembly = typeof(UserModule).Assembly;

builder.Services.AddCarterWithAssemblies(
    coreAssembly,
    userAssembly
);

builder.Services.AddCqrsWithAssemblies(
    coreAssembly,
    userAssembly
);

builder.Services.AddAuthorization();

builder.Services
    .AddCoreModule()
    .AddUserModule()
    .AddEndpointsApiExplorer()
    .AddSwaggerGen(c =>
        {
            // Add JWT authentication to Swagger
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header uses Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        }
    );

builder.Services.AddAppExceptionHandler();

WebApplication app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseSerilogRequestLogging();
app.UseAppExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();
app.MapCarter();

// Configure middleware extensions  modules.
app
    .UseCoreModule()
    .UseUserModule();

app.Run();

