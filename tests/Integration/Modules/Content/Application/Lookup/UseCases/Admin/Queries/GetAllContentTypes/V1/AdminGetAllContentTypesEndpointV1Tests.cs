namespace _116.Integration.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Queries.GetAllContentTypes.V1;

/// <summary>
/// Integration tests for the AdminGetAllContentTypes endpoint.
/// </summary>
[Collection("Database")]
public class AdminGetAllContentTypesEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task GetAllContentTypes_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync(ApiRoutes.Admin.ContentTypes);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAllContentTypes_AsAdmin_ReturnsOk()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.GetAsync(ApiRoutes.Admin.ContentTypes);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAllContentTypes_AsSuperAdmin_ReturnsOk()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync(ApiRoutes.Admin.ContentTypes);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAllContentTypes_WithSearchParam_ReturnsOk()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.ContentTypes}?search=Article");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
