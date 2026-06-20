namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Queries.GetAllShorts.V1;

/// <summary>
/// Integration tests for the AdminGetAllShorts endpoint.
/// </summary>
[Collection("Database")]
public class AdminGetAllShortsEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task GetAllShorts_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync(ApiRoutes.Admin.Shorts);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAllShorts_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync(ApiRoutes.Admin.Shorts);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAllShorts_AsAdmin_IsAllowed()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.GetAsync(ApiRoutes.Admin.Shorts);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
