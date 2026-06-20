using System.Text.Json;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Commands.ActivatePermission.V1;

/// <summary>
/// Integration tests for the AdminActivatePermission endpoint.
/// </summary>
[Collection("Database")]
public class AdminActivatePermissionEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
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
    public async Task ActivatePermission_ShouldReturn200_WhenSuperAdmin()
    {
        Client.AuthenticateAsSuperAdmin();

        var createPayload = new
        {
            Resource = UniqueResource("ac"),
            Action = UniqueAction("ac"),
            Description = "To be activated",
        };

        var createResponse = await Client.PostAsJsonAsync(ApiRoutes.Admin.Permissions, createPayload);
        var createBody = await createResponse.Content.ReadAsStringAsync();
        using var createDoc = JsonDocument.Parse(createBody);
        var permissionId = createDoc.RootElement.GetProperty("permission").GetProperty("id").GetString();

        await Client.PatchAsync($"{ApiRoutes.Admin.Permissions}/{permissionId}/deactivate", null);

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Permissions}/{permissionId}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
