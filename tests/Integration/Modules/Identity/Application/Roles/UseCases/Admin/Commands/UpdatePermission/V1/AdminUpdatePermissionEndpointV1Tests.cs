using System.Text.Json;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Commands.UpdatePermission.V1;

/// <summary>
/// Integration tests for the AdminUpdatePermission endpoint.
/// </summary>
[Collection("Database")]
public class AdminUpdatePermissionEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    /// <summary>
    /// Generates a unique resource name that fits the 15-char max length.
    /// </summary>
    private static string UniqueResource(string prefix = "pt") => $"{prefix}_{Guid.NewGuid().ToString("N")[..8]}";

    /// <summary>
    /// Generates a unique action name that fits the 15-char max length.
    /// </summary>
    private static string UniqueAction(string prefix = "act") => $"{prefix}_{Guid.NewGuid().ToString("N")[..8]}";

    [Fact]
    public async Task UpdatePermission_ShouldReturn200_WhenSuperAdminWithValidData()
    {
        Client.AuthenticateAsSuperAdmin();

        var createPayload = new
        {
            Resource = UniqueResource("up"),
            Action = UniqueAction("up"),
            Description = "To be updated",
        };

        var createResponse = await Client.PostAsJsonAsync(ApiRoutes.Admin.Permissions, createPayload);
        var createBody = await createResponse.Content.ReadAsStringAsync();
        using var createDoc = JsonDocument.Parse(createBody);
        var permissionId = createDoc.RootElement.GetProperty("permission").GetProperty("id").GetString();

        var updatePayload = new
        {
            Resource = UniqueResource("upd"),
            Action = UniqueAction("upd"),
            Description = "Updated description",
        };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Permissions}/{permissionId}", updatePayload);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdatePermission_ShouldReturn404_WhenNonExistent()
    {
        Client.AuthenticateAsSuperAdmin();

        var nonExistentId = Guid.NewGuid();
        var updatePayload = new
        {
            Resource = UniqueResource("nf"),
            Action = UniqueAction("nf"),
            Description = "Should not be found",
        };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Permissions}/{nonExistentId}", updatePayload);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
