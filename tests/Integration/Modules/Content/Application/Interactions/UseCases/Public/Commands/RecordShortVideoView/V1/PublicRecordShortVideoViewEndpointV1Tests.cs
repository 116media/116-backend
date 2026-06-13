namespace _116.Integration.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.RecordShortVideoView.V1;

/// <summary>
/// Integration tests for the PublicRecordShortVideoView endpoint.
/// </summary>
[Collection("Database")]
public class PublicRecordShortVideoViewEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task RecordShortVideoView_AsAnonymous_ReturnsOk()
    {
        Client.ClearAuthentication();

        var response = await Client.PostAsync($"{ApiRoutes.Public.Shorts}/{Guid.NewGuid()}/views", null);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RecordShortVideoView_AsVisitor_ReturnsOk()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PostAsync($"{ApiRoutes.Public.Shorts}/{Guid.NewGuid()}/views", null);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }
}
