namespace _116.Integration.Tests.Modules.Identity.Application.User.UseCases.Public.Commands.UpdateAvatar.V1;

/// <summary>
/// Integration tests for the PublicUpdateAvatar endpoint.
/// </summary>
[Collection("Database")]
public class PublicUpdateAvatarEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private const string PublicMeAvatar = $"{ApiRoutes.Public.Me}/avatar";

    [Fact]
    public async Task UpdateAvatar_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(new byte[] { 0xFF, 0xD8 }), "file", "avatar.jpg");

        var response = await Client.PatchAsync(PublicMeAvatar, content);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateAvatar_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();

        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(new byte[] { 0xFF, 0xD8 }), "file", "avatar.jpg");

        var response = await Client.PatchAsync(PublicMeAvatar, content);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
