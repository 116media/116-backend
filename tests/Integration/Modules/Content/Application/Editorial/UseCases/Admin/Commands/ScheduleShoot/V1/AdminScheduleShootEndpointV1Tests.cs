namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.ScheduleShoot.V1;

/// <summary>
/// Integration tests for the AdminScheduleShoot endpoint.
/// </summary>
[Collection("Database")]
public class AdminScheduleShootEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task ScheduleShoot_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PatchAsJsonAsync(
            $"{ApiRoutes.Admin.Videos}/{nonExistentId}/shoot",
            new { Reason = "test" }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ScheduleShoot_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PatchAsJsonAsync(
            $"{ApiRoutes.Admin.Videos}/{nonExistentId}/shoot",
            new { Reason = "test" }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ScheduleShoot_AsAdmin_IsAllowed()
    {
        Client.AuthenticateAsAdmin();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PatchAsJsonAsync(
            $"{ApiRoutes.Admin.Videos}/{nonExistentId}/shoot",
            new { Reason = "test" }
        );

        response
            .StatusCode.Should()
            .BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.UnprocessableEntity);
    }
}
