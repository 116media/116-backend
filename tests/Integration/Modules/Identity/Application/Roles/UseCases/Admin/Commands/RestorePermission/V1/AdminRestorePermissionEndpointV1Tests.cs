using System.Text.Json;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Commands.RestorePermission.V1;

/// <summary>
/// Integration tests for the AdminRestorePermission endpoint.
/// </summary>
[Collection("Database")]
public class AdminRestorePermissionEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
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
    public async Task RestorePermission_ShouldReturn200_WhenSuperAdminAfterSoftDelete()
    {
        Client.AuthenticateAsSuperAdmin();

        var createPayload = new
        {
            Resource = UniqueResource("rs"),
            Action = UniqueAction("rs"),
            Description = "To be restored",
        };

        var createResponse = await Client.PostAsJsonAsync(ApiRoutes.Admin.Permissions, createPayload);
        var createBody = await createResponse.Content.ReadAsStringAsync();
        using var createDoc = JsonDocument.Parse(createBody);
        var permissionId = createDoc.RootElement.GetProperty("permission").GetProperty("id").GetString();

        await Client.DeleteAsync($"{ApiRoutes.Admin.Permissions}/{permissionId}");

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Permissions}/{permissionId}/restore", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Verifies that restoring a permission that is not deleted returns a 409 Conflict.
    /// </summary>
    [Fact]
    public async Task RestorePermission_WhenNotDeleted_ReturnsConflict()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var permission = PermissionFactory.Create(UniqueResource("rn"), UniqueAction("rn"));
        seedContext.Permissions.Add(permission);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Permissions}/{permission.Id}/restore", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
