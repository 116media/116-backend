namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.CreateShortVideo.V1;

/// <summary>
/// Integration tests for the AdminCreateShortVideo endpoint.
/// </summary>
[Collection("Database")]
public class AdminCreateShortVideoEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task CreateShortVideo_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        using var formContent = new MultipartFormDataContent();
        formContent.Add(new ByteArrayContent(new byte[] { 0xFF, 0xD8 }), "file", "test.jpg");

        var response = await Client.PostAsync(ApiRoutes.Admin.Shorts, formContent);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateShortVideo_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        using var formContent = new MultipartFormDataContent();
        formContent.Add(new ByteArrayContent(new byte[] { 0xFF, 0xD8 }), "file", "test.jpg");

        var response = await Client.PostAsync(ApiRoutes.Admin.Shorts, formContent);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateShortVideo_AsAdmin_IsAllowed()
    {
        Client.AuthenticateAsAdmin();

        using var formContent = new MultipartFormDataContent();
        formContent.Add(new ByteArrayContent(new byte[] { 0xFF, 0xD8 }), "file", "test.jpg");

        var response = await Client.PostAsync(ApiRoutes.Admin.Shorts, formContent);

        response
            .StatusCode.Should()
            .BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity, HttpStatusCode.NotFound);
    }
}
