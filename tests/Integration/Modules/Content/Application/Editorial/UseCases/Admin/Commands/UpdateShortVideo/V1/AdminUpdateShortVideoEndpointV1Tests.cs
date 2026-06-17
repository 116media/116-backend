using System.Net.Http.Headers;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateShortVideo.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpdateShortVideo.V1;

/// <summary>
/// Integration tests for the AdminUpdateShortVideo endpoint.
/// </summary>
[Collection("Database")]
public class AdminUpdateShortVideoEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    /// <summary>
    /// The endpoint binds <c>title</c> from the query string (it is a plain minimal-API
    /// parameter); only the optional replacement video file travels in the multipart body.
    /// </summary>
    private static string ShortUrl(Guid id, string title) =>
        $"{ApiRoutes.Admin.Shorts}/{id}?title={Uri.EscapeDataString(title)}";

    [Fact]
    public async Task UpdateShortVideo_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        Guid nonExistentId = Guid.NewGuid();

        using var formContent = new MultipartFormDataContent();
        formContent.Add(new ByteArrayContent([0xFF, 0xD8]), "file", "test.jpg");

        var response = await Client.PutAsync($"{ApiRoutes.Admin.Shorts}/{nonExistentId}", formContent);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateShortVideo_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();
        Guid nonExistentId = Guid.NewGuid();

        using var formContent = new MultipartFormDataContent();
        formContent.Add(new ByteArrayContent([0xFF, 0xD8]), "file", "test.jpg");

        var response = await Client.PutAsync($"{ApiRoutes.Admin.Shorts}/{nonExistentId}", formContent);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateShortVideo_AsAdmin_IsAllowed()
    {
        Client.AuthenticateAsAdmin();
        Guid nonExistentId = Guid.NewGuid();

        using var formContent = new MultipartFormDataContent();

        var response = await Client.PutAsync(ShortUrl(nonExistentId, "Updated Title"), formContent);

        response
            .StatusCode.Should()
            .BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateShortVideo_WithInvalidGuid_ShouldReturnBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();

        using var formContent = new MultipartFormDataContent();

        var response = await Client.PutAsync(
            $"{ApiRoutes.Admin.Shorts}/not-a-guid?title={Uri.EscapeDataString("Updated Title")}",
            formContent
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateShortVideo_WithEmptyTitle_ShouldReturnBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        Guid id = Guid.NewGuid();

        using var formContent = new MultipartFormDataContent();

        var response = await Client.PutAsync(ShortUrl(id, string.Empty), formContent);

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Verifies that updating the title of an existing short video succeeds, echoes the new
    /// title in the response DTO, and persists it.
    /// </summary>
    [Fact]
    public async Task UpdateShortVideo_AsSuperAdmin_WithValidData_ReturnsOkAndPersists()
    {
        ShortVideoEntity shortVideo = await SeedAsync<ContentDbContext, ShortVideoEntity>(ctx =>
        {
            ShortVideoEntity entity = ShortVideoFactory.Create();
            ctx.ShortVideos.Add(entity);
            return entity;
        });

        Client.AuthenticateAsSuperAdmin();

        // A non-empty multipart body is required for the form to parse; the replacement
        // video file is valid so only the title update is exercised.
        using var formContent = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent([0x00, 0x00, 0x00, 0x1C, 0x66, 0x74, 0x79, 0x70]);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("video/mp4");
        formContent.Add(fileContent, "videoFile", "clip.mp4");

        var response = await Client.PutAsync(ShortUrl(shortVideo.Id, "Updated Short Video Title"), formContent);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminUpdateShortVideoResponse body = await response.ReadAsAsync<AdminUpdateShortVideoResponse>();
        body.ShortVideo.Id.Should().Be(shortVideo.Id);
        body.ShortVideo.Title.Should().Be("Updated Short Video Title");

        await using ContentDbContext verifyContext = CreateDbContext<ContentDbContext>();
        ShortVideoEntity? persisted = await verifyContext.ShortVideos.FindAsync(shortVideo.Id);
        persisted!.Title.Should().Be("Updated Short Video Title");
    }

    /// <summary>
    /// Verifies that updating a short video with a file having a disallowed extension
    /// (e.g., ".exe") returns a 400 Bad Request response from the validator.
    /// </summary>
    [Fact]
    public async Task UpdateShortVideo_WithWrongFileExtension_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        Guid id = Guid.NewGuid();

        using var formContent = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent([0x00, 0x01, 0x02]);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("video/mp4");
        formContent.Add(fileContent, "videoFile", "test.exe");

        var response = await Client.PutAsync(ShortUrl(id, "Updated Title"), formContent);

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Verifies that updating a short video with a title exceeding the maximum allowed length
    /// (200 characters) returns a 400 Bad Request response from the ValidShortVideoTitle validator rule.
    /// </summary>
    [Fact]
    public async Task UpdateShortVideo_WithTitleTooLong_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        Guid id = Guid.NewGuid();

        using var formContent = new MultipartFormDataContent();

        var response = await Client.PutAsync(ShortUrl(id, new string('T', 300)), formContent);

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);
    }
}
