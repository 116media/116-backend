namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Queries.GetActiveVideos.V1;

/// <summary>
/// Integration tests for the AdminGetActiveVideos endpoint.
/// </summary>
[Collection("Database")]
public class AdminGetActiveVideosEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task GetActiveVideos_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Videos}/active");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetActiveVideos_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Videos}/active");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetActiveVideos_AsAdmin_IsAllowed()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Videos}/active");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
