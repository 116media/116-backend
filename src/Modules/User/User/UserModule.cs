using System.Text;
using _116.Shared.Application.Configurations;
using _116.Shared.Application.Exceptions.Handlers.Contracts;
using _116.Shared.Infrastructure;
using _116.Shared.Infrastructure.Seed;
using _116.User.Application.Shared.Authorizations.Extensions;
using _116.User.Application.Shared.Exceptions.Handlers;
using _116.User.Application.Shared.Mappers;
using _116.User.Application.Shared.Repositories;
using _116.User.Application.Shared.Services;
using _116.User.Infrastructure.Repositories;
using _116.User.Infrastructure.Persistence;
using _116.User.Infrastructure.Persistence.Seeds.SuperAdmin;
using _116.User.Infrastructure.Persistence.Seeds.Visitor;
using _116.User.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace _116.User;

/// <summary>
/// Provides extension methods to register and configure the User module's services and middleware.
/// </summary>
public static class UserModule
{
    /// <summary>
    /// Gets the shared module configuration options for the User module.
    /// </summary>
    private static ModuleOptions<UserDbContext> GetModuleOptions() => new()
    {
        ModuleName = "User",
        SchemaName = "user",
        EnableMigrations = true,
        EnableSeeding = true
    };

    /// <summary>
    /// Adds the User module's services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to register services into.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> for chaining.</returns>
    /// <remarks>
    /// Registers database context with interceptors, authentication services, JWT configuration,
    /// and authorization policies for user management.
    /// </remarks>
    /// <example>
    /// <code>
    /// builder.Services.AddUserModule(builder.Configuration);
    /// </code>
    /// </example>
    public static IServiceCollection AddUserModule(this IServiceCollection services)
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
        });

        // Configure Authorization using centralized configuration
        services.AddUserModuleAuthorization();

        // Register custom exception handlers for this module
        services.AddSingleton<IExceptionStrategy, AccountInactiveExceptionHandler>();
        services.AddSingleton<IExceptionStrategy, AccountNotVerifiedExceptionHandler>();

        return services;
    }

    /// <summary>
    /// Configures the User module's middleware in the application pipeline.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The updated <see cref="IApplicationBuilder"/> for chaining.</returns>
    /// <remarks>
    /// Applies pending EF Core migrations and executes the data seeder for user management.
    /// </remarks>
    /// <example>
    /// <code>
    /// app.UseUserModule();
    /// </code>
    /// </example>
    public static IApplicationBuilder UseUserModule(this IApplicationBuilder app)
    {
        // Configure Http request pipeline.
        // Use Api endpoint services.
        // Use application UseCase services.
        // Use DataSource - Infrastructure services.
        app.UseModuleDatabase(GetModuleOptions());

        return app;
    }
}
