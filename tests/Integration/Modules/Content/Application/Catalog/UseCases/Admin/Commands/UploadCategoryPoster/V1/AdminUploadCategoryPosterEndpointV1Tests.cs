using _116.Content.Application.Catalog.UseCases.Admin.Commands.UploadCategoryPoster.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Core.Domain.Entities;
using _116.Core.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Helpers;
using FluentValidation;
using FluentValidation.Results;

namespace _116.Integration.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.UploadCategoryPoster.V1;

/// <summary>
/// Integration tests for the AdminUploadCategoryPoster endpoint.
/// </summary>
[Collection("Database")]
public class AdminUploadCategoryPosterEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private const string PosterSegment = "poster";

    private static string ValidationDetail(string property, string message) =>
        new ValidationException([new ValidationFailure(property, message)]).Message;

    private static MultipartFormDataContent BuildPosterContent()
    {
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 });
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        content.Add(fileContent, "file", "poster.jpg");
        return content;
    }

    private static MultipartFormDataContent BuildRealImagePosterContent(byte red, byte green, byte blue)
    {
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(ImageTestHelpers.SolidColorPng(red, green, blue));
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
        content.Add(fileContent, "file", "poster.png");
        return content;
    }

    private async Task<CategoryEntity> SeedCategoryAsync() =>
        await SeedAsync<ContentDbContext, CategoryEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            ctx.ContentTypes.Add(contentType);
            CategoryEntity cat = CategoryFactory.Create(contentType.Id);
            ctx.Categories.Add(cat);
            return cat;
        });

    [Fact]
    public async Task UploadCategoryPoster_AsSuperAdmin_WithFile_ReturnsOk()
    {
        CategoryEntity category = await SeedCategoryAsync();

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
    public async Task UploadCategoryPoster_WithRealImage_ExtractsAndReturnsColors()
    {
        CategoryEntity category = await SeedCategoryAsync();

        Client.AuthenticateAsSuperAdmin();
        // Solid yellow poster — a light background that must resolve to black text.
        using MultipartFormDataContent content = BuildRealImagePosterContent(255, 235, 59);

        var response = await Client.PutAsync($"{ApiRoutes.Admin.Categories}/{category.Id}/{PosterSegment}", content);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminUploadCategoryPosterResponse>();
        body.Category.Colors.Should().NotBeNull();
        body.Category.Colors!.Background.Should().Be("#FFEB3B");
        body.Category.Colors.Foreground.Should().Be("#000000");

        // The colors are persisted once, at write time, on the poster file.
        await using var contentContext = CreateDbContext<ContentDbContext>();
        CategoryEntity? updated = await contentContext.Categories.FindAsync(category.Id);
        updated!.PosterFileId.Should().NotBeNull();

        await using var coreContext = CreateDbContext<CoreDbContext>();
        FileEntity? posterFile = await coreContext.Files.FindAsync(updated.PosterFileId);
        posterFile.Should().NotBeNull();
        posterFile!.DominantColorHex.Should().Be("#FFEB3B");
        posterFile.ForegroundColorHex.Should().Be("#000000");
    }

    [Fact]
    public async Task UploadCategoryPoster_ReplacingPoster_RecomputesColors()
    {
        CategoryEntity category = await SeedCategoryAsync();
        Client.AuthenticateAsSuperAdmin();

        // First poster: yellow (light → black text).
        using (MultipartFormDataContent firstContent = BuildRealImagePosterContent(255, 235, 59))
        {
            var firstResponse = await Client.PutAsync(
                $"{ApiRoutes.Admin.Categories}/{category.Id}/{PosterSegment}",
                firstContent
            );
            firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // Replace with navy (dark → white text); colors must be overwritten.
        using MultipartFormDataContent secondContent = BuildRealImagePosterContent(13, 27, 42);
        var response = await Client.PutAsync(
            $"{ApiRoutes.Admin.Categories}/{category.Id}/{PosterSegment}",
            secondContent
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminUploadCategoryPosterResponse>();
        body.Category.Colors.Should().NotBeNull();
        body.Category.Colors!.Background.Should().Be("#0D1B2A");
        body.Category.Colors.Foreground.Should().Be("#FFFFFF");
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

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Category"))
        );
    }

    [Fact]
    public async Task UploadCategoryPoster_WithNoFilePart_ReturnsLocalizedValidationProblem()
    {
        Client.AuthenticateAsSuperAdmin();

        using var content = new MultipartFormDataContent();
        content.Add(new StringContent("unused"), "note");

        var response = await Client.PutAsync($"{ApiRoutes.Admin.Categories}/{Guid.NewGuid()}/{PosterSegment}", content);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail("File", Localized<CategoryErrorMessage>(m => m.FileRequired()))
        );
    }
}
