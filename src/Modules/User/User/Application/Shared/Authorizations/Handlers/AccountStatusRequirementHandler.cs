using _116.BuildingBlocks.Constants;
using _116.User.Application.Shared.Authorizations.Requirements;
using _116.User.Application.Shared.Repositories;
using Microsoft.AspNetCore.Authorization;
using Npgsql;
using System.Security.Claims;
using _116.User.Domain.Entities;

namespace _116.User.Application.Shared.Authorizations.Handlers;

/// <summary>
/// Authorization handler that validates account status requirements against user data.
/// </summary>
/// <remarks>
/// Checks user account status from the database first, with JWT token claims as fallback for DB errors.
/// Used for enforcing account status policies like verification, active status, etc.
/// </remarks>
public class AccountStatusRequirementHandler(IUserRepository userRepository)
    : AuthorizationHandler<AccountStatusRequirement>
{
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
    /// This method first attempts to validate the user's status from the database for real-time accuracy.
    /// If database connectivity issues occur, it falls back to validating JWT claims.
    /// Other exceptions (like validation errors) are allowed to bubble up to provide proper user feedback.
    /// </remarks>
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AccountStatusRequirement requirement
    )
    {
        // Extract user ID from JWT token claims
        string? userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
        {
            return;
        }

        try
        {
            UserEntity? user = await userRepository.GetUserByIdAsync(userId);
            if (user is not null && CheckRequirementAgainstUser(user, requirement))
            {
                context.Succeed(requirement);
            }
        }
        catch (Exception ex) when (IsDbConnectivityError(ex))
        {
            // Fallback to JWT claims for database connectivity errors
            string? claimValue = context.User.FindFirst(requirement.ClaimType)?.Value;
            if (
                !string.IsNullOrEmpty(claimValue) &&
                claimValue.Equals(requirement.ClaimValue, StringComparison.OrdinalIgnoreCase)
            )
            {
                context.Succeed(requirement);
            }
        }
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
    /// This method maps JWT claim types (IsVerified, IsActive, IsLoggedIn) to their corresponding
    /// user entity properties and performs boolean comparison with the requirement's expected value.
    /// Returns <c>false</c> for unknown claim types or invalid requirement values.
    /// </remarks>
    private static bool CheckRequirementAgainstUser(UserEntity user, AccountStatusRequirement requirement)
    {
        bool actualValue = requirement.ClaimType switch
        {
            JwtClaimsConstants.IsVerified => user.IsVerified,
            JwtClaimsConstants.IsActive => user.IsActive,
            JwtClaimsConstants.IsLoggedIn => user.IsLoggedIn,
            _ => false
        };

        // Compare with expected requirement value
        if (bool.TryParse(requirement.ClaimValue, out bool expectedValue))
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
                _ => false
            },

            _ => false
        };
    }
}
