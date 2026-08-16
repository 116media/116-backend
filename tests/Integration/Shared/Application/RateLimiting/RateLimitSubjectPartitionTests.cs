namespace _116.Integration.Tests.Shared.Application.RateLimiting;

/// <summary>
/// Verifies the rate limiter partitions an authenticated caller by subject rather than by IP. On the
/// real-limiter host the limiter runs after authentication, so an authenticated request resolves
/// <c>RateLimitPartitioning</c>'s <c>user:{subject}</c> branch — a bucket separate from the anonymous
/// per-IP buckets the policy theory exhausts, so a single call is never rejected.
/// </summary>
/// <param name="db">The dedicated database and rate-limited application host.</param>
[Collection("RateLimiting")]
public class RateLimitSubjectPartitionTests(RateLimitedPostgresFixture db) : IDisposable
{
    private readonly HttpClient _client = db.Api.CreateClient();

    /// <inheritdoc />
    public void Dispose()
    {
        _client.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task AuthenticatedRequest_IsPartitionedBySubject_AndNotRejectedOnFirstCall()
    {
        _client.AuthenticateAs(TestUser.VisitorId, "Visitor");

        using HttpResponseMessage response = await _client.GetAsync(Routes.Public.Me.Profile());

        // The limiter ran with the authenticated subject (a fresh user:{id} bucket), so the first call
        // is never a 429 — regardless of the endpoint's own outcome once authorization runs.
        response.StatusCode.Should().NotBe(HttpStatusCode.TooManyRequests);
    }
}
