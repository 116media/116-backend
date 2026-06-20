namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpdateShortVideo.V1;

/// <summary>
/// Integration tests for the AdminUpdateShortVideo endpoint.
/// </summary>
[Collection("Database")]
public class AdminUpdateShortVideoEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task UpdateShortVideo_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var nonExistentId = Guid.NewGuid();

        using var formContent = new MultipartFormDataContent();
        formContent.Add(new ByteArrayContent(new byte[] { 0xFF, 0xD8 }), "file", "test.jpg");

        var response = await Client.PutAsync($"{ApiRoutes.Admin.Shorts}/{nonExistentId}", formContent);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateShortVideo_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();
        var nonExistentId = Guid.NewGuid();

        using var formContent = new MultipartFormDataContent();
        formContent.Add(new ByteArrayContent(new byte[] { 0xFF, 0xD8 }), "file", "test.jpg");

        var response = await Client.PutAsync($"{ApiRoutes.Admin.Shorts}/{nonExistentId}", formContent);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateShortVideo_AsAdmin_IsAllowed()
    {
        Client.AuthenticateAsAdmin();
        var nonExistentId = Guid.NewGuid();

        using var formContent = new MultipartFormDataContent();
        formContent.Add(new ByteArrayContent(new byte[] { 0xFF, 0xD8 }), "file", "test.jpg");

        var response = await Client.PutAsync($"{ApiRoutes.Admin.Shorts}/{nonExistentId}", formContent);

        response
            .StatusCode.Should()
            .BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity, HttpStatusCode.NotFound);
    }
}
