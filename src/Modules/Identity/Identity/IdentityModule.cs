using System.Text;
using _116.Identity.Domain.Constants;
using _116.Shared.Application.Configurations;
using _116.Shared.Application.Exceptions.Handlers.Contracts;
using _116.Shared.Infrastructure;
using _116.Shared.Infrastructure.Seed;
using _116.Identity.Application.Shared.Authorizations.Extensions;
using _116.Identity.Application.Shared.Exceptions.Handlers;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Application.Shared.Services;
using _116.Identity.Infrastructure.Repositories;
using _116.Identity.Infrastructure.Persistence;
using _116.Identity.Infrastructure.Persistence.Seeds.SuperAdmin;
using _116.Identity.Infrastructure.Persistence.Seeds.Visitor;
using _116.Identity.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace _116.Identity;

/// <summary>
/// Provides extension methods to register and configure the Identity module's services and middleware.
/// </summary>
public static class IdentityModule
{
    /// <summary>
    /// Gets the shared module configuration options for the Identity module.
    /// </summary>
    private static ModuleOptions<IdentityDbContext> GetModuleOptions() => new()
    {
        ModuleName = IdentityConstants.ModuleName,
        SchemaName = IdentityConstants.SchemaName,
        EnableMigrations = true,
        EnableSeeding = true
    };

    /// <summary>
    /// Adds the Identity module's services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to register services into.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> for chaining.</returns>
    /// <remarks>
    /// Registers database context with interceptors, authentication services, JWT configuration,
    /// and authorization policies for user management.
    /// </remarks>
    /// <example>
    /// <code>
    /// builder.Services.AddIdentityModule(builder.Configuration);
    /// </code>
    /// </example>
    public static IServiceCollection AddIdentityModule(this IServiceCollection services)
    {
        // Add services to the container.
        // Api Endpoint services.
        // Application UseCase services.
        // DataSource - Infrastructure services.

        // Register the database with base module infrastructure
        services.AddModuleDatabase(GetModuleOptions());

        // Configure Mapster mappings for optimal performance
        UserMapper.Configure();

        // Register Unit of Work for transaction management
        services.AddScoped<IIdentityUnitOfWork, IdentityUnitOfWork>();

        // Register user management services
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IOtpService, OtpService>();
        services.AddScoped<IOtpRepository, OtpRepository>();

        // Register data seeder for initial user data population
        services.AddScoped<IDataSeeder, SuperAdminSeeder>();
        services.AddScoped<IDataSeeder, VisitorRoleSeeder>();

        // Configure JWT Authentication
        var (secret, issuer, audience, _) = AppEnvironment.Jwt();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret!)),
                ClockSkew = TimeSpan.Zero
            };

            // Configure custom JWT Bearer events for consistent error handling
            options.ConfigureJwtBearerEvents();
        });

        // Configure Identityorization using centralized configuration
        services.AddIdentityModuleAuthorization();

        // Register custom exception handlers for this module
        services.AddSingleton<IExceptionStrategy, AccountInactiveExceptionHandler>();
        services.AddSingleton<IExceptionStrategy, AccountNotVerifiedExceptionHandler>();
        services.AddSingleton<IExceptionStrategy, UserNotLoggedInExceptionHandler>();
        services.AddSingleton<IExceptionStrategy, OtpAttemptsLimitExceptionHandler>();
        services.AddSingleton<IExceptionStrategy, OtpExpirationExceptionHandler>();
        services.AddSingleton<IExceptionStrategy, AccessDeniedExceptionHandler>();

        return services;
    }

    /// <summary>
    /// Configures the Identity module's middleware in the application pipeline.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The updated <see cref="IApplicationBuilder"/> for chaining.</returns>
    /// <remarks>
    /// Applies pending EF Core migrations and executes the data seeder for user management.
    /// </remarks>
    /// <example>
    /// <code>
    /// app.UseIdentityModule();
    /// </code>
    /// </example>
    public static IApplicationBuilder UseIdentityModule(this IApplicationBuilder app)
    {
        // Configure Http request pipeline.
        // Use Api endpoint services.
        // Use application UseCase services.
        // Use DataSource - Infrastructure services.
        app.UseModuleDatabase(GetModuleOptions());

        return app;
    }
}
