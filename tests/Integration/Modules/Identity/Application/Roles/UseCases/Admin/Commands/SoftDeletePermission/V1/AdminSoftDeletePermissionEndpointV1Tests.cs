using System.Text.Json;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Commands.SoftDeletePermission.V1;

/// <summary>
/// Integration tests for the AdminSoftDeletePermission endpoint.
/// </summary>
[Collection("Database")]
public class AdminSoftDeletePermissionEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
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
    public async Task SoftDeletePermission_ShouldReturn200_WhenSuperAdmin()
    {
        Client.AuthenticateAsSuperAdmin();

        var createPayload = new
        {
            Resource = UniqueResource("sd"),
            Action = UniqueAction("sd"),
            Description = "To be soft deleted",
        };

        var createResponse = await Client.PostAsJsonAsync(ApiRoutes.Admin.Permissions, createPayload);
        var createBody = await createResponse.Content.ReadAsStringAsync();
        using var createDoc = JsonDocument.Parse(createBody);
        var permissionId = createDoc.RootElement.GetProperty("permission").GetProperty("id").GetString();

        var response = await Client.DeleteAsync($"{ApiRoutes.Admin.Permissions}/{permissionId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
