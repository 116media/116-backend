using _116.Identity.Application.User.UseCases.Public.Queries.GetOwnProfile.V1;

namespace _116.Integration.Tests.Shared.Application.Versioning;

/// <summary>
/// Verifies that the header arm of the combined API version reader configured in
/// <c>Program.cs</c> is live, by asserting the two behaviours that distinguish a combined reader
/// from a URL-segment reader alone: a header agreeing with the URL segment resolves the endpoint,
/// and a header disagreeing with it is rejected rather than silently ignored.
/// </summary>
/// <remarks>
/// Every route is mapped under the <c>api/v{version:apiVersion}</c> group, so the header can
/// never be the sole source of a version. Its only observable effect is on a request that also
/// carries the URL segment, which is what these tests drive.
/// </remarks>
/// <param name="db">The shared Testcontainer database and application host.</param>
[Collection("Database")]
public class ApiVersionReaderTests(PostgresFixture db) : BaseApiTest(db)
{
    /// <summary>
    /// The header name <c>Program.cs</c> configures the <c>HeaderApiVersionReader</c> with.
    /// </summary>
    private const string VersionHeader = "X-Api-Version";

    /// <summary>
    /// The version the URL segment of every v1 route carries, in the same textual form the
    /// segment reader reports it. The combined reader compares raw values, so an agreeing header
    /// must match the segment character for character.
    /// </summary>
    private const string RequestedVersion = "1";

    /// <summary>
    /// A version no v1 endpoint is mapped to, used to make the header disagree with the segment.
    /// </summary>
    private const string ConflictingVersion = "2.0";

    [Fact]
    public async Task AgreeingVersionHeader_ResolvesTheSameEndpointAsTheUrlSegment()
    {
        Client.AuthenticateAsVisitor();

        using HttpResponseMessage response = await SendWithVersionHeaderAsync(RequestedVersion);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicGetOwnProfileResponse body = await response.ReadAsAsync<PublicGetOwnProfileResponse>();
        body.User.Id.Should().Be(TestUser.VisitorId);
    }

    [Fact]
    public async Task ConflictingVersionHeader_IsRejected()
    {
        Client.AuthenticateAsVisitor();

        using HttpResponseMessage response = await SendWithVersionHeaderAsync(ConflictingVersion);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Requests the visitor's own profile on the v1 URL segment, carrying the given version in
    /// the header the combined reader is configured with.
    /// </summary>
    /// <param name="version">The version to send in the header.</param>
    /// <returns>The endpoint's response.</returns>
    private async Task<HttpResponseMessage> SendWithVersionHeaderAsync(string version)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, Routes.Public.Me.Profile());
        request.Headers.Add(VersionHeader, version);

        return await Client.SendAsync(request);
    }
}
