using System.Text.Json;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Commands.HardDeletePermission.V1;

/// <summary>
/// Integration tests for the AdminHardDeletePermission endpoint.
/// </summary>
[Collection("Database")]
public class AdminHardDeletePermissionEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
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
    public async Task HardDeletePermission_ShouldReturn200_AndRemoveFromDatabase()
    {
        Client.AuthenticateAsSuperAdmin();

        var createPayload = new
        {
            Resource = UniqueResource("hd"),
            Action = UniqueAction("hd"),
            Description = "To be hard deleted",
        };

        var createResponse = await Client.PostAsJsonAsync(ApiRoutes.Admin.Permissions, createPayload);
        var createBody = await createResponse.Content.ReadAsStringAsync();
        using var createDoc = JsonDocument.Parse(createBody);
        var permissionId = Guid.Parse(createDoc.RootElement.GetProperty("permission").GetProperty("id").GetString()!);

        var response = await Client.DeleteAsync($"{ApiRoutes.Admin.Permissions}/{permissionId}/hard");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using var context = CreateDbContext<IdentityDbContext>();
        var exists = await context.Permissions.AnyAsync(p => p.Id == permissionId);
        exists.Should().BeFalse();
    }
}
