using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadLyricsCover.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UploadLyricsCover.V1;

/// <summary>
/// Integration tests for the AdminUploadLyricsCover endpoint.
/// </summary>
[Collection("Database")]
public class AdminUploadLyricsCoverEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private async Task<LyricsEntity> SeedLyricsAsync()
    {
        return await SeedAsync<ContentDbContext, LyricsEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            LyricsEntity lyrics = LyricsFactory.Create(category.Id);
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Lyrics.Add(lyrics);
            return lyrics;
        });
    }

    private static MultipartFormDataContent CreateCoverContent()
    {
        var formContent = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent([0xFF, 0xD8, 0xFF, 0xE0]);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        formContent.Add(fileContent, "file", "cover.jpg");
        return formContent;
    }

    [Fact]
    public async Task UploadLyricsCover_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        using MultipartFormDataContent formContent = CreateCoverContent();

        var response = await Client.PostAsync(
            Routes.Admin.Editorial.Cover(EditorialRouteConstants.Lyrics, Guid.NewGuid()),
            formContent
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UploadLyricsCover_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        using MultipartFormDataContent formContent = CreateCoverContent();

        var response = await Client.PostAsync(
            Routes.Admin.Editorial.Cover(EditorialRouteConstants.Lyrics, Guid.NewGuid()),
            formContent
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UploadLyricsCover_AsAdmin_WithNonExistentId_ReturnsNotFound()
    {
        Client.AuthenticateAsAdmin();

        using MultipartFormDataContent formContent = CreateCoverContent();

        var response = await Client.PostAsync(
            Routes.Admin.Editorial.Cover(EditorialRouteConstants.Lyrics, Guid.NewGuid()),
            formContent
        );

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Verifies the cover URL is null before any upload, then resolves correctly to the
    /// uploaded Cloudinary URL after a successful upload, persisting the resolved file id.
    /// </summary>
    [Fact]
    public async Task UploadLyricsCover_AsSuperAdmin_WithValidFile_ReturnsOkAndPersists()
    {
        LyricsEntity lyrics = await SeedLyricsAsync();
        lyrics.CoverImageFileId.Should().BeNull();

        Client.AuthenticateAsSuperAdmin();

        using MultipartFormDataContent formContent = CreateCoverContent();

        var response = await Client.PostAsync(
            Routes.Admin.Editorial.Cover(EditorialRouteConstants.Lyrics, lyrics.Id),
            formContent
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminUploadLyricsCoverResponse body = await response.ReadAsAsync<AdminUploadLyricsCoverResponse>();
        body.CoverImageUrl.Should().StartWith("https://res.cloudinary.com/test-cloud/");
        body.CoverImageStorageKey.Should().NotBeNullOrEmpty();

        await using ContentDbContext verifyContext = CreateDbContext<ContentDbContext>();
        LyricsEntity? persisted = await verifyContext.Lyrics.FindAsync(lyrics.Id);
        persisted!.CoverImageFileId.Should().NotBeNull();
    }

    /// <summary>
    /// Uploading a replacement cover for a lyrics page that already has one overwrites the
    /// stored file reference rather than accumulating multiple files.
    /// </summary>
    [Fact]
    public async Task UploadLyricsCover_WhenLyricsHasExistingCover_OverwritesInPlace()
    {
        LyricsEntity lyrics = await SeedLyricsAsync();
        Client.AuthenticateAsSuperAdmin();

        using (MultipartFormDataContent firstUpload = CreateCoverContent())
        {
            await Client.PostAsync(
                Routes.Admin.Editorial.Cover(EditorialRouteConstants.Lyrics, lyrics.Id),
                firstUpload
            );
        }

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        LyricsEntity? afterFirst = await ctx.Lyrics.FindAsync(lyrics.Id);
        Guid firstFileId = afterFirst!.CoverImageFileId!.Value;

        using MultipartFormDataContent secondUpload = CreateCoverContent();
        var response = await Client.PostAsync(
            Routes.Admin.Editorial.Cover(EditorialRouteConstants.Lyrics, lyrics.Id),
            secondUpload
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using ContentDbContext verifyContext = CreateDbContext<ContentDbContext>();
        LyricsEntity? afterSecond = await verifyContext.Lyrics.FindAsync(lyrics.Id);
        afterSecond!.CoverImageFileId.Should().NotBeNull();
        firstFileId.Should().NotBe(Guid.Empty);
    }
}
