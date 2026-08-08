using _116.Identity.Application.Roles.UseCases.Admin.Commands.UpdateRole.V1;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Builders.Requests.Identity;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Commands.UpdateRole.V1;

/// <summary>
/// Integration tests for the AdminUpdateRole endpoint.
/// </summary>
[Collection("Database")]
public class AdminUpdateRoleEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string ShortName(string prefix = "r") => $"{prefix}{Guid.NewGuid().ToString("N")[..8]}";

    [Fact]
    public async Task UpdateRole_AsSuperAdmin_WithValidData_ReturnsSuccess()
    {
        RoleEntity role = await SeedAsync<IdentityDbContext, RoleEntity>(ctx =>
        {
            RoleEntity entity = RoleFactory.Create(ShortName("ub"), "Original description");
            ctx.Roles.Add(entity);
            return entity;
        });

        Client.AuthenticateAsSuperAdmin();
        AdminUpdateRoleRequest request = new AdminUpdateRoleRequestBuilder().Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Roles}/{role.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminUpdateRoleResponse>();
        body.Role.Id.Should().Be(role.Id);
        body.Role.Name.Should().Be(request.Name);
        body.Role.Description.Should().Be(request.Description);

        await using IdentityDbContext context = CreateDbContext<IdentityDbContext>();
        RoleEntity? updated = await context.Roles.FindAsync(role.Id);
        updated!.Name.Should().Be(request.Name);
        updated.Description.Should().Be(request.Description);
    }

    [Fact]
    public async Task UpdateRole_NonExistentRole_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();
        AdminUpdateRoleRequest request = new AdminUpdateRoleRequestBuilder().Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Roles}/{Guid.NewGuid()}", request);

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Role"))
        );
    }

    [Fact]
    public async Task UpdateRole_AsAdmin_ReturnsForbidden()
    {
        RoleEntity role = await SeedAsync<IdentityDbContext, RoleEntity>(ctx =>
        {
            RoleEntity entity = RoleFactory.Create(ShortName("uf"), "Admin cannot update");
            ctx.Roles.Add(entity);
            return entity;
        });

        Client.AuthenticateAsAdmin();
        AdminUpdateRoleRequest request = new AdminUpdateRoleRequestBuilder().Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Roles}/{role.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
