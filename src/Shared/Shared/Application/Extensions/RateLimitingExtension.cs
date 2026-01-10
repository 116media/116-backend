using System.Threading.RateLimiting;
using _116.BuildingBlocks.Constants;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Shared.Application.Builders.RateLimit;
using _116.Shared.Application.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;

namespace _116.Shared.Application.Extensions;

/// <summary>
/// Extension methods for configuring rate limiting using a hybrid three-tier strategy:
/// - Sliding Window: security-critical endpoints
/// - Token Bucket: resource-intensive endpoints
/// - Fixed Window: general-access endpoints
/// </summary>
public static class RateLimitingExtension
{
    /// <summary>
    /// Registers rate limiting middleware with all configured policies.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = OnRateLimitRejected;

            ConfigureSlidingWindowPolicies(options);
            ConfigureTokenBucketPolicies(options);
            ConfigureFixedWindowPolicies(options);
        });

        return services;
    }

    /// <summary>
    /// Configures Sliding Window rate limiters for security-critical endpoints
    /// such as authentication and credential management.
    /// </summary>
    private static void ConfigureSlidingWindowPolicies(RateLimiterOptions options)
    {
        new SlidingWindowBuilder(options)
            .AddPolicy(
                RateLimitPolicies.Authentication,
                AuthenticationRateLimitConstants.PermitLimit,
                AuthenticationRateLimitConstants.WindowSeconds,
                AuthenticationRateLimitConstants.SegmentsPerWindow
            )
            .AddPolicy(
                RateLimitPolicies.Otp,
                OtpRateLimitConstants.PermitLimit,
                OtpRateLimitConstants.WindowSeconds,
                OtpRateLimitConstants.SegmentsPerWindow
            )
            .AddPolicy(
                RateLimitPolicies.PasswordManagement,
                PasswordManagementRateLimitConstants.PermitLimit,
                PasswordManagementRateLimitConstants.WindowSeconds,
                PasswordManagementRateLimitConstants.SegmentsPerWindow
            );
    }

    /// <summary>
    /// Configures Token Bucket rate limiters for expensive or burst-prone endpoints.
    /// </summary>
    private static void ConfigureTokenBucketPolicies(RateLimiterOptions options)
    {
        new TokenBucketBuilder(options)
            .AddPolicy(
                RateLimitPolicies.FileUpload,
                FileUploadRateLimitConstants.TokenLimit,
                FileUploadRateLimitConstants.TokensPerPeriod,
                FileUploadRateLimitConstants.ReplenishmentPeriodSeconds
            )
            .AddPolicy(
                RateLimitPolicies.DataExport,
                DataExportRateLimitConstants.TokenLimit,
                DataExportRateLimitConstants.TokensPerPeriod,
                DataExportRateLimitConstants.ReplenishmentPeriodSeconds
            );
    }

    /// <summary>
    /// Configures Fixed Window rate limiters for general access and read-heavy endpoints.
    /// </summary>
    private static void ConfigureFixedWindowPolicies(RateLimiterOptions options)
    {
        new FixedWindowBuilder(options)
            .AddPolicy(
                RateLimitPolicies.ContentBrowsing,
                ContentBrowsingRateLimitConstants.PermitLimit,
                ContentBrowsingRateLimitConstants.WindowSeconds
            )
            .AddPolicy(
                RateLimitPolicies.UserProfile,
                UserProfileRateLimitConstants.PermitLimit,
                UserProfileRateLimitConstants.WindowSeconds
            )
            .AddPolicy(
                RateLimitPolicies.SessionManagement,
                SessionManagementRateLimitConstants.PermitLimit,
                SessionManagementRateLimitConstants.WindowSeconds
            )
            .AddPolicy(
                RateLimitPolicies.AdminMetrics,
                AdminMetricsRateLimitConstants.PermitLimit,
                AdminMetricsRateLimitConstants.WindowSeconds
            );
    }

    /// <summary>
    /// Handles requests that exceed the rate limit by throwing a
    /// <see cref="RateLimitExceededException"/> handled by the global exception pipeline.
    /// </summary>
    private static ValueTask OnRateLimitRejected(OnRejectedContext context, CancellationToken cancellationToken)
    {
        TimeSpan retryAfter = TimeSpan.Zero;

        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retry))
        {
            retryAfter = retry;
        }

        throw new RateLimitExceededException(retryAfter);
    }
}
