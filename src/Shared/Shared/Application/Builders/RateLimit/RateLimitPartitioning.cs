using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace _116.Shared.Application.Builders.RateLimit;

/// <summary>
/// Resolves the partition key every rate-limit policy is bucketed by: the authenticated subject when
/// the request carries one, otherwise the client IP. Anonymous, pre-auth endpoints (login, OTP,
/// password reset) therefore partition by IP, so one caller can no longer drain a policy for everyone.
/// </summary>
public static class RateLimitPartitioning
{
    private const string AnonymousPartition = "anonymous";

    // The "sub" claim type, spelled out to avoid a JwtBearer package reference from Shared.
    private const string SubjectClaim = "sub";

    /// <summary>
    /// Returns a stable partition key for <paramref name="httpContext" />. Prefers the subject claim
    /// so an authenticated caller is limited across IPs; falls back to the connection's remote IP,
    /// which is the real client once forwarded headers are honoured. Requires the rate limiter to run
    /// after authentication — otherwise the principal is empty and every request keys by IP.
    /// </summary>
    /// <param name="httpContext">The request whose caller is being partitioned.</param>
    /// <returns>A partition key of the form <c>user:{subject}</c> or <c>ip:{address}</c>.</returns>
    public static string ResolvePartitionKey(HttpContext httpContext)
    {
        string? subject =
            httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? httpContext.User.FindFirstValue(SubjectClaim);

        if (!string.IsNullOrWhiteSpace(subject))
        {
            return $"user:{subject}";
        }

        string? ip = httpContext.Connection.RemoteIpAddress?.ToString();
        return $"ip:{ip ?? AnonymousPartition}";
    }
}
