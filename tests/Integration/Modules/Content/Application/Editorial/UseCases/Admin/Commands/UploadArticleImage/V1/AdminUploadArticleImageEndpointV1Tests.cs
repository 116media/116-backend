namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UploadArticleImage.V1;

/// <summary>
/// Integration tests for the AdminUploadArticleImage endpoint.
/// </summary>
[Collection("Database")]
public class AdminUploadArticleImageEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task UploadArticleImage_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var nonExistentId = Guid.NewGuid();

        using var formContent = new MultipartFormDataContent();
        formContent.Add(new ByteArrayContent(new byte[] { 0xFF, 0xD8 }), "file", "test.jpg");

        var response = await Client.PostAsync($"{ApiRoutes.Admin.Articles}/{nonExistentId}/images", formContent);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UploadArticleImage_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();
        var nonExistentId = Guid.NewGuid();

        using var formContent = new MultipartFormDataContent();
        formContent.Add(new ByteArrayContent(new byte[] { 0xFF, 0xD8 }), "file", "test.jpg");

        var response = await Client.PostAsync($"{ApiRoutes.Admin.Articles}/{nonExistentId}/images", formContent);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UploadArticleImage_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();
        var nonExistentId = Guid.NewGuid();

        using var formContent = new MultipartFormDataContent();
        formContent.Add(new ByteArrayContent(new byte[] { 0xFF, 0xD8 }), "file", "test.jpg");

        var response = await Client.PostAsync($"{ApiRoutes.Admin.Articles}/{nonExistentId}/images", formContent);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UploadArticleImage_AsSuperAdmin_WithNonExistentId_ReturnsError()
    {
        Client.AuthenticateAsSuperAdmin();
        var nonExistentId = Guid.NewGuid();

        using var formContent = new MultipartFormDataContent();
        formContent.Add(new ByteArrayContent(new byte[] { 0xFF, 0xD8 }), "file", "test.jpg");

        var response = await Client.PostAsync($"{ApiRoutes.Admin.Articles}/{nonExistentId}/images", formContent);

        response
            .StatusCode.Should()
            .BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.UnprocessableEntity);
    }
}
