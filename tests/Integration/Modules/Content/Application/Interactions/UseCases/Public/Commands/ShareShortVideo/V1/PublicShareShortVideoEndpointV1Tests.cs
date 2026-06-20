namespace _116.Integration.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.ShareShortVideo.V1;

/// <summary>
/// Integration tests for the PublicShareShortVideo endpoint.
/// </summary>
[Collection("Database")]
public class PublicShareShortVideoEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task ShareShortVideo_AsAnonymous_ReturnsOk()
    {
        Client.ClearAuthentication();

        var response = await Client.PostAsync($"{ApiRoutes.Public.Shorts}/{Guid.NewGuid()}/shares", null);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShareShortVideo_AsVisitor_ReturnsOk()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PostAsync($"{ApiRoutes.Public.Shorts}/{Guid.NewGuid()}/shares", null);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }
}
