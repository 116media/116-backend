using System.Net;
using System.Security.Claims;
using _116.Shared.Application.Builders.RateLimit;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace _116.Unit.Tests.Shared.Application.Builders.RateLimit;

/// <summary>
/// Unit tests for <see cref="RateLimitPartitioning.ResolvePartitionKey"/>. An authenticated caller is
/// keyed by subject; an anonymous caller by client IP.
/// </summary>
public class RateLimitPartitioningTests
{
    [Fact]
    public void ResolvePartitionKey_WithSubjectClaim_KeysByUser()
    {
        // Arrange
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "user-123")], "test")),
        };

        // Act
        string key = RateLimitPartitioning.ResolvePartitionKey(context);

        // Assert
        key.Should().Be("user:user-123");
    }

    [Fact]
    public void ResolvePartitionKey_WhenAnonymous_KeysByClientIp()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse("203.0.113.7");

        // Act
        string key = RateLimitPartitioning.ResolvePartitionKey(context);

        // Assert
        key.Should().Be("ip:203.0.113.7");
    }

    [Fact]
    public void ResolvePartitionKey_WhenAnonymousWithNoIp_KeysByAnonymous()
    {
        // Arrange — no principal and no remote IP
        var context = new DefaultHttpContext();

        // Act
        string key = RateLimitPartitioning.ResolvePartitionKey(context);

        // Assert
        key.Should().Be("ip:anonymous");
    }
}
