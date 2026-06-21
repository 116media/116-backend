using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

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

    [Fact]
    public async Task DeactivateShortVideo_AsSuperAdmin_AlreadyInactive_ReturnsConflict()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var shortVideo = ShortVideoFactory.CreateInactive();
        context.ShortVideos.Add(shortVideo);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Shorts}/{shortVideo.Id}/deactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    /// <summary>
    /// Verifies that deactivating an active short video succeeds and returns a 200 OK response,
    /// exercising the happy path of <c>ShortVideoEntity.Deactivate</c>.
    /// </summary>
    [Fact]
    public async Task DeactivateShortVideo_AsSuperAdmin_ActiveShortVideo_ReturnsOk()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var shortVideo = ShortVideoFactory.Create();
        context.ShortVideos.Add(shortVideo);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Shorts}/{shortVideo.Id}/deactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
