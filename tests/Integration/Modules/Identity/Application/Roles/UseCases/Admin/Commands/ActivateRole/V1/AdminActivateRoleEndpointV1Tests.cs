using _116.Identity.Application.Roles.UseCases.Admin.Commands.ActivateRole.V1;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Commands.ActivateRole.V1;

/// <summary>
/// Integration tests for the AdminActivateRole endpoint.
/// </summary>
[Collection("Database")]
public class AdminActivateRoleEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string ShortName(string prefix = "r") => $"{prefix}{Guid.NewGuid().ToString("N")[..8]}";

    private async Task<bool> IsRoleActiveAsync(Guid id)
    {
        await using IdentityDbContext ctx = CreateDbContext<IdentityDbContext>();
        RoleEntity? role = await ctx.Roles.FindAsync(id);
        return role!.IsActive;
    }

    [Fact]
    public async Task ActivateRole_AsSuperAdmin_ReturnsSuccess()
    {
        RoleEntity role = await SeedAsync<IdentityDbContext, RoleEntity>(ctx =>
        {
            RoleEntity entity = RoleFactory.CreateInactive(ShortName("ac"));
            ctx.Roles.Add(entity);
            return entity;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(Routes.Admin.Roles.Activate(role.Id), null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminActivateRoleResponse>();
        body.Role.Id.Should().Be(role.Id);
        body.Role.IsActive.Should().BeTrue();

        (await IsRoleActiveAsync(role.Id)).Should().BeTrue();
    }

    [Fact]
    public async Task ActivateRole_NonExistentRole_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(Routes.Admin.Roles.Activate(Guid.NewGuid()), null);

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Verifies that activating a role that is already active returns a 409 Conflict.
    /// </summary>
    [Fact]
    public async Task ActivateRole_WhenAlreadyActive_ReturnsConflict()
    {
        RoleEntity role = await SeedAsync<IdentityDbContext, RoleEntity>(ctx =>
        {
            RoleEntity entity = RoleFactory.Create(ShortName("aa"));
            ctx.Roles.Add(entity);
            return entity;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(Routes.Admin.Roles.Activate(role.Id), null);

        await response.ShouldBeProblem(HttpStatusCode.Conflict);
        (await IsRoleActiveAsync(role.Id)).Should().BeTrue();
    }
}
