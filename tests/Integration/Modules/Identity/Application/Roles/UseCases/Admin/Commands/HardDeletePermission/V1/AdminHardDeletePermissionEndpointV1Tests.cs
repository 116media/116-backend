using _116.Identity.Application.Roles.UseCases.Admin.Commands.HardDeletePermission.V1;
using _116.Identity.Domain.Entities;
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
        PermissionEntity permission = await SeedAsync<IdentityDbContext, PermissionEntity>(ctx =>
        {
            PermissionEntity entity = PermissionFactory.Create(UniqueResource("hd"), UniqueAction("hd"));
            ctx.Permissions.Add(entity);
            return entity;
        });
        Guid permissionId = permission.Id;

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.DeleteAsync(Routes.Admin.Permissions.Hard(permissionId));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminHardDeletePermissionResponse>();
        body.IsSuccess.Should().BeTrue();

        await using IdentityDbContext context = CreateDbContext<IdentityDbContext>();
        bool exists = await context.Permissions.AnyAsync(p => p.Id == permissionId);
        exists.Should().BeFalse();
    }
}
