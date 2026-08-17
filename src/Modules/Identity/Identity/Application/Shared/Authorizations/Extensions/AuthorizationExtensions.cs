using System.Security.Claims;
using System.Text.Json;
using _116.BuildingBlocks.Constants;
using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.Identity.Application.Auth.Constants;
using _116.Identity.Application.Session.Cache;
using _116.Identity.Application.Shared.Authorizations.Configuration;
using _116.Identity.Application.Shared.Authorizations.Handlers;
using _116.Identity.Application.Shared.Authorizations.Requirements;
using _116.Identity.Application.Shared.Cache;
using _116.Identity.Application.Shared.Errors.Messages;
using _116.Identity.Application.Shared.Exceptions;
using _116.Identity.Application.Shared.Exceptions.Handlers;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Handlers.Strategies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace _116.Identity.Application.Shared.Authorizations.Extensions;

/// <summary>
/// Provides extension methods for configuring authorization policies and requirements.
/// </summary>
/// <remarks>
/// Centralizes authorization configuration to promote consistency and reusability across modules.
/// Follows the modular monolithic architecture pattern by encapsulating authorization concerns
/// within the Identity module while providing extensibility for other modules.
/// </remarks>
public static class AuthorizationExtensions
{
    /// <summary>
    /// Cached JsonSerializerOptions for consistent JSON serialization across JWT events.
    /// </summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    /// <summary>
    /// Configures authorization policies and handlers for the Auth module.
    /// </summary>
    /// <param name="services">The service collection to add authorization services to</param>
    /// <returns>The service collection for method chaining</returns>
    /// <remarks>
    /// This method encapsulates the complete authorization setup including:
    /// <list type="bullet">
    ///     <item>Registration of authorization requirement handlers</item>
    ///     <item>Configuration of account status policies</item>
    ///     <item>Configuration of user role policies</item>
    /// </list>
    /// The configuration is data-driven and easily extensible for new policy requirements.
    /// </remarks>
    public static IServiceCollection AddIdentityModuleAuthorization(this IServiceCollection services)
    {
        // Register authorization requirement handlers
        services.AddScoped<IAuthorizationHandler, UserRoleRequirementHandler>();
        services.AddScoped<IAuthorizationHandler, AccountStatusRequirementHandler>();
        // Configure authorization policies
        services.ConfigureAuthorizationPolicies();
        return services;
    }

    /// <summary>
    /// Configures authorization policies using a centralized, data-driven approach.
    /// </summary>
    /// <param name="services">The service collection to add policies to</param>
    /// <returns>The service collection for method chaining</returns>
    private static IServiceCollection ConfigureAuthorizationPolicies(this IServiceCollection services)
    {
        AuthorizationBuilder authBuilder = services.AddAuthorizationBuilder();
        // Configure policies using centralized configuration
        AuthorizationConfiguration policyConfiguration = AuthorizationPolicyConfiguration.GetConfiguration();
        authBuilder.ConfigureAccountStatusPolicies(policies: policyConfiguration.AccountStatusPolicies);
        authBuilder.ConfigureUserRolePolicies(policies: policyConfiguration.UserRolePolicies);
        return services;
    }

    /// <summary>
    /// Configures account status policies using requirements.
    /// </summary>
    /// <param name="authBuilder">The authorization builder</param>
    /// <param name="policies">Dictionary of policy configurations</param>
    /// <returns>The authorization builder for method chaining</returns>
    private static AuthorizationBuilder ConfigureAccountStatusPolicies(
        this AuthorizationBuilder authBuilder,
        Dictionary<string, (string ClaimType, string ClaimValue)> policies
    )
    {
        foreach (var (policyName, (claimType, claimValue)) in policies)
        {
            authBuilder.AddPolicy(
                name: policyName,
                policy =>
                    policy.Requirements.Add(new AccountStatusRequirement(claimType: claimType, claimValue: claimValue))
            );
        }

        return authBuilder;
    }

    /// <summary>
    /// Configures user role policies using requirements. The Visitor policy additionally
    /// requires the <c>is_active</c> and <c>is_verified</c> claims.
    /// </summary>
    /// <param name="authBuilder">The authorization builder</param>
    /// <param name="policies">Dictionary of policy configurations</param>
    /// <returns>The authorization builder for method chaining</returns>
    private static AuthorizationBuilder ConfigureUserRolePolicies(
        this AuthorizationBuilder authBuilder,
        Dictionary<string, string[]> policies
    )
    {
        foreach (var (policyName, roles) in policies)
        {
            authBuilder.AddPolicy(
                name: policyName,
                policy =>
                {
                    policy.Requirements.Add(new UserRoleRequirement(allowedRoles: roles));

                    if (policyName == UserRolePolicies.RequireVisitorOnly)
                    {
                        policy.RequireClaim(JwtClaimsConstants.IsActive, "true");
                        policy.RequireClaim(JwtClaimsConstants.IsVerified, "true");
                    }
                }
            );
        }

        return authBuilder;
    }

    /// <summary>
    /// Configures JWT Bearer events for consistent authentication and authorization error handling.
    /// </summary>
    /// <param name="options">The JWT Bearer options to configure</param>
    /// <remarks>
    /// Leverages the existing exception handling system for consistent error responses.
    /// Uses AuthenticationExceptionHandler and AuthorizationExceptionHandler for standardized ProblemDetails.
    /// </remarks>
    public static void ConfigureJwtBearerEvents(this JwtBearerOptions options)
    {
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (string.IsNullOrEmpty(context.Token))
                {
                    context.Token = context.Request.Cookies[TokenCookieConstants.AccessTokenCookie];
                }

                return Task.CompletedTask;
            },
            OnTokenValidated = async context =>
            {
                ClaimsPrincipal? principal = context.Principal;

                if (
                    !Guid.TryParse(principal?.FindFirst(JwtClaimsConstants.SessionId)?.Value, out Guid sessionId)
                    || !Guid.TryParse(principal.FindFirst(ClaimTypes.NameIdentifier)?.Value, out Guid userId)
                )
                {
                    return;
                }

                if (
                    !Guid.TryParse(principal.FindFirst(JwtClaimsConstants.SecurityStamp)?.Value, out Guid tokenStamp)
                    || !long.TryParse(
                        principal.FindFirst(JwtClaimsConstants.TokenVersion)?.Value,
                        out long tokenVersion
                    )
                )
                {
                    context.Fail("The token is missing its security markers.");
                    return;
                }

                IServiceProvider services = context.HttpContext.RequestServices;

                if (services.GetRequiredService<ISessionRevocationCache>().IsRevoked(sessionId: sessionId))
                {
                    context.Fail("The session has been revoked.");
                    return;
                }

                UserSecurityState current = await services
                    .GetRequiredService<IUserSecurityStateCache>()
                    .GetAsync(userId: userId, cancellationToken: context.HttpContext.RequestAborted);

                if (tokenStamp != current.SecurityStamp)
                {
                    context.Fail("Credentials changed; re-authentication required.");
                    return;
                }

                if (tokenVersion < current.TokenVersion)
                {
                    context.Fail("Permissions changed; token refresh required.");
                }
            },
            OnChallenge = async context =>
            {
                // Prevent default challenge response
                context.HandleResponse();

                // Create an AuthenticationException and use the existing handler
                var authMsg = context.HttpContext.RequestServices.GetRequiredService<AuthenticationErrorMessage>();
                var authHandler = new AccessTokenExpiryExceptionHandler();
                var authException = new AccessTokenExpiryException(authMsg.JwtTokenRequired());
                ProblemDetails problemDetails = authHandler.CreateProblemDetails(
                    exception: authException,
                    context: context.HttpContext
                );
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/problem+json";
                await context.Response.WriteAsync(
                    JsonSerializer.Serialize(value: problemDetails, options: JsonOptions)
                );
            },
            OnForbidden = async context =>
            {
                // Create an AuthorizationException and use the existing handler
                var authzMsg = context.HttpContext.RequestServices.GetRequiredService<AuthorizationErrorMessage>();
                var authHandler = new AuthorizationExceptionHandler();
                var authException = new AuthorizationException(authzMsg.AccessDenied());
                ProblemDetails problemDetails = authHandler.CreateProblemDetails(
                    exception: authException,
                    context: context.HttpContext
                );
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/problem+json";
                await context.Response.WriteAsync(
                    JsonSerializer.Serialize(value: problemDetails, options: JsonOptions)
                );
            },
        };
    }
}
