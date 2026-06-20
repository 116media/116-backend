using System.Net.Http.Json;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.UploadCategoryPoster.V1;

/// <summary>
/// Integration tests for the AdminUploadCategoryPoster endpoint.
/// </summary>
[Collection("Database")]
public class AdminUploadCategoryPosterEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string ShortName(string prefix = "c") => $"{prefix}{Guid.NewGuid().ToString("N")[..8]}";

    private static string ShortSlug(string prefix = "s") => $"{prefix}-{Guid.NewGuid().ToString("N")[..8]}";

    [Fact]
    public async Task UploadCategoryPoster_AsSuperAdmin_WithFile_ReturnsOk()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        seedContext.ContentTypes.Add(contentType);
        var category = CategoryFactory.Create(contentType.Id);
        seedContext.Categories.Add(category);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();
        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 });
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        content.Add(fileContent, "file", "poster.jpg");

        var response = await Client.PutAsync($"{ApiRoutes.Admin.Categories}/{category.Id}/poster", content);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UploadCategoryPoster_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();
        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(new byte[] { 0xFF, 0xD8 }), "file", "poster.jpg");

        var response = await Client.PutAsync($"{ApiRoutes.Admin.Categories}/{Guid.NewGuid()}/poster", content);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UploadCategoryPoster_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(new byte[] { 0xFF, 0xD8 }), "file", "poster.jpg");

        var response = await Client.PutAsync($"{ApiRoutes.Admin.Categories}/{Guid.NewGuid()}/poster", content);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UploadCategoryPoster_NonExistentCategory_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();
        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(new byte[] { 0xFF, 0xD8 }), "file", "poster.jpg");

        var response = await Client.PutAsync($"{ApiRoutes.Admin.Categories}/{Guid.NewGuid()}/poster", content);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
