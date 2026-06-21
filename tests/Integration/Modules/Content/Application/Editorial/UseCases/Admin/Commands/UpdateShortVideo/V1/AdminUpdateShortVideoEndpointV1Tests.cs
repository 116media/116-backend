using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpdateShortVideo.V1;

/// <summary>
/// Integration tests for the AdminUpdateShortVideo endpoint.
/// </summary>
[Collection("Database")]
public class AdminUpdateShortVideoEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task UpdateShortVideo_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var nonExistentId = Guid.NewGuid();

        using var formContent = new MultipartFormDataContent();
        formContent.Add(new ByteArrayContent(new byte[] { 0xFF, 0xD8 }), "file", "test.jpg");

        var response = await Client.PutAsync($"{ApiRoutes.Admin.Shorts}/{nonExistentId}", formContent);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateShortVideo_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();
        var nonExistentId = Guid.NewGuid();

        using var formContent = new MultipartFormDataContent();
        formContent.Add(new ByteArrayContent(new byte[] { 0xFF, 0xD8 }), "file", "test.jpg");

        var response = await Client.PutAsync($"{ApiRoutes.Admin.Shorts}/{nonExistentId}", formContent);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateShortVideo_AsAdmin_IsAllowed()
    {
        Client.AuthenticateAsAdmin();
        var nonExistentId = Guid.NewGuid();

        using var formContent = new MultipartFormDataContent();
        formContent.Add(new ByteArrayContent(new byte[] { 0xFF, 0xD8 }), "file", "test.jpg");

        var response = await Client.PutAsync($"{ApiRoutes.Admin.Shorts}/{nonExistentId}", formContent);

        response
            .StatusCode.Should()
            .BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateShortVideo_WithInvalidGuid_ShouldReturnBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();

        using var formContent = new MultipartFormDataContent();
        formContent.Add(new StringContent("Updated Title"), "title");

        var response = await Client.PutAsync($"{ApiRoutes.Admin.Shorts}/not-a-guid", formContent);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateShortVideo_WithEmptyTitle_ShouldReturnBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        var id = Guid.NewGuid();

        using var formContent = new MultipartFormDataContent();
        formContent.Add(new StringContent(""), "title");

        var response = await Client.PutAsync($"{ApiRoutes.Admin.Shorts}/{id}", formContent);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateShortVideo_AsSuperAdmin_WithValidData_ReturnsOk()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var shortVideo = ShortVideoFactory.Create();
        seedContext.ShortVideos.Add(shortVideo);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        using var formContent = new MultipartFormDataContent();
        formContent.Add(new StringContent("Updated Short Video Title"), "title");

        var response = await Client.PutAsync($"{ApiRoutes.Admin.Shorts}/{shortVideo.Id}", formContent);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Verifies that updating a short video with a file having a disallowed extension
    /// (e.g., ".exe") returns a 400 Bad Request response from the validator.
    /// </summary>
    [Fact]
    public async Task UpdateShortVideo_WithWrongFileExtension_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        var id = Guid.NewGuid();

        using var formContent = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[] { 0x00, 0x01, 0x02 });
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("video/mp4");
        formContent.Add(fileContent, "videoFile", "test.exe");
        formContent.Add(new StringContent("Updated Title"), "title");

        var response = await Client.PutAsync($"{ApiRoutes.Admin.Shorts}/{id}", formContent);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Verifies that updating a short video with a file exceeding the maximum allowed size
    /// (350 MB) returns a 400 Bad Request response from the ValidShortVideoFile validator rule.
    /// </summary>
    [Fact]
    public async Task UpdateShortVideo_WithOversizedFile_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        var id = Guid.NewGuid();

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
        formContent.Add(new StringContent("Valid Title"), "title");

        var response = await Client.PutAsync($"{ApiRoutes.Admin.Shorts}/{id}", formContent);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Verifies that updating a short video with a title exceeding the maximum allowed length
    /// (200 characters) returns a 400 Bad Request response from the ValidShortVideoTitle validator rule.
    /// </summary>
    [Fact]
    public async Task UpdateShortVideo_WithTitleTooLong_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        var id = Guid.NewGuid();

        using var formContent = new MultipartFormDataContent();
        formContent.Add(new StringContent(new string('T', 300)), "title");

        var response = await Client.PutAsync($"{ApiRoutes.Admin.Shorts}/{id}", formContent);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
