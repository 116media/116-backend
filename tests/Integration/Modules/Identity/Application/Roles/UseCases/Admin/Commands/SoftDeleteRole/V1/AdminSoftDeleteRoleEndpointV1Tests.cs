using _116.Identity.Application.Roles.UseCases.Admin.Commands.SoftDeleteRole.V1;
using _116.Identity.Application.Shared.Errors.Messages;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Commands.SoftDeleteRole.V1;

/// <summary>
/// Integration tests for the AdminSoftDeleteRole endpoint.
/// </summary>
[Collection("Database")]
public class AdminSoftDeleteRoleEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string ShortName(string prefix = "r") => $"{prefix}{Guid.NewGuid().ToString("N")[..8]}";

    private async Task<bool> IsRoleDeletedAsync(Guid id)
    {
        await using IdentityDbContext ctx = CreateDbContext<IdentityDbContext>();
        RoleEntity? role = await ctx.Roles.FindAsync(id);
        return role!.IsDeleted;
    }

    [Fact]
    public async Task SoftDeleteRole_AsSuperAdmin_ReturnsSuccess()
    {
        RoleEntity role = await SeedAsync<IdentityDbContext, RoleEntity>(ctx =>
        {
            RoleEntity entity = RoleFactory.Create(ShortName("sd"), "Will be soft deleted");
            ctx.Roles.Add(entity);
            return entity;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.DeleteAsync($"{ApiRoutes.Admin.Roles}/{role.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminSoftDeleteRoleResponse>();
        body.IsSuccess.Should().BeTrue();
        body.Role.Id.Should().Be(role.Id);
        body.Role.IsDeleted.Should().BeTrue();

        (await IsRoleDeletedAsync(role.Id)).Should().BeTrue();
    }

    [Fact]
    public async Task SoftDeleteRole_WhenAlreadyDeleted_ReturnsConflict()
    {
        RoleEntity role = await SeedAsync<IdentityDbContext, RoleEntity>(ctx =>
        {
            RoleEntity entity = RoleFactory.CreateDeleted(ShortName("dd"));
            ctx.Roles.Add(entity);
            return entity;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.DeleteAsync($"{ApiRoutes.Admin.Roles}/{role.Id}");

        await response.ShouldBeProblem<ConflictException>(
            HttpStatusCode.Conflict,
            Localized<ConflictErrorMessage>(m => m.RoleAlreadyDeleted())
        );
        (await IsRoleDeletedAsync(role.Id)).Should().BeTrue();
    }
}
