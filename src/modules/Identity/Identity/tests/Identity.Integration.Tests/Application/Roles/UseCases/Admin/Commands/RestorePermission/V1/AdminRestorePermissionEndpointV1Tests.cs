using _116.BuildingBlocks.Application.Exceptions;
using _116.Identity.Application.Roles.UseCases.Admin.Commands.RestorePermission.V1;
using _116.Identity.Application.Shared.Errors.Messages;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Identity.TestData.Factories;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Identity.TestData.Mocks.Repositories;
using _116.Identity.TestData.Mocks.Services;
using _116.Storage.TestData.Factories;
using _116.Storage.TestData.Mocks.Infrastructure;
using _116.Storage.TestData.Mocks.Services;
using _116.Tests.TestData.Mocks;

namespace _116.Identity.Integration.Tests.Application.Roles.UseCases.Admin.Commands.RestorePermission.V1;

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

    private async Task<bool> IsPermissionDeletedAsync(Guid id)
    {
        await using IdentityDbContext ctx = CreateDbContext<IdentityDbContext>();
        PermissionEntity? permission = await ctx.Permissions.FindAsync(id);
        return permission!.IsDeleted;
    }

    [Fact]
    public async Task RestorePermission_ShouldReturn200_WhenSuperAdminAfterSoftDelete()
    {
        PermissionEntity permission = await SeedAsync<IdentityDbContext, PermissionEntity>(ctx =>
        {
            PermissionEntity entity = PermissionFactory.CreateDeleted();
            ctx.Permissions.Add(entity);
            return entity;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(Routes.Admin.Permissions.Restore(permission.Id), null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminRestorePermissionResponse>();
        body.Permission.Id.Should().Be(permission.Id);
        body.Permission.IsDeleted.Should().BeFalse();

        (await IsPermissionDeletedAsync(permission.Id)).Should().BeFalse();
    }

    [Fact]
    public async Task RestorePermission_WhenNotDeleted_ReturnsConflict()
    {
        PermissionEntity permission = await SeedAsync<IdentityDbContext, PermissionEntity>(ctx =>
        {
            PermissionEntity entity = PermissionFactory.Create(UniqueResource("rn"), UniqueAction("rn"));
            ctx.Permissions.Add(entity);
            return entity;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(Routes.Admin.Permissions.Restore(permission.Id), null);

        await response.ShouldBeProblem<ConflictException>(
            HttpStatusCode.Conflict,
            Localized<ConflictErrorMessage>(m => m.PermissionNotDeleted())
        );
        (await IsPermissionDeletedAsync(permission.Id)).Should().BeFalse();
    }
}
