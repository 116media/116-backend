using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Identity.Infrastructure.Persistence.Seeds.Visitor;

namespace _116.Integration.Tests.Modules.Identity.Seeders;

/// <summary>
/// Integration tests for <see cref="VisitorRoleSeeder" />.
/// Verifies Visitor role and permission seeding against real PostgreSQL.
/// </summary>
[Collection("Database")]
public class VisitorRoleSeederTests(PostgresFixture postgres) : BaseRepositoryTest(postgres)
{
    [Fact]
    public async Task SeedAllAsync_ShouldCreateVisitorRole()
    {
        var seeder = Resolve<VisitorRoleSeeder>();

        await seeder.SeedAllAsync();

        await using var context = CreateDbContext<IdentityDbContext>();
        RoleEntity? visitor = await context
            .Roles.Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Name == "Visitor");

        visitor.Should().NotBeNull();
        visitor!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task SeedAllAsync_ShouldCreateAllVisitorPermissions()
    {
        var seeder = Resolve<VisitorRoleSeeder>();

        await seeder.SeedAllAsync();

        await using var context = CreateDbContext<IdentityDbContext>();
        RoleEntity visitor = await context
            .Roles.Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstAsync(r => r.Name == "Visitor");

        visitor.RolePermissions.Should().NotBeEmpty();
        visitor.RolePermissions.Should().AllSatisfy(rp => rp.Permission.Should().NotBeNull());
    }

    [Fact]
    public async Task SeedAllAsync_CalledTwice_ShouldBeIdempotent()
    {
        var seeder = Resolve<VisitorRoleSeeder>();

        await seeder.SeedAllAsync();
        await seeder.SeedAllAsync();

        await using var context = CreateDbContext<IdentityDbContext>();
        int count = await context.Roles.CountAsync(r => r.Name == "Visitor");

        count.Should().Be(1);
    }

    [Fact]
    public async Task SeedAllAsync_ShouldCreateActivePermissions()
    {
        var seeder = Resolve<VisitorRoleSeeder>();

        await seeder.SeedAllAsync();

        await using var context = CreateDbContext<IdentityDbContext>();
        List<PermissionEntity> permissions = await context.Permissions.ToListAsync();

        permissions.Should().AllSatisfy(p => p.IsActive.Should().BeTrue());
    }
}
