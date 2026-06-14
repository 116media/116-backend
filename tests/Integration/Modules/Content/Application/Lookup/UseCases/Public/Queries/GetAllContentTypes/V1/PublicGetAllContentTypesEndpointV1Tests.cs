namespace _116.Integration.Tests.Modules.Content.Application.Lookup.UseCases.Public.Queries.GetAllContentTypes.V1;

/// <summary>
/// Integration tests for the PublicGetAllContentTypes endpoint.
/// </summary>
[Collection("Database")]
public class PublicGetAllContentTypesEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task GetAllContentTypes_AsAnonymous_ReturnsOk()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync(ApiRoutes.Public.ContentTypes);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAllContentTypes_AsVisitor_ReturnsOk()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync(ApiRoutes.Public.ContentTypes);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
