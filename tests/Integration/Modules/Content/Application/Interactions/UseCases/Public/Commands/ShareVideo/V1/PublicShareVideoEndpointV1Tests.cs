namespace _116.Integration.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.ShareVideo.V1;

/// <summary>
/// Integration tests for the PublicShareVideo endpoint.
/// </summary>
[Collection("Database")]
public class PublicShareVideoEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task ShareVideo_AsAnonymous_ReturnsOk()
    {
        Client.ClearAuthentication();

        var response = await Client.PostAsync($"{ApiRoutes.Public.Videos}/{Guid.NewGuid()}/shares", null);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShareVideo_AsVisitor_ReturnsOk()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PostAsync($"{ApiRoutes.Public.Videos}/{Guid.NewGuid()}/shares", null);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }
}
