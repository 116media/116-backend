using System.Text.Json;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Queries.GetPermissionById.V1;

/// <summary>
/// Integration tests for the AdminGetPermissionById endpoint.
/// </summary>
[Collection("Database")]
public class AdminGetPermissionByIdEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
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
    public async Task GetPermissionById_ShouldReturn200_WhenAdminAndExists()
    {
        Client.AuthenticateAsSuperAdmin();

        var createPayload = new
        {
            Resource = UniqueResource("gi"),
            Action = UniqueAction("gi"),
            Description = "Seeded for get by id",
        };

        var createResponse = await Client.PostAsJsonAsync(ApiRoutes.Admin.Permissions, createPayload);
        var createBody = await createResponse.Content.ReadAsStringAsync();
        using var createDoc = JsonDocument.Parse(createBody);
        var permissionId = createDoc.RootElement.GetProperty("permission").GetProperty("id").GetString();

        Client.AuthenticateAsAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Permissions}/{permissionId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetPermissionById_ShouldReturn404_WhenNonExistentGuid()
    {
        Client.AuthenticateAsAdmin();

        var nonExistentId = Guid.NewGuid();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Permissions}/{nonExistentId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
