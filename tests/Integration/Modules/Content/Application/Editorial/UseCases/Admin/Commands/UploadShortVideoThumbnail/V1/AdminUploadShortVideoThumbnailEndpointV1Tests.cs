using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadShortVideoThumbnail.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UploadShortVideoThumbnail.V1;

/// <summary>
/// Integration tests for the AdminUploadShortVideoThumbnail endpoint.
/// </summary>
[Collection("Database")]
public class AdminUploadShortVideoThumbnailEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static MultipartFormDataContent CreateThumbnailContent()
    {
        var formContent = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent([0xFF, 0xD8, 0xFF, 0xE0]);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        formContent.Add(fileContent, "file", "test.jpg");
        return formContent;
    }

    [Fact]
    public async Task UploadShortVideoThumbnail_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        using MultipartFormDataContent formContent = CreateThumbnailContent();

        var response = await Client.PostAsync(
            Routes.Admin.Editorial.Thumbnail(EditorialRouteConstants.Shorts, Guid.NewGuid()),
            formContent
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UploadShortVideoThumbnail_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        using MultipartFormDataContent formContent = CreateThumbnailContent();

        var response = await Client.PostAsync(
            Routes.Admin.Editorial.Thumbnail(EditorialRouteConstants.Shorts, Guid.NewGuid()),
            formContent
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UploadShortVideoThumbnail_AsAdmin_WithNonExistentId_ReturnsNotFound()
    {
        Client.AuthenticateAsAdmin();

        using MultipartFormDataContent formContent = CreateThumbnailContent();

        var response = await Client.PostAsync(
            Routes.Admin.Editorial.Thumbnail(EditorialRouteConstants.Shorts, Guid.NewGuid()),
            formContent
        );

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("ShortVideo"))
        );
    }

    [Fact]
    public async Task UploadShortVideoThumbnail_AsSuperAdmin_WithValidFile_ReturnsOkAndPersists()
    {
        ShortVideoEntity shortVideo = await SeedAsync<ContentDbContext, ShortVideoEntity>(ctx =>
        {
            ShortVideoEntity entity = ShortVideoFactory.Create();
            ctx.ShortVideos.Add(entity);
            return entity;
        });

        Client.AuthenticateAsSuperAdmin();

        using MultipartFormDataContent formContent = CreateThumbnailContent();

        var response = await Client.PostAsync(
            Routes.Admin.Editorial.Thumbnail(EditorialRouteConstants.Shorts, shortVideo.Id),
            formContent
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminUploadShortVideoThumbnailResponse body =
            await response.ReadAsAsync<AdminUploadShortVideoThumbnailResponse>();
        body.ThumbnailUrl.Should().StartWith("https://res.cloudinary.com/test-cloud/");
        body.ThumbnailStorageKey.Should().NotBeNullOrEmpty();

        await using ContentDbContext verifyContext = CreateDbContext<ContentDbContext>();
        ShortVideoEntity? persisted = await verifyContext.ShortVideos.FindAsync(shortVideo.Id);
        persisted!.ThumbnailFileId.Should().NotBeNull();
    }
}
