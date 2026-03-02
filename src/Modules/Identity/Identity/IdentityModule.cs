using System.Text;
using _116.Identity.Application.Adapters.Wangkanai.Detection;
using _116.Identity.Application.Auth.Exceptions.Handlers;
using _116.Identity.Application.Auth.Repositories;
using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Auth.UseCases.Admin.Commands.ForgotPassword;
using _116.Identity.Application.Auth.UseCases.Admin.Commands.ForgotPassword.Contracts;
using _116.Identity.Application.Auth.UseCases.Admin.Commands.Login;
using _116.Identity.Application.Auth.UseCases.Admin.Commands.Login.Contracts;
using _116.Identity.Application.Auth.UseCases.Admin.Commands.ResendOtp;
using _116.Identity.Application.Auth.UseCases.Admin.Commands.ResendOtp.Contracts;
using _116.Identity.Application.Auth.UseCases.Admin.Commands.ResetPassword;
using _116.Identity.Application.Auth.UseCases.Admin.Commands.ResetPassword.Contracts;
using _116.Identity.Application.Auth.UseCases.Admin.Commands.SignOut;
using _116.Identity.Application.Auth.UseCases.Admin.Commands.SignOut.Contracts;
using _116.Identity.Application.Auth.UseCases.Public.Commands.ForgotPassword;
using _116.Identity.Application.Auth.UseCases.Public.Commands.ForgotPassword.Contracts;
using _116.Identity.Application.Auth.UseCases.Public.Commands.Login;
using _116.Identity.Application.Auth.UseCases.Public.Commands.Login.Contracts;
using _116.Identity.Application.Auth.UseCases.Public.Commands.ResendOtp;
using _116.Identity.Application.Auth.UseCases.Public.Commands.ResendOtp.Contracts;
using _116.Identity.Application.Auth.UseCases.Public.Commands.ResetPassword;
using _116.Identity.Application.Auth.UseCases.Public.Commands.ResetPassword.Contracts;
using _116.Identity.Application.Auth.UseCases.Public.Commands.SignOut;
using _116.Identity.Application.Auth.UseCases.Public.Commands.SignOut.Contracts;
using _116.Identity.Application.Auth.UseCases.Public.Commands.SignUp;
using _116.Identity.Application.Auth.UseCases.Public.Commands.SignUp.Contracts;
using _116.Identity.Application.Auth.UseCases.Public.Commands.SocialLogin;
using _116.Identity.Application.Auth.UseCases.Public.Commands.SocialLogin.Contracts;
using _116.Identity.Application.Session.Factories;
using _116.Identity.Application.Session.Repositories;
using _116.Identity.Application.Session.Services;
using _116.Identity.Application.Session.UseCases.Public.Commands.RefreshToken;
using _116.Identity.Application.Session.UseCases.Public.Commands.RefreshToken.Contracts;
using _116.Identity.Application.Shared.Authorizations.Extensions;
using _116.Identity.Application.Shared.Exceptions.Handlers;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Application.User.UseCases.Admin.Commands.UpdateAvatar;
using _116.Identity.Application.User.UseCases.Admin.Commands.UpdateAvatar.Contracts;
using _116.Identity.Application.User.UseCases.Admin.Commands.UpdateOwnProfile;
using _116.Identity.Application.User.UseCases.Admin.Commands.UpdateOwnProfile.Contracts;
using _116.Identity.Application.User.UseCases.Public.Commands.UpdateAvatar;
using _116.Identity.Application.User.UseCases.Public.Commands.UpdateAvatar.Contracts;
using _116.Identity.Application.User.UseCases.Public.Commands.UpdateOwnProfile;
using _116.Identity.Application.User.UseCases.Public.Commands.UpdateOwnProfile.Contracts;
using _116.Identity.Domain.Constants;
using _116.Identity.Infrastructure.Adapters.Wangkanai.Detection;
using _116.Identity.Infrastructure.Persistence;
using _116.Identity.Infrastructure.Persistence.Seeds.SuperAdmin;
using _116.Identity.Infrastructure.Persistence.Seeds.Visitor;
using _116.Identity.Infrastructure.Repositories;
using _116.Identity.Infrastructure.Services;
using _116.Shared.Application.Configurations;
using _116.Shared.Application.Exceptions.Handlers.Contracts;
using _116.Shared.Infrastructure;
using Mapster;
using MapsterMapper;
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
    private static ModuleOptions<IdentityDbContext> GetModuleOptions()
    {
        // Disable migrations and seeding in Testing environment (tests use InMemory database)
        string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        bool enableSeeding = !environment.Equals("Testing", StringComparison.OrdinalIgnoreCase);

        return new ModuleOptions<IdentityDbContext>
        {
            ModuleName = IdentityConstants.ModuleName,
            SchemaName = IdentityConstants.SchemaName,
            EnableMigrations = enableSeeding,
            EnableSeeding = enableSeeding,
        };
    }

    /// <summary>
    /// Adds the Identity module's services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to register services into.</param>
    /// <returns>The updated <see cref="IServiceCollection" /> for chaining.</returns>
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
        services.AddModuleDatabase(GetModuleOptions());

        // Register Mapster configuration and IMapper (thread-safe, no global state)
        TypeAdapterConfig mappingConfig = MappingRegistration.CreateConfiguration();
        services.AddSingleton(mappingConfig);
        services.AddScoped<IMapper>(sp => new Mapper(sp.GetRequiredService<TypeAdapterConfig>()));

        services.AddHttpContextAccessor();
        services.AddDetection();
        services.AddScoped<IIdentityUnitOfWork, IdentityUnitOfWork>();

        // Register adapters
        services.AddScoped<IClientOriginDetectionAdapter, WangkanaiClientOriginDetectionAdapter>();

        // Register user management services
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();
        services.AddScoped<IUserRoleRepository, UserRoleRepository>();
        services.AddScoped<IOtpService, OtpService>();
        services.AddScoped<IOtpRepository, OtpRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<ISessionMetadataService, SessionMetadataService>();
        services.AddScoped<ISessionExportService, SessionExportService>();

        // Register authentication factories
        services.AddScoped<ISessionFactory, SessionFactory>();
        services.AddScoped<IPublicSignUpAuthFactory, PublicSignUpAuthFactory>();
        services.AddScoped<IAdminLoginAuthFactory, AdminLoginAuthFactory>();
        services.AddScoped<IPublicLoginAuthFactory, PublicLoginAuthFactory>();
        services.AddScoped<IPublicSocialLoginAuthFactory, PublicSocialLoginAuthFactory>();
        services.AddScoped<IPublicUpdateProfileAuthFactory, PublicUpdateProfileAuthFactory>();
        services.AddScoped<IAdminUpdateProfileAuthFactory, AdminUpdateProfileAuthFactory>();
        services.AddScoped<IPublicUpdateAvatarAuthFactory, PublicUpdateAvatarAuthFactory>();
        services.AddScoped<IAdminUpdateAvatarAuthFactory, AdminUpdateAvatarAuthFactory>();
        services.AddScoped<IPublicResetPasswordAuthFactory, PublicResetPasswordAuthFactory>();
        services.AddScoped<IAdminResetPasswordAuthFactory, AdminResetPasswordAuthFactory>();
        services.AddScoped<IPublicForgotPasswordOtpFactory, PublicForgotPasswordOtpFactory>();
        services.AddScoped<IAdminForgotPasswordOtpFactory, AdminForgotPasswordOtpFactory>();
        services.AddScoped<IPublicResendOtpFactory, PublicResendOtpFactory>();
        services.AddScoped<IAdminResendOtpFactory, AdminResendOtpFactory>();
        services.AddScoped<IPublicSignOutSessionFactory, PublicSignOutSessionFactory>();
        services.AddScoped<IAdminSignOutSessionFactory, AdminSignOutSessionFactory>();
        services.AddScoped<IPublicRefreshTokenFactory, PublicRefreshTokenFactory>();

        services.AddScoped<SuperAdminSeeder>();
        services.AddScoped<VisitorRoleSeeder>();

        var (secret, issuer, audience, _, _) = AppEnvironment.Jwt();
        services
            .AddAuthentication(defaultScheme: JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
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
                    ClockSkew = TimeSpan.Zero,
                };

                options.ConfigureJwtBearerEvents();
            });

        services.AddIdentityModuleAuthorization();

        // Register custom exception handlers for this module
        services.AddSingleton<IExceptionStrategy, AccountInactiveExceptionHandler>();
        services.AddSingleton<IExceptionStrategy, AccountNotVerifiedExceptionHandler>();
        services.AddSingleton<IExceptionStrategy, OtpAttemptsLimitExceptionHandler>();
        services.AddSingleton<IExceptionStrategy, OtpExpirationExceptionHandler>();
        services.AddSingleton<IExceptionStrategy, AccessDeniedExceptionHandler>();
        services.AddSingleton<IExceptionStrategy, AccessTokenExpiryExceptionHandler>();
        services.AddSingleton<IExceptionStrategy, RefreshTokenExpiryExceptionHandler>();

        return services;
    }

    /// <summary>
    /// Configures the Identity module's middleware in the application pipeline.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The updated <see cref="IApplicationBuilder" /> for chaining.</returns>
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
        ModuleOptions<IdentityDbContext> options = GetModuleOptions();
        app.UseModuleDatabase(options);

        if (!options.EnableSeeding)
        {
            return app;
        }

        using IServiceScope scope = app.ApplicationServices.CreateScope();
        scope.ServiceProvider.GetRequiredService<SuperAdminSeeder>().SeedAllAsync().GetAwaiter().GetResult();
        scope.ServiceProvider.GetRequiredService<VisitorRoleSeeder>().SeedAllAsync().GetAwaiter().GetResult();

        return app;
    }
}
