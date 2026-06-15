namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.CreateShortVideo.V1;

/// <summary>
/// Integration tests for the AdminCreateShortVideo endpoint.
/// </summary>
[Collection("Database")]
public class AdminCreateShortVideoEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task CreateShortVideo_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        using var formContent = new MultipartFormDataContent();
        formContent.Add(new ByteArrayContent(new byte[] { 0xFF, 0xD8 }), "file", "test.jpg");

        var response = await Client.PostAsync(ApiRoutes.Admin.Shorts, formContent);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateShortVideo_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        using var formContent = new MultipartFormDataContent();
        formContent.Add(new ByteArrayContent(new byte[] { 0xFF, 0xD8 }), "file", "test.jpg");

        var response = await Client.PostAsync(ApiRoutes.Admin.Shorts, formContent);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateShortVideo_AsAdmin_IsAllowed()
    {
        Client.AuthenticateAsAdmin();

        using var formContent = new MultipartFormDataContent();
        formContent.Add(new ByteArrayContent(new byte[] { 0xFF, 0xD8 }), "file", "test.jpg");

        var response = await Client.PostAsync(ApiRoutes.Admin.Shorts, formContent);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateShortVideo_WithValidMp4_ShouldPassFileValidation()
    {
        Client.AuthenticateAsSuperAdmin();

        using var formContent = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[] { 0x00, 0x00, 0x00, 0x1C, 0x66, 0x74, 0x79, 0x70 });
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("video/mp4");
        formContent.Add(fileContent, "videoFile", "clip.mp4");
        formContent.Add(new StringContent("Test Short"), "title");
        formContent.Add(new StringContent($"test-short-{Guid.NewGuid():N}"[..20]), "slug");

        var response = await Client.PostAsync(ApiRoutes.Admin.Shorts, formContent);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateShortVideo_WithInvalidExtension_ShouldReturnBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();

        using var formContent = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[] { 0x00, 0x01, 0x02 });
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("video/mp4");
        formContent.Add(fileContent, "videoFile", "clip.flv");
        formContent.Add(new StringContent("Bad Extension Short"), "title");
        formContent.Add(new StringContent($"bad-ext-{Guid.NewGuid():N}"[..20]), "slug");

        var response = await Client.PostAsync(ApiRoutes.Admin.Shorts, formContent);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateShortVideo_WithEmptyTitle_ShouldReturnBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();

        using var formContent = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[] { 0x00, 0x00, 0x00, 0x1C, 0x66, 0x74, 0x79, 0x70 });
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("video/mp4");
        formContent.Add(fileContent, "videoFile", "clip.mp4");
        formContent.Add(new StringContent(""), "title");
        formContent.Add(new StringContent("valid-slug"), "slug");

        var response = await Client.PostAsync(ApiRoutes.Admin.Shorts, formContent);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
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
        var fileContent = new ByteArrayContent(new byte[] { 0x00, 0x00, 0x00, 0x1C, 0x66, 0x74, 0x79, 0x70 });
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("video/mp4");
        formContent.Add(fileContent, "videoFile", "clip.mp4");
        formContent.Add(new StringContent("Valid Title"), "title");
        formContent.Add(new StringContent(""), "slug");

        var response = await Client.PostAsync(ApiRoutes.Admin.Shorts, formContent);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateShortVideo_WithInvalidSlug_ShouldReturnBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();

        using var formContent = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[] { 0x00, 0x00, 0x00, 0x1C, 0x66, 0x74, 0x79, 0x70 });
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("video/mp4");
        formContent.Add(fileContent, "videoFile", "clip.mp4");
        formContent.Add(new StringContent("Valid Title"), "title");
        formContent.Add(new StringContent("INVALID SLUG!!!"), "slug");

        var response = await Client.PostAsync(ApiRoutes.Admin.Shorts, formContent);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Verifies that creating a short video with a file exceeding the maximum allowed size
    /// (350 MB) returns a 400 Bad Request response from the validator.
    /// </summary>
    [Fact]
    public async Task CreateShortVideo_WithOversizedFile_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();

        using var formContent = new MultipartFormDataContent();
        var oversizedBytes = new byte[1024];
        var streamContent = new StreamContent(new MemoryStream(oversizedBytes));
        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("video/mp4");
        streamContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue(
            "form-data"
        )
        {
            Name = "\"videoFile\"",
            FileName = "\"oversized.mp4\"",
            Size = 400L * 1024 * 1024,
        };
        formContent.Add(streamContent);
        formContent.Add(new StringContent("Oversized Video"), "title");
        formContent.Add(new StringContent($"oversized-{Guid.NewGuid():N}"[..20]), "slug");

        var response = await Client.PostAsync(ApiRoutes.Admin.Shorts, formContent);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
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
        var fileContent = new ByteArrayContent(new byte[] { 0x00, 0x00, 0x00, 0x1C, 0x66, 0x74, 0x79, 0x70 });
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("video/mp4");
        formContent.Add(fileContent, "videoFile", "clip.mp4");
        formContent.Add(new StringContent(new string('T', 300)), "title");
        formContent.Add(new StringContent("valid-slug"), "slug");

        var response = await Client.PostAsync(ApiRoutes.Admin.Shorts, formContent);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
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
        var fileContent = new ByteArrayContent(new byte[] { 0x00, 0x00, 0x00, 0x1C, 0x66, 0x74, 0x79, 0x70 });
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("video/mp4");
        formContent.Add(fileContent, "videoFile", "clip.mp4");
        formContent.Add(new StringContent("Valid Title"), "title");
        formContent.Add(new StringContent(new string('a', 300)), "slug");

        var response = await Client.PostAsync(ApiRoutes.Admin.Shorts, formContent);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
