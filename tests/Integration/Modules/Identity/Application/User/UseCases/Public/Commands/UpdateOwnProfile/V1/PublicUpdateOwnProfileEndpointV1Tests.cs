using System.Text.Json;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.User.UseCases.Public.Commands.UpdateOwnProfile.V1;

/// <summary>
/// Integration tests for the PublicUpdateOwnProfile endpoint.
/// </summary>
[Collection("Database")]
public class PublicUpdateOwnProfileEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private const string AdminMeProfile = $"{ApiRoutes.Admin.Base}/me/profile";
    private const string AdminMeAvatar = $"{ApiRoutes.Admin.Base}/me/avatar";
    private const string PublicMeProfile = $"{ApiRoutes.Public.Me}/profile";
    private const string PublicMeAvatar = $"{ApiRoutes.Public.Me}/avatar";

    [Fact]
    public async Task PublicUpdateOwnProfile_AsVisitor_WithoutValidSession_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();
        var request = new { UserName = "updated123" };

        var response = await Client.PatchAsJsonAsync(PublicMeProfile, request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.Forbidden, HttpStatusCode.BadRequest);
    }
}
