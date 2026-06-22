using System.Net.Http.Headers;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateShortVideo.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.CreateShortVideo.V1;

/// <summary>
/// Integration tests for the AdminCreateShortVideo endpoint.
/// </summary>
[Collection("Database")]
public class AdminCreateShortVideoEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static readonly byte[] ValidMp4Bytes = [0x00, 0x00, 0x00, 0x1C, 0x66, 0x74, 0x79, 0x70];

    /// <summary>
    /// The endpoint binds <c>title</c> and <c>slug</c> from the query string (they are plain
    /// minimal-API parameters); only the video file travels in the multipart body.
    /// </summary>
    private static string ShortsUrl(string title, string slug) =>
        $"{ApiRoutes.Admin.Shorts}?title={Uri.EscapeDataString(title)}&slug={Uri.EscapeDataString(slug)}";

    private static void AddVideoFile(MultipartFormDataContent form, byte[] bytes, string fileName)
    {
        var fileContent = new ByteArrayContent(bytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("video/mp4");
        form.Add(fileContent, "videoFile", fileName);
    }

    [Fact]
    public async Task CreateShortVideo_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        using var formContent = new MultipartFormDataContent();
        formContent.Add(new ByteArrayContent([0xFF, 0xD8]), "file", "test.jpg");

        var response = await Client.PostAsync(ApiRoutes.Admin.Shorts, formContent);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateShortVideo_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        using var formContent = new MultipartFormDataContent();
        formContent.Add(new ByteArrayContent([0xFF, 0xD8]), "file", "test.jpg");

        var response = await Client.PostAsync(ApiRoutes.Admin.Shorts, formContent);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateShortVideo_AsAdmin_IsAllowed()
    {
        Client.AuthenticateAsAdmin();

        using var formContent = new MultipartFormDataContent();
        formContent.Add(new ByteArrayContent([0xFF, 0xD8]), "file", "test.jpg");

        var response = await Client.PostAsync(ApiRoutes.Admin.Shorts, formContent);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Verifies that creating a short video with a valid mp4 file, title, and slug succeeds,
    /// returns the created short video DTO, and persists the row.
    /// </summary>
    [Fact]
    public async Task CreateShortVideo_WithValidMp4_ReturnsCreatedAndPersists()
    {
        Client.AuthenticateAsSuperAdmin();
        string slug = $"test-short-{Guid.NewGuid():N}"[..20];

        using var formContent = new MultipartFormDataContent();
        AddVideoFile(formContent, ValidMp4Bytes, "clip.mp4");

        var response = await Client.PostAsync(ShortsUrl("Test Short", slug), formContent);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        AdminCreateShortVideoResponse body = await response.ReadAsAsync<AdminCreateShortVideoResponse>();
        body.ShortVideo.Id.Should().NotBeEmpty();
        body.ShortVideo.Title.Should().Be("Test Short");
        body.ShortVideo.Slug.Should().Be(slug);

        await using ContentDbContext verifyContext = CreateDbContext<ContentDbContext>();
        ShortVideoEntity? persisted = await verifyContext.ShortVideos.FindAsync(body.ShortVideo.Id);
        persisted.Should().NotBeNull();
        persisted!.Slug.Should().Be(slug);
    }

    [Fact]
    public async Task CreateShortVideo_WithInvalidExtension_ShouldReturnBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        string slug = $"bad-ext-{Guid.NewGuid():N}"[..20];

        using var formContent = new MultipartFormDataContent();
        AddVideoFile(formContent, [0x00, 0x01, 0x02], "clip.flv");

        var response = await Client.PostAsync(ShortsUrl("Bad Extension Short", slug), formContent);

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateShortVideo_WithEmptyTitle_ShouldReturnBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();

        using var formContent = new MultipartFormDataContent();
        AddVideoFile(formContent, ValidMp4Bytes, "clip.mp4");

        var response = await Client.PostAsync(ShortsUrl(string.Empty, "valid-slug"), formContent);

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Verifies that creating a short video with an empty slug returns 400 Bad Request
    /// from the validator because the slug is required and must not be empty.
    /// </summary>
    [Fact]
    public async Task CreateShortVideo_WithEmptySlug_ShouldReturnBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();

        using var formContent = new MultipartFormDataContent();
        AddVideoFile(formContent, ValidMp4Bytes, "clip.mp4");

        var response = await Client.PostAsync(ShortsUrl("Valid Title", string.Empty), formContent);

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateShortVideo_WithInvalidSlug_ShouldReturnBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();

        using var formContent = new MultipartFormDataContent();
        AddVideoFile(formContent, ValidMp4Bytes, "clip.mp4");

        var response = await Client.PostAsync(ShortsUrl("Valid Title", "INVALID SLUG!!!"), formContent);

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Verifies that creating a short video with a title exceeding the maximum allowed length
    /// (200 characters) returns a 400 Bad Request response from the ValidShortVideoTitle validator rule.
    /// </summary>
    [Fact]
    public async Task CreateShortVideo_WithTitleTooLong_ShouldReturnBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();

        using var formContent = new MultipartFormDataContent();
        AddVideoFile(formContent, ValidMp4Bytes, "clip.mp4");

        var response = await Client.PostAsync(ShortsUrl(new string('T', 300), "valid-slug"), formContent);

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Verifies that creating a short video with a slug exceeding the maximum allowed length
    /// (220 characters) returns a 400 Bad Request response from the ValidShortVideoSlug validator rule.
    /// </summary>
    [Fact]
    public async Task CreateShortVideo_WithSlugTooLong_ShouldReturnBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();

        using var formContent = new MultipartFormDataContent();
        AddVideoFile(formContent, ValidMp4Bytes, "clip.mp4");

        var response = await Client.PostAsync(ShortsUrl("Valid Title", new string('a', 300)), formContent);

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);
    }
}
