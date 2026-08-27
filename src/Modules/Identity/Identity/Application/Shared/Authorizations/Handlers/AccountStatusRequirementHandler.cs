using System.Security.Claims;
using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Shared.Authorizations.Requirements;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Npgsql;

namespace _116.Identity.Application.Shared.Authorizations.Handlers;

/// <summary>
/// Authorization handler that validates account status requirements against user data.
/// </summary>
/// <remarks>
/// Resolves the user once per request — later policy evaluations reuse the entity cached in
/// <c>HttpContext.Items</c>. A database outage fails the requirement rather than trusting
/// possibly stale token claims.
/// </remarks>
public class AccountStatusRequirementHandler(IAuthRepository authRepository, IHttpContextAccessor httpContextAccessor)
    : AuthorizationHandler<AccountStatusRequirement>
{
    private const string AccountStatusItemKey = "account-status";

    /// <summary>
    /// Evaluates the account status requirement against the current authorization context.
    /// Checks the database first for user status, with JWT claims as fallback for connectivity errors.
    /// </summary>
    /// <param name="context">The authorization context containing user claims and authorization state.</param>
    /// <param name="requirement">
    /// The account status requirements specifying that claim type and expected value to validate.
    /// </param>
    /// <returns>A task representing the asynchronous authorization evaluation operation.</returns>
    /// <remarks>
    /// The user's status is read from the database for real-time accuracy and cached on the
    /// request, so several policies in one authorization pass cost a single query.
    /// Other exceptions (like validation errors) are allowed to bubble up to provide proper user feedback.
    /// </remarks>
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AccountStatusRequirement requirement
    )
    {
        // Extract user ID from JWT token claims
        string? userIdClaim = context.User.FindFirst(type: ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(value: userIdClaim) || !Guid.TryParse(input: userIdClaim, out Guid userId))
        {
            return;
        }

        try
        {
            UserEntity? user = await ResolveUserAsync(userId: userId);
            if (user is not null && CheckRequirementAgainstUser(user: user, requirement: requirement))
            {
                context.Succeed(requirement: requirement);
            }
        }
        catch (Exception ex) when (IsDbConnectivityError(exception: ex))
        {
            // Token claims may be stale (deactivation, deletion), so an unverifiable status
            // fails closed instead of trusting them.
            context.Fail(
                new AuthorizationFailureReason(handler: this, message: "Account status could not be verified.")
            );
        }
    }

    /// <summary>
    /// Loads the user once per request; later policy evaluations reuse the cached entity.
    /// </summary>
    /// <param name="userId">The authenticated user's identifier.</param>
    /// <returns>The user, or null when none exists.</returns>
    private async Task<UserEntity?> ResolveUserAsync(Guid userId)
    {
        HttpContext? httpContext = httpContextAccessor.HttpContext;
        if (httpContext?.Items[AccountStatusItemKey] is UserEntity cached && cached.Id == userId)
        {
            return cached;
        }

        UserEntity? user = await authRepository.FindUserByIdOrThrow(userId: userId);
        if (user is not null && httpContext is not null)
        {
            httpContext.Items[AccountStatusItemKey] = user;
        }

        return user;
    }

    /// <summary>
    /// Maps the requirement to the correct user property and checks if it matches the expected value.
    /// </summary>
    /// <param name="user">The user entity containing the account status properties to validate.</param>
    /// <param name="requirement">The account status requirement containing the claim type and expected value.</param>
    /// <returns>
    /// <c>true</c> if the user's account status matches the requirement's expected value;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method maps JWT claim types (IsVerified, IsActive) to their corresponding
    /// user entity properties and performs boolean comparison with the requirement's expected value.
    /// Returns <c>false</c> for unknown claim types or invalid requirement values.
    /// </remarks>
    private static bool CheckRequirementAgainstUser(UserEntity user, AccountStatusRequirement requirement)
    {
        bool actualValue = requirement.ClaimType switch
        {
            JwtClaimsConstants.IsVerified => user.IsVerified,
            JwtClaimsConstants.IsActive => user.IsActive,
            _ => false,
        };

        // Compare with expected requirement value
        if (bool.TryParse(value: requirement.ClaimValue, out bool expectedValue))
        {
            return actualValue == expectedValue;
        }

        return false;
    }

    /// <summary>
    /// Determines if the exception is a database connectivity error that should trigger JWT fallback.
    /// </summary>
    /// <param name="exception">The exception to check for connectivity issues.</param>
    /// <returns><c>true</c> if it's a connectivity error; otherwise, <c>false</c>.</returns>
    private static bool IsDbConnectivityError(Exception exception)
    {
        return exception switch
        {
            // Network/connection timeouts
            TimeoutException => true,
            TaskCanceledException => true,
            OperationCanceledException => true,
            // PostgresQL-specific connectivity errors
            NpgsqlException npgsqlEx => npgsqlEx.SqlState switch
            {
                "08000" => true, // connection_exception
                "08003" => true, // connection_does_not_exist
                "08006" => true, // connection_failure
                "08001" => true, // sqlclient_unable_to_establish_sqlconnection
                "08004" => true, // sqlserver_rejected_establishment_of_sqlconnection
                "08007" => true, // transaction_resolution_unknown
                "57P01" => true, // admin_shutdown
                "57P02" => true, // crash_shutdown
                "57P03" => true, // cannot_connect_now
                _ => false,
            },
            _ => false,
        };
    }
}
