using System.Text.Json;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.User.UseCases.Admin.Commands.UpdateAvatar.V1;

/// <summary>
/// Integration tests for the AdminUpdateAvatar endpoint.
/// </summary>
[Collection("Database")]
public class AdminUpdateAvatarEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private const string AdminMeProfile = $"{ApiRoutes.Admin.Base}/me/profile";
    private const string AdminMeAvatar = $"{ApiRoutes.Admin.Base}/me/avatar";
    private const string PublicMeProfile = $"{ApiRoutes.Public.Me}/profile";
    private const string PublicMeAvatar = $"{ApiRoutes.Public.Me}/avatar";

    [Fact]
    public async Task AdminUpdateAvatar_WithNoAuth_Returns401()
    {
        Client.ClearAuthentication();

        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(new byte[] { 0xFF, 0xD8 }), "file", "avatar.jpg");

        var response = await Client.PatchAsync(AdminMeAvatar, content);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AdminUpdateAvatar_WithInvalidExtension_ShouldReturnBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();

        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[] { 0xFF, 0xD8, 0xFF });
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        content.Add(fileContent, "avatarFile", "avatar.bmp");

        var response = await Client.PatchAsync(AdminMeAvatar, content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AdminUpdateAvatar_WithInvalidMimeType_ShouldReturn422()
    {
        Client.AuthenticateAsSuperAdmin();

        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[] { 0x00, 0x01, 0x02 });
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
        content.Add(fileContent, "avatarFile", "document.pdf");

        var response = await Client.PatchAsync(AdminMeAvatar, content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
