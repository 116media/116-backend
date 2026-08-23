using _116.Identity.Application.Roles.UseCases.Admin.Commands.RestoreRole.V1;
using _116.Identity.Application.Shared.Errors.Messages;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Commands.RestoreRole.V1;

/// <summary>
/// Integration tests for the AdminRestoreRole endpoint.
/// </summary>
[Collection("Database")]
public class AdminRestoreRoleEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string ShortName(string prefix = "r") => $"{prefix}{Guid.NewGuid().ToString("N")[..8]}";

    private async Task<bool> IsRoleDeletedAsync(Guid id)
    {
        await using IdentityDbContext ctx = CreateDbContext<IdentityDbContext>();
        RoleEntity? role = await ctx.Roles.FindAsync(id);
        return role!.IsDeleted;
    }

    [Fact]
    public async Task RestoreRole_AsSuperAdmin_AfterSoftDelete_ReturnsSuccess()
    {
        RoleEntity role = await SeedAsync<IdentityDbContext, RoleEntity>(ctx =>
        {
            RoleEntity entity = RoleFactory.CreateDeleted(ShortName("rs"));
            ctx.Roles.Add(entity);
            return entity;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(Routes.Admin.Roles.Restore(role.Id), null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminRestoreRoleResponse>();
        body.Role.Id.Should().Be(role.Id);
        body.Role.IsDeleted.Should().BeFalse();

        (await IsRoleDeletedAsync(role.Id)).Should().BeFalse();
    }

    [Fact]
    public async Task RestoreRole_WhenNotDeleted_ReturnsConflict()
    {
        RoleEntity role = await SeedAsync<IdentityDbContext, RoleEntity>(ctx =>
        {
            RoleEntity entity = RoleFactory.Create(ShortName("rn"));
            ctx.Roles.Add(entity);
            return entity;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(Routes.Admin.Roles.Restore(role.Id), null);

        await response.ShouldBeProblem<ConflictException>(
            HttpStatusCode.Conflict,
            Localized<ConflictErrorMessage>(m => m.RoleNotDeleted())
        );
        (await IsRoleDeletedAsync(role.Id)).Should().BeFalse();
    }
}
