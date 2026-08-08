using _116.Identity.Application.Roles.UseCases.Admin.Commands.DeactivateRole.V1;
using _116.Identity.Application.Shared.Errors.Messages;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Commands.DeactivateRole.V1;

/// <summary>
/// Integration tests for the AdminDeactivateRole endpoint.
/// </summary>
[Collection("Database")]
public class AdminDeactivateRoleEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string ShortName(string prefix = "r") => $"{prefix}{Guid.NewGuid().ToString("N")[..8]}";

    private async Task<bool> IsRoleActiveAsync(Guid id)
    {
        await using IdentityDbContext ctx = CreateDbContext<IdentityDbContext>();
        RoleEntity? role = await ctx.Roles.FindAsync(id);
        return role!.IsActive;
    }

    [Fact]
    public async Task DeactivateRole_AsSuperAdmin_ReturnsSuccess()
    {
        RoleEntity role = await SeedAsync<IdentityDbContext, RoleEntity>(ctx =>
        {
            RoleEntity entity = RoleFactory.Create(ShortName("da"), "Will be deactivated");
            ctx.Roles.Add(entity);
            return entity;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(Routes.Admin.Roles.Deactivate(role.Id), null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminDeactivateRoleResponse>();
        body.Role.Id.Should().Be(role.Id);
        body.Role.IsActive.Should().BeFalse();

        (await IsRoleActiveAsync(role.Id)).Should().BeFalse();
    }

    [Fact]
    public async Task DeactivateRole_WhenAlreadyInactive_ReturnsConflict()
    {
        RoleEntity role = await SeedAsync<IdentityDbContext, RoleEntity>(ctx =>
        {
            RoleEntity entity = RoleFactory.CreateInactive(ShortName("di"));
            ctx.Roles.Add(entity);
            return entity;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(Routes.Admin.Roles.Deactivate(role.Id), null);

        await response.ShouldBeProblem<ConflictException>(
            HttpStatusCode.Conflict,
            Localized<ConflictErrorMessage>(m => m.RoleAlreadyInactive())
        );
        (await IsRoleActiveAsync(role.Id)).Should().BeFalse();
    }
}
