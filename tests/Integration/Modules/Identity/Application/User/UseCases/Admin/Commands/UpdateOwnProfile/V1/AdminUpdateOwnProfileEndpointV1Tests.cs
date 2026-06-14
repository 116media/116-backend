using System.Text.Json;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.User.UseCases.Admin.Commands.UpdateOwnProfile.V1;

/// <summary>
/// Integration tests for the AdminUpdateOwnProfile endpoint.
/// </summary>
[Collection("Database")]
public class AdminUpdateOwnProfileEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private const string AdminMeProfile = $"{ApiRoutes.Admin.Base}/me/profile";
    private const string AdminMeAvatar = $"{ApiRoutes.Admin.Base}/me/avatar";
    private const string PublicMeProfile = $"{ApiRoutes.Public.Me}/profile";
    private const string PublicMeAvatar = $"{ApiRoutes.Public.Me}/avatar";

    [Fact]
    public async Task AdminUpdateOwnProfile_AsSuperAdmin_WithoutValidSession_ReturnsForbidden()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new { UserName = "newname123" };

        var response = await Client.PatchAsJsonAsync(AdminMeProfile, request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.Forbidden, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AdminUpdateOwnProfile_WithNoAuth_Returns401()
    {
        Client.ClearAuthentication();
        var request = new { UserName = "noauth123" };

        var response = await Client.PatchAsJsonAsync(AdminMeProfile, request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
