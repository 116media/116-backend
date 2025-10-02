using System.Text;
using _116.Auth.Domain.Constants;
using _116.Shared.Application.Configurations;
using _116.Shared.Application.Exceptions.Handlers.Contracts;
using _116.Shared.Infrastructure;
using _116.Shared.Infrastructure.Seed;
using _116.Auth.Application.Shared.Authorizations.Extensions;
using _116.Auth.Application.Shared.Exceptions.Handlers;
using _116.Auth.Application.Shared.Mappers;
using _116.Auth.Application.Shared.Repositories;
using _116.Auth.Application.Shared.Services;
using _116.Auth.Infrastructure.Repositories;
using _116.Auth.Infrastructure.Persistence;
using _116.Auth.Infrastructure.Persistence.Seeds.SuperAdmin;
using _116.Auth.Infrastructure.Persistence.Seeds.Visitor;
using _116.Auth.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace _116.Auth;

/// <summary>
/// Provides extension methods to register and configure the Auth module's services and middleware.
/// </summary>
public static class AuthModule
{
    /// <summary>
    /// Gets the shared module configuration options for the Auth module.
    /// </summary>
    private static ModuleOptions<AuthDbContext> GetModuleOptions() => new()
    {
        ModuleName = AuthConstants.ModuleName,
        SchemaName = AuthConstants.SchemaName,
        EnableMigrations = true,
        EnableSeeding = true
    };

    /// <summary>
    /// Adds the Auth module's services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to register services into.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> for chaining.</returns>
    /// <remarks>
    /// Registers database context with interceptors, authentication services, JWT configuration,
    /// and authorization policies for user management.
    /// </remarks>
    /// <example>
    /// <code>
    /// builder.Services.AddAuthModule(builder.Configuration);
    /// </code>
    /// </example>
    public static IServiceCollection AddAuthModule(this IServiceCollection services)
    {
        // Add services to the container.
        // Api Endpoint services.
        // Application UseCase services.
        // DataSource - Infrastructure services.

        // Register the database with base module infrastructure
        services.AddModuleDatabase(GetModuleOptions());

        // Configure Mapster mappings for optimal performance
        UserMapper.Configure();

        // Register user management services
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IUserService, UserService>();
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

        // Configure Authorization using centralized configuration
        services.AddAuthModuleAuthorization();

        // Register custom exception handlers for this module
        services.AddSingleton<IExceptionStrategy, AccountInactiveExceptionHandler>();
        services.AddSingleton<IExceptionStrategy, AccountNotVerifiedExceptionHandler>();
        services.AddSingleton<IExceptionStrategy, UserNotLoggedInExceptionHandler>();

        return services;
    }

    /// <summary>
    /// Configures the Auth module's middleware in the application pipeline.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The updated <see cref="IApplicationBuilder"/> for chaining.</returns>
    /// <remarks>
    /// Applies pending EF Core migrations and executes the data seeder for user management.
    /// </remarks>
    /// <example>
    /// <code>
    /// app.UseAuthModule();
    /// </code>
    /// </example>
    public static IApplicationBuilder UseAuthModule(this IApplicationBuilder app)
    {
        // Configure Http request pipeline.
        // Use Api endpoint services.
        // Use application UseCase services.
        // Use DataSource - Infrastructure services.
        app.UseModuleDatabase(GetModuleOptions());

        return app;
    }
}
