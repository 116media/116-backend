using System.Text.Json;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.User.UseCases.Public.Queries.GetOwnProfile.V1;

/// <summary>
/// Integration tests for the PublicGetOwnProfile endpoint.
/// </summary>
[Collection("Database")]
public class PublicGetOwnProfileEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private const string AdminMeProfile = $"{ApiRoutes.Admin.Base}/me/profile";
    private const string AdminMeAvatar = $"{ApiRoutes.Admin.Base}/me/avatar";
    private const string PublicMeProfile = $"{ApiRoutes.Public.Me}/profile";
    private const string PublicMeAvatar = $"{ApiRoutes.Public.Me}/avatar";

    [Fact]
    public async Task PublicGetOwnProfile_AsVisitor_Returns200()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync(PublicMeProfile);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        doc.RootElement.TryGetProperty("user", out var userProp).Should().BeTrue();
        userProp.GetProperty("id").GetString().Should().Be(TestUser.VisitorId.ToString());
    }

    [Fact]
    public async Task PublicGetOwnProfile_WithNoAuth_Returns401()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync(PublicMeProfile);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PublicGetOwnProfile_AsAdmin_Returns403()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.GetAsync(PublicMeProfile);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
