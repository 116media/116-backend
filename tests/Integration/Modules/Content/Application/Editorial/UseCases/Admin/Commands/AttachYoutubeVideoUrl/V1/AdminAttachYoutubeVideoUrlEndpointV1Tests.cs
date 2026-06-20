namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.AttachYoutubeVideoUrl.V1;

/// <summary>
/// Integration tests for the AdminAttachYoutubeVideoUrl endpoint.
/// </summary>
[Collection("Database")]
public class AdminAttachYoutubeVideoUrlEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task AttachYoutubeVideoUrl_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PatchAsJsonAsync(
            $"{ApiRoutes.Admin.Videos}/{nonExistentId}/youtube",
            new { Reason = "test" }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AttachYoutubeVideoUrl_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PatchAsJsonAsync(
            $"{ApiRoutes.Admin.Videos}/{nonExistentId}/youtube",
            new { Reason = "test" }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AttachYoutubeVideoUrl_AsAdmin_IsAllowed()
    {
        Client.AuthenticateAsAdmin();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PatchAsJsonAsync(
            $"{ApiRoutes.Admin.Videos}/{nonExistentId}/youtube",
            new { Reason = "test" }
        );

        response
            .StatusCode.Should()
            .BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.UnprocessableEntity);
    }
}
