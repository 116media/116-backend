using _116.BuildingBlocks.Constants;
using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadShortVideoFile.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Factories.Content;
using FluentValidation;
using FluentValidation.Results;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UploadShortVideoFile.V1;

/// <summary>
/// Integration tests for the AdminUploadShortVideoFile endpoint.
/// </summary>
[Collection("Database")]
public class AdminUploadShortVideoFileEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string ValidationDetail(string property, string message) =>
        new ValidationException([new ValidationFailure(property, message)]).Message;

    private static MultipartFormDataContent CreateVideoContent()
    {
        var formContent = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent([0x00, 0x00, 0x00, 0x1C, 0x66, 0x74, 0x79, 0x70]);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("video/mp4");
        formContent.Add(fileContent, "file", "clip.mp4");
        return formContent;
    }

    [Fact]
    public async Task UploadShortVideoFile_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        using MultipartFormDataContent formContent = CreateVideoContent();

        var response = await Client.PostAsync(
            Routes.Admin.Editorial.Video(EditorialRouteConstants.Shorts, Guid.NewGuid()),
            formContent
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UploadShortVideoFile_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        using MultipartFormDataContent formContent = CreateVideoContent();

        var response = await Client.PostAsync(
            Routes.Admin.Editorial.Video(EditorialRouteConstants.Shorts, Guid.NewGuid()),
            formContent
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UploadShortVideoFile_AsAdmin_WithNonExistentId_ReturnsNotFound()
    {
        Client.AuthenticateAsAdmin();

        using MultipartFormDataContent formContent = CreateVideoContent();

        var response = await Client.PostAsync(
            Routes.Admin.Editorial.Video(EditorialRouteConstants.Shorts, Guid.NewGuid()),
            formContent
        );

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("ShortVideo"))
        );
    }

    [Fact]
    public async Task UploadShortVideoFile_WithWrongFileExtension_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();

        var formContent = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent([0x00, 0x01, 0x02]);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("video/mp4");
        formContent.Add(fileContent, "file", "clip.exe");

        var response = await Client.PostAsync(
            Routes.Admin.Editorial.Video(EditorialRouteConstants.Shorts, Guid.NewGuid()),
            formContent
        );

        formContent.Dispose();

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail(
                "File",
                Localized<ShortVideoErrorMessage>(m =>
                    m.FileInvalidExtension(string.Join(", ", FileConstants.AllowedVideoExtensions))
                )
            )
        );
    }

    [Fact]
    public async Task UploadShortVideoFile_AsSuperAdmin_WithValidFile_ReturnsOkAndPersists()
    {
        ShortVideoEntity shortVideo = await SeedAsync<ContentDbContext, ShortVideoEntity>(ctx =>
        {
            ShortVideoEntity entity = ShortVideoFactory.CreateDraft();
            ctx.ShortVideos.Add(entity);
            return entity;
        });

        Client.AuthenticateAsSuperAdmin();

        using MultipartFormDataContent formContent = CreateVideoContent();

        var response = await Client.PostAsync(
            Routes.Admin.Editorial.Video(EditorialRouteConstants.Shorts, shortVideo.Id),
            formContent
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminUploadShortVideoFileResponse body = await response.ReadAsAsync<AdminUploadShortVideoFileResponse>();
        body.VideoUrl.Should().StartWith("https://res.cloudinary.com/test-cloud/");
        body.VideoStorageKey.Should().NotBeNullOrEmpty();

        await using ContentDbContext verifyContext = CreateDbContext<ContentDbContext>();
        ShortVideoEntity? persisted = await verifyContext.ShortVideos.FindAsync(shortVideo.Id);
        persisted!.VideoFileId.Should().NotBeNull();
    }

    [Fact]
    public async Task UploadShortVideoFile_WithNoFilePart_ReturnsLocalizedValidationProblem()
    {
        Client.AuthenticateAsSuperAdmin();

        using var formContent = new MultipartFormDataContent();
        formContent.Add(new StringContent("unused"), "note");

        var response = await Client.PostAsync(
            Routes.Admin.Editorial.Video(EditorialRouteConstants.Shorts, Guid.NewGuid()),
            formContent
        );

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail("File", Localized<ShortVideoErrorMessage>(m => m.FileRequired()))
        );
    }
}
