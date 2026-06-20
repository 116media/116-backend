namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.DeactivateShortVideo.V1;

/// <summary>
/// Integration tests for the AdminDeactivateShortVideo endpoint.
/// </summary>
[Collection("Database")]
public class AdminDeactivateShortVideoEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task DeactivateShortVideo_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Shorts}/{nonExistentId}/deactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeactivateShortVideo_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Shorts}/{nonExistentId}/deactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeactivateShortVideo_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Shorts}/{nonExistentId}/deactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeactivateShortVideo_AsSuperAdmin_WithNonExistentId_ReturnsError()
    {
        Client.AuthenticateAsSuperAdmin();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Shorts}/{nonExistentId}/deactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
