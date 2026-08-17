using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Identity.Infrastructure.Persistence.Seeds.SuperAdmin;

namespace _116.Integration.Tests.Modules.Identity.Seeders;

/// <summary>
/// Integration tests for <see cref="SuperAdminSeeder" />.
/// Verifies SuperAdmin user seeding with real PostgreSQL and full DI pipeline.
/// </summary>
[Collection("Database")]
public class SuperAdminSeederTests(PostgresFixture postgres) : BaseRepositoryTest(postgres)
{
    [Fact]
    public async Task SeedAllAsync_ShouldCreateSuperAdminUser()
    {
        var seeder = Resolve<SuperAdminSeeder>();

        await seeder.SeedAllAsync();

        await using var context = CreateDbContext<IdentityDbContext>();
        UserEntity? superAdmin = await context
            .Users.Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.UserRoles.Any(ur => ur.Role.Name == "SuperAdmin"));

        superAdmin.Should().NotBeNull();
        superAdmin!.IsActive.Should().BeTrue();
        superAdmin.IsVerified.Should().BeTrue();
    }

    [Fact]
    public async Task SeedAllAsync_ShouldCreateTheSuperAdminTokenState()
    {
        var seeder = Resolve<SuperAdminSeeder>();

        await seeder.SeedAllAsync();

        await using var context = CreateDbContext<IdentityDbContext>();
        UserEntity superAdmin = await context
            .Users.Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstAsync(u => u.UserRoles.Any(ur => ur.Role.Name == "SuperAdmin"));

        UserTokenStateEntity? tokenState = await context.UserTokenStates.FirstOrDefaultAsync(s =>
            s.Id == superAdmin.Id
        );

        tokenState.Should().NotBeNull();
        tokenState!.SecurityStamp.Should().NotBe(Guid.Empty);
        tokenState.TokenVersion.Should().Be(0);
    }

    [Fact]
    public async Task SeedAllAsync_ShouldAssignSuperAdminRole()
    {
        var seeder = Resolve<SuperAdminSeeder>();

        await seeder.SeedAllAsync();

        await using var context = CreateDbContext<IdentityDbContext>();
        UserEntity superAdmin = await context
            .Users.Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstAsync(u => u.UserRoles.Any(ur => ur.Role.Name == "SuperAdmin"));

        superAdmin.UserRoles.Should().ContainSingle(ur => ur.Role.Name == "SuperAdmin");
    }

    [Fact]
    public async Task SeedAllAsync_ShouldHashPassword()
    {
        var seeder = Resolve<SuperAdminSeeder>();

        await seeder.SeedAllAsync();

        await using var context = CreateDbContext<IdentityDbContext>();
        UserEntity superAdmin = await context
            .Users.Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstAsync(u => u.UserRoles.Any(ur => ur.Role.Name == "SuperAdmin"));

        superAdmin.PasswordHash.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task SeedAllAsync_CalledTwice_ShouldBeIdempotent()
    {
        var seeder = Resolve<SuperAdminSeeder>();

        await seeder.SeedAllAsync();
        await seeder.SeedAllAsync();

        await using var context = CreateDbContext<IdentityDbContext>();
        int count = await context
            .Users.Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .CountAsync(u => u.UserRoles.Any(ur => ur.Role.Name == "SuperAdmin"));

        count.Should().Be(1);
    }

    [Fact]
    public async Task SeedAllAsync_ShouldCreateSuperAdminRole()
    {
        var seeder = Resolve<SuperAdminSeeder>();

        await seeder.SeedAllAsync();

        await using var context = CreateDbContext<IdentityDbContext>();
        RoleEntity? role = await context.Roles.FirstOrDefaultAsync(r => r.Name == "SuperAdmin");

        role.Should().NotBeNull();
        role!.IsActive.Should().BeTrue();
    }
}
