namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.RejectVideo.V1;

/// <summary>
/// Integration tests for the AdminRejectVideo endpoint.
/// </summary>
[Collection("Database")]
public class AdminRejectVideoEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task RejectVideo_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PatchAsJsonAsync(
            $"{ApiRoutes.Admin.Videos}/{nonExistentId}/reject",
            new { Reason = "test" }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RejectVideo_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PatchAsJsonAsync(
            $"{ApiRoutes.Admin.Videos}/{nonExistentId}/reject",
            new { Reason = "test" }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RejectVideo_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PatchAsJsonAsync(
            $"{ApiRoutes.Admin.Videos}/{nonExistentId}/reject",
            new { Reason = "test" }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RejectVideo_AsSuperAdmin_WithNonExistentId_ReturnsError()
    {
        Client.AuthenticateAsSuperAdmin();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PatchAsJsonAsync(
            $"{ApiRoutes.Admin.Videos}/{nonExistentId}/reject",
            new { Reason = "test" }
        );

        response
            .StatusCode.Should()
            .BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.UnprocessableEntity);
    }
}
