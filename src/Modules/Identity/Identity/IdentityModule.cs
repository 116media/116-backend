using System.Text;
using _116.Identity.Application.Adapters.SocialAuth;
using _116.Identity.Application.Adapters.Wangkanai.Detection;
using _116.Identity.Application.Auth.EventHandlers;
using _116.Identity.Application.Auth.Exceptions.Handlers;
using _116.Identity.Application.Auth.Factories;
using _116.Identity.Application.Auth.Factories.Contracts;
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
using _116.Identity.Application.Session.Cache;
using _116.Identity.Application.Session.EventHandlers;
using _116.Identity.Application.Session.Factories;
using _116.Identity.Application.Session.Factories.Contracts;
using _116.Identity.Application.Session.Repositories;
using _116.Identity.Application.Session.Services;
using _116.Identity.Application.Shared.Authorizations.Extensions;
using _116.Identity.Application.Shared.Cache;
using _116.Identity.Application.Shared.Errors;
using _116.Identity.Application.Shared.Errors.Facade;
using _116.Identity.Application.Shared.Errors.Messages;
using _116.Identity.Application.Shared.EventHandlers;
using _116.Identity.Application.Shared.Exceptions.Handlers;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Application.User.EventHandlers;
using _116.Identity.Application.User.Services;
using _116.Identity.Application.User.UseCases.Admin.Commands.UpdateAvatar;
using _116.Identity.Application.User.UseCases.Admin.Commands.UpdateAvatar.Contracts;
using _116.Identity.Application.User.UseCases.Admin.Commands.UpdateOwnProfile;
using _116.Identity.Application.User.UseCases.Admin.Commands.UpdateOwnProfile.Contracts;
using _116.Identity.Application.User.UseCases.Public.Commands.UpdateAvatar;
using _116.Identity.Application.User.UseCases.Public.Commands.UpdateAvatar.Contracts;
using _116.Identity.Application.User.UseCases.Public.Commands.UpdateOwnProfile;
using _116.Identity.Application.User.UseCases.Public.Commands.UpdateOwnProfile.Contracts;
using _116.Identity.Contracts.Application.Services;
using _116.Identity.Domain.Constants;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.Events;
using _116.Identity.Infrastructure.Adapters.SocialAuth;
using _116.Identity.Infrastructure.Adapters.Wangkanai.Detection;
using _116.Identity.Infrastructure.BackgroundJobs;
using _116.Identity.Infrastructure.Cache;
using _116.Identity.Infrastructure.Persistence;
using _116.Identity.Infrastructure.Persistence.Seeds.SuperAdmin;
using _116.Identity.Infrastructure.Persistence.Seeds.Visitor;
using _116.Identity.Infrastructure.Repositories;
using _116.Identity.Infrastructure.Services;
using _116.Shared.Application.Configurations.Schemas;
using _116.Shared.Application.Exceptions.Handlers.Contracts;
using _116.Shared.Application.Extensions;
using _116.Shared.Application.Services;
using _116.Shared.Infrastructure;
using _116.Shared.Infrastructure.Seed;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace _116.Identity;

/// <summary>
/// Provides extension methods to register and configure the Identity module's services and middleware.
/// </summary>
public static class IdentityModule
{
    /// <summary>
    /// Gets the shared module configuration options for the Identity module.
    /// Migrations and seeding run in every environment except Testing.
    /// </summary>
    /// <param name="environment">The host environment the options are derived from.</param>
    /// <returns>The module options for the supplied environment.</returns>
    private static ModuleOptions<IdentityDbContext> GetModuleOptions()
    {
        return new ModuleOptions<IdentityDbContext>
        {
            ModuleName = IdentityConstants.ModuleName,
            SchemaName = IdentityConstants.SchemaName,
        };
    }

    /// <summary>
    /// Adds the Identity module's services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to register services into.</param>
    /// <param name="environment">The host environment deciding whether the module migrates and seeds.</param>
    /// <returns>The updated <see cref="IServiceCollection" /> for chaining.</returns>
    /// <example>
    /// <code>
    /// builder.Services.AddIdentityModule(builder.Environment);
    /// </code>
    /// </example>
    public static IServiceCollection AddIdentityModule(this IServiceCollection services, IHostEnvironment environment)
    {
        services.AddModuleDatabase(GetModuleOptions());

        // Register error message classes (IStringLocalizer-backed)
        services.AddScoped<ValidationErrorMessage>();
        services.AddScoped<AuthenticationErrorMessage>();
        services.AddScoped<AuthorizationErrorMessage>();
        services.AddScoped<ConflictErrorMessage>();

        // Register error factory classes
        services.AddScoped<UserErrors>();
        services.AddScoped<SessionErrors>();
        services.AddScoped<IdentityI18n>();

        // Contribute Identity mappings to the shared cross-module Mapster config
        services.AddModuleMappings(new MappingRegistration());

        services.AddHttpContextAccessor();
        services.AddDetection();
        services.AddScoped<IIdentityUnitOfWork, IdentityUnitOfWork>();
        services.AddScoped(typeof(IIdentityRepository<>), typeof(IdentityRepository<>));

        // Replay delivers events raised inside a transaction, not just retries failed dispatches.
        services.AddScheduledJob<IdentityOutboxReplayJob>(cronExpression: "0 */1 * * * ?");

        // Register adapters
        services.AddScoped<IClientOriginDetectionAdapter, WangkanaiClientOriginDetectionAdapter>();

        // Register user management services
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<IClaimsProvider, AuthRepository>();
        services.AddScoped<IUserLookupService, UserLookupService>();
        services.AddScoped<IAvatarService, AvatarService>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IOtpService>(sp => new OtpService(
            pepper: SecurityEnv.OtpPepper.Value,
            timeProvider: sp.GetRequiredService<TimeProvider>()
        ));
        services.AddScoped<IOtpRepository, OtpRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<IUserTokenStateRepository, UserTokenStateRepository>();
        services.AddScoped<IAccountLockoutRepository, AccountLockoutRepository>();
        services.AddScoped<ISessionMetadataService, SessionMetadataService>();

        services.AddSingleton<ISessionRevocationCache, SessionRevocationCache>();
        services.AddSingleton<IUserSecurityStateCache, UserSecurityStateCache>();
        services.AddScoped<ITokenDeliveryService, TokenDeliveryService>();
        services.AddScoped<ISessionExportService, SessionExportService>();

        // Register authentication factories
        services.AddScoped<ISessionFactory, SessionFactory>();
        services.AddScoped<IOtpVerificationFactory, OtpVerificationFactory>();
        services.AddScoped<IPublicSignUpAuthFactory, PublicSignUpAuthFactory>();
        services.AddScoped<IAdminLoginAuthFactory, AdminLoginAuthFactory>();
        services.AddScoped<IPublicLoginAuthFactory, PublicLoginAuthFactory>();
        services.AddScoped<IPublicSocialLoginAuthFactory, PublicSocialLoginAuthFactory>();

        // Social-login token verification: keyed adapters resolved through the factory.
        services.Configure<SocialAuthOptions>(options =>
        {
            options.GoogleClientId = SocialAuthEnv.GoogleClientId.Value;
            options.FacebookAppId = SocialAuthEnv.FacebookAppId.Value;
            options.FacebookAppSecret = SocialAuthEnv.FacebookAppSecret.Value;
        });

        services.AddKeyedScoped<ISocialTokenVerifier, GoogleTokenVerifier>(EnumAuthProvider.Google);
        services.AddHttpClient<FacebookTokenVerifier>(client =>
        {
            client.BaseAddress = new Uri(SocialAuthConstants.FacebookGraphBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(10);
        });
        services.AddKeyedScoped<ISocialTokenVerifier>(
            EnumAuthProvider.Facebook,
            (sp, _) => sp.GetRequiredService<FacebookTokenVerifier>()
        );
        services.AddScoped<ISocialTokenVerifierFactory, SocialTokenVerifierFactory>();
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
        services.AddScoped<IRefreshTokenFactory, RefreshTokenFactory>();

        services.AddScheduledJob<ExpiredOtpCleanupJob>(cronExpression: IdentityConstants.ExpiredOtpCleanupCron);

        services.AddScoped<SuperAdminSeeder>();
        services.AddScoped<VisitorRoleSeeder>();
        if (!environment.IsEnvironment("Testing"))
        {
            services.AddScoped<IDataSeeder>(sp => sp.GetRequiredService<SuperAdminSeeder>());
            services.AddScoped<IDataSeeder>(sp => sp.GetRequiredService<VisitorRoleSeeder>());
        }

        // Register domain event handlers: welcome and security notifications
        services.AddScoped<IDomainEventHandler<UserVerifiedEvent>, UserVerifiedWelcomeEmailHandler>();
        services.AddScoped<IDomainEventHandler<OtpIssuedEvent>, OtpIssuedEmailHandler>();
        services.AddScoped<IDomainEventHandler<UserPasswordChangedEvent>, UserPasswordChangedNotificationsHandler>();
        services.AddScoped<IDomainEventHandler<UserEmailChangedEvent>, UserEmailChangedNotificationsHandler>();
        services.AddScoped<IDomainEventHandler<UserRoleGrantedEvent>, UserRoleGrantedNotificationsHandler>();
        services.AddScoped<IDomainEventHandler<UserRoleRevokedEvent>, UserRoleRevokedNotificationsHandler>();
        services.AddScoped<
            IDomainEventHandler<UserSignedOutAllDevicesEvent>,
            UserSignedOutAllDevicesNotificationsHandler
        >();

        // Register domain event handlers
        services.AddScoped<IDomainEventHandler<RefreshTokenReplayDetectedEvent>, RefreshTokenReplaySecurityHandler>();
        services.AddScoped<IDomainEventHandler<UserDeactivatedEvent>, UserDeactivatedSecurityHandler>();
        services.AddScoped<IDomainEventHandler<SessionRevokedEvent>, SessionRevokedLogHandler>();
        services.AddScoped<IDomainEventHandler<RoleChangedEvent>, IdentityLookupCacheHandler>();
        services.AddScoped<IDomainEventHandler<PermissionChangedEvent>, IdentityLookupCacheHandler>();

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
                    ValidIssuer = JwtEnv.Issuer.Value,
                    ValidAudience = JwtEnv.Audience.Value,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtEnv.Secret.Value)),
                    ClockSkew = TimeSpan.Zero,
                };

                options.ConfigureJwtBearerEvents();
            });

        services.AddIdentityModuleAuthorization();

        // Register custom exception handlers for this module
        services.AddSingleton<IExceptionStrategy, AccountInactiveExceptionHandler>();
        services.AddSingleton<IExceptionStrategy, AccountNotVerifiedExceptionHandler>();
        services.AddSingleton<IExceptionStrategy, DomainRuleExceptionStrategy>();
        services.AddSingleton<IExceptionStrategy, OtpAttemptsLimitExceptionHandler>();
        services.AddSingleton<IExceptionStrategy, OtpExpirationExceptionHandler>();
        services.AddSingleton<IExceptionStrategy, SocialTokenVerificationExceptionHandler>();
        services.AddSingleton<IExceptionStrategy, UnsupportedProviderExceptionHandler>();
        services.AddSingleton<IExceptionStrategy, AccessDeniedExceptionHandler>();
        services.AddSingleton<IExceptionStrategy, AccessTokenExpiryExceptionHandler>();
        services.AddSingleton<IExceptionStrategy, RefreshTokenExpiryExceptionHandler>();

        return services;
    }
}
