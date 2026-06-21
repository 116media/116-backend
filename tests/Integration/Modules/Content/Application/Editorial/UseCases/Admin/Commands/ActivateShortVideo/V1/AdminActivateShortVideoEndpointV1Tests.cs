using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.ActivateShortVideo.V1;

/// <summary>
/// Integration tests for the AdminActivateShortVideo endpoint.
/// </summary>
[Collection("Database")]
public class AdminActivateShortVideoEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task ActivateShortVideo_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Shorts}/{nonExistentId}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ActivateShortVideo_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Shorts}/{nonExistentId}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ActivateShortVideo_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Shorts}/{nonExistentId}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ActivateShortVideo_AsSuperAdmin_WithNonExistentId_ReturnsError()
    {
        Client.AuthenticateAsSuperAdmin();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Shorts}/{nonExistentId}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ActivateShortVideo_AsSuperAdmin_AlreadyActive_ReturnsConflict()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var shortVideo = ShortVideoFactory.Create();
        context.ShortVideos.Add(shortVideo);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Shorts}/{shortVideo.Id}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    /// <summary>
    /// Verifies that activating an inactive short video succeeds and returns a 200 OK response,
    /// exercising the happy path of <c>ShortVideoEntity.Activate</c>.
    /// </summary>
    [Fact]
    public async Task ActivateShortVideo_AsSuperAdmin_InactiveShortVideo_ReturnsOk()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var shortVideo = ShortVideoFactory.CreateInactive();
        context.ShortVideos.Add(shortVideo);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Shorts}/{shortVideo.Id}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
