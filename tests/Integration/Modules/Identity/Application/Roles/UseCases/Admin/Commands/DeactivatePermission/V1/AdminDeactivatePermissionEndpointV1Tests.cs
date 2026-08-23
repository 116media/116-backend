using _116.Identity.Application.Roles.UseCases.Admin.Commands.DeactivatePermission.V1;
using _116.Identity.Application.Shared.Errors.Messages;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Commands.DeactivatePermission.V1;

/// <summary>
/// Integration tests for the AdminDeactivatePermission endpoint.
/// </summary>
[Collection("Database")]
public class AdminDeactivatePermissionEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    /// <summary>
    /// Generates a unique resource name that fits the 15-char max length.
    /// </summary>
    private static string UniqueResource(string prefix = "pt") => $"{prefix}_{Guid.NewGuid().ToString("N")[..8]}";

    /// <summary>
    /// Generates a unique action name that fits the 15-char max length.
    /// </summary>
    private static string UniqueAction(string prefix = "act") => $"{prefix}_{Guid.NewGuid().ToString("N")[..8]}";

    private async Task<bool> IsPermissionActiveAsync(Guid id)
    {
        await using IdentityDbContext ctx = CreateDbContext<IdentityDbContext>();
        PermissionEntity? permission = await ctx.Permissions.FindAsync(id);
        return permission!.IsActive;
    }

    [Fact]
    public async Task DeactivatePermission_ShouldReturn200_WhenSuperAdmin()
    {
        PermissionEntity permission = await SeedAsync<IdentityDbContext, PermissionEntity>(ctx =>
        {
            PermissionEntity entity = PermissionFactory.Create(UniqueResource("da"), UniqueAction("da"));
            ctx.Permissions.Add(entity);
            return entity;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(Routes.Admin.Permissions.Deactivate(permission.Id), null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminDeactivatePermissionResponse>();
        body.Permission.Id.Should().Be(permission.Id);
        body.Permission.IsActive.Should().BeFalse();

        (await IsPermissionActiveAsync(permission.Id)).Should().BeFalse();
    }

    [Fact]
    public async Task DeactivatePermission_WhenAlreadyInactive_ReturnsConflict()
    {
        PermissionEntity permission = await SeedAsync<IdentityDbContext, PermissionEntity>(ctx =>
        {
            PermissionEntity entity = PermissionFactory.CreateInactive();
            ctx.Permissions.Add(entity);
            return entity;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(Routes.Admin.Permissions.Deactivate(permission.Id), null);

        await response.ShouldBeProblem<ConflictException>(
            HttpStatusCode.Conflict,
            Localized<ConflictErrorMessage>(m => m.PermissionAlreadyInactive())
        );
        (await IsPermissionActiveAsync(permission.Id)).Should().BeFalse();
    }
}
