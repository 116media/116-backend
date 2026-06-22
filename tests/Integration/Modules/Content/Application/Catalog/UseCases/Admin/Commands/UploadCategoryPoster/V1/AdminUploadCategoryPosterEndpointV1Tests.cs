using _116.Content.Application.Catalog.UseCases.Admin.Commands.UploadCategoryPoster.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.UploadCategoryPoster.V1;

/// <summary>
/// Integration tests for the AdminUploadCategoryPoster endpoint.
/// </summary>
[Collection("Database")]
public class AdminUploadCategoryPosterEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private const string PosterSegment = "poster";

    private static MultipartFormDataContent BuildPosterContent()
    {
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 });
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        content.Add(fileContent, "file", "poster.jpg");
        return content;
    }

    [Fact]
    public async Task UploadCategoryPoster_AsSuperAdmin_WithFile_ReturnsOk()
    {
        CategoryEntity category = await SeedAsync<ContentDbContext, CategoryEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            ctx.ContentTypes.Add(contentType);
            CategoryEntity cat = CategoryFactory.Create(contentType.Id);
            ctx.Categories.Add(cat);
            return cat;
        });

        Client.AuthenticateAsSuperAdmin();
        using MultipartFormDataContent content = BuildPosterContent();

        var response = await Client.PutAsync($"{ApiRoutes.Admin.Categories}/{category.Id}/{PosterSegment}", content);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminUploadCategoryPosterResponse>();
        body.Category.Id.Should().Be(category.Id);
        body.Category.PosterUrl.Should().NotBeNullOrEmpty();
        body.Category.PosterUrl.Should().Contain("res.cloudinary.com/test-cloud");

        await using var verifyContext = CreateDbContext<ContentDbContext>();
        CategoryEntity? updated = await verifyContext.Categories.FindAsync(category.Id);
        updated.Should().NotBeNull();
        updated!.PosterFileId.Should().NotBeNull();
    }

    [Fact]
    public async Task UploadCategoryPoster_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();
        using MultipartFormDataContent content = BuildPosterContent();

        var response = await Client.PutAsync($"{ApiRoutes.Admin.Categories}/{Guid.NewGuid()}/{PosterSegment}", content);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UploadCategoryPoster_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        using MultipartFormDataContent content = BuildPosterContent();

        var response = await Client.PutAsync($"{ApiRoutes.Admin.Categories}/{Guid.NewGuid()}/{PosterSegment}", content);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UploadCategoryPoster_NonExistentCategory_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();
        using MultipartFormDataContent content = BuildPosterContent();

        var response = await Client.PutAsync($"{ApiRoutes.Admin.Categories}/{Guid.NewGuid()}/{PosterSegment}", content);

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }
}
