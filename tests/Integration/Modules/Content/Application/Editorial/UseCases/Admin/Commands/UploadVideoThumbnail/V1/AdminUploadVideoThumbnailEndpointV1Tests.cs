using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadVideoThumbnail.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UploadVideoThumbnail.V1;

/// <summary>
/// Integration tests for the AdminUploadVideoThumbnail endpoint.
/// </summary>
[Collection("Database")]
public class AdminUploadVideoThumbnailEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private async Task<VideoEntity> SeedVideoAsync()
    {
        return await SeedAsync<ContentDbContext, VideoEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            VideoEntity video = VideoFactory.Create(category.Id);
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Videos.Add(video);
            return video;
        });
    }

    private static MultipartFormDataContent CreateThumbnailContent()
    {
        var formContent = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent([0xFF, 0xD8, 0xFF, 0xE0]);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        formContent.Add(fileContent, "file", "test.jpg");
        return formContent;
    }

    [Fact]
    public async Task UploadVideoThumbnail_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        using MultipartFormDataContent formContent = CreateThumbnailContent();

        var response = await Client.PostAsync(
            Routes.Admin.Editorial.Thumbnail(EditorialRouteConstants.Videos, Guid.NewGuid()),
            formContent
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UploadVideoThumbnail_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        using MultipartFormDataContent formContent = CreateThumbnailContent();

        var response = await Client.PostAsync(
            Routes.Admin.Editorial.Thumbnail(EditorialRouteConstants.Videos, Guid.NewGuid()),
            formContent
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UploadVideoThumbnail_AsAdmin_WithNonExistentId_ReturnsNotFound()
    {
        Client.AuthenticateAsAdmin();

        using MultipartFormDataContent formContent = CreateThumbnailContent();

        var response = await Client.PostAsync(
            Routes.Admin.Editorial.Thumbnail(EditorialRouteConstants.Videos, Guid.NewGuid()),
            formContent
        );

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Video"))
        );
    }

    [Fact]
    public async Task UploadVideoThumbnail_AsSuperAdmin_WithValidFile_ReturnsOkAndPersists()
    {
        VideoEntity video = await SeedVideoAsync();
        Client.AuthenticateAsSuperAdmin();

        using MultipartFormDataContent formContent = CreateThumbnailContent();

        var response = await Client.PostAsync(
            Routes.Admin.Editorial.Thumbnail(EditorialRouteConstants.Videos, video.Id),
            formContent
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminUploadVideoThumbnailResponse body = await response.ReadAsAsync<AdminUploadVideoThumbnailResponse>();
        body.ThumbnailUrl.Should().StartWith("https://res.cloudinary.com/test-cloud/");
        body.ThumbnailStorageKey.Should().NotBeNullOrEmpty();

        await using ContentDbContext verifyContext = CreateDbContext<ContentDbContext>();
        VideoEntity? persisted = await verifyContext.Videos.FindAsync(video.Id);
        persisted!.ThumbnailFileId.Should().NotBeNull();
    }
}
