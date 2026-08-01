using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Infrastructure.Repositories;

/// <summary>
/// Integration tests for <see cref="IUserRoleRepository"/> verifying
/// junction record operations against a real PostgreSQL database.
/// </summary>
[Collection("Database")]
public class UserRoleRepositoryTests(PostgresFixture postgres) : BaseRepositoryTest(postgres)
{
    [Fact]
    public async Task ExistsByUserAndRoleAsync_ExistingAssociation_ShouldReturnTrue()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var user = UserFactory.CreateVerifiedActive();
        var role = RoleFactory.Create();
        seedContext.Users.Add(user);
        seedContext.Roles.Add(role);
        await seedContext.SaveChangesAsync();

        var userRole = UserRoleEntity.CreateBootstrap(Guid.NewGuid(), user.Id, role.Id);
        seedContext.UserRoles.Add(userRole);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IUserRoleRepository>();

        var exists = await repo.ExistsByUserAndRoleAsync(user.Id, role.Id);

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByUserAndRoleAsync_NonExistentAssociation_ShouldReturnFalse()
    {
        var repo = Resolve<IUserRoleRepository>();

        var exists = await repo.ExistsByUserAndRoleAsync(Guid.NewGuid(), Guid.NewGuid());

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task GetByUserAndRoleAsync_ExistingAssociation_ShouldReturnEntity()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var user = UserFactory.CreateVerifiedActive();
        var role = RoleFactory.Create();
        seedContext.Users.Add(user);
        seedContext.Roles.Add(role);
        await seedContext.SaveChangesAsync();

        var userRole = UserRoleEntity.CreateBootstrap(Guid.NewGuid(), user.Id, role.Id);
        seedContext.UserRoles.Add(userRole);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IUserRoleRepository>();

        var result = await repo.GetByUserAndRoleAsync(user.Id, role.Id);

        result.Should().NotBeNull();
        result!.UserId.Should().Be(user.Id);
        result.RoleId.Should().Be(role.Id);
    }

    [Fact]
    public async Task GetByUserAndRoleAsync_NonExistentAssociation_ShouldReturnNull()
    {
        var repo = Resolve<IUserRoleRepository>();

        var result = await repo.GetByUserAndRoleAsync(Guid.NewGuid(), Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_ShouldPersistJunctionRecord()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var user = UserFactory.CreateVerifiedActive();
        var role = RoleFactory.Create();
        seedContext.Users.Add(user);
        seedContext.Roles.Add(role);
        await seedContext.SaveChangesAsync();

        var (repo, db) = CreateScopedRepository<IUserRoleRepository, IdentityDbContext>();

        var userRole = UserRoleEntity.CreateBootstrap(Guid.NewGuid(), user.Id, role.Id);
        await repo.AddAsync(userRole);
        await db.SaveChangesAsync();

        await using var verifyContext = CreateDbContext<IdentityDbContext>();
        var saved = await verifyContext.UserRoles.FindAsync(userRole.Id);

        saved.Should().NotBeNull();
        saved!.UserId.Should().Be(user.Id);
        saved.RoleId.Should().Be(role.Id);
    }

    [Fact]
    public async Task Delete_ShouldRemoveJunctionRecord()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var user = UserFactory.CreateVerifiedActive();
        var role = RoleFactory.Create();
        seedContext.Users.Add(user);
        seedContext.Roles.Add(role);
        await seedContext.SaveChangesAsync();

        var userRole = UserRoleEntity.CreateBootstrap(Guid.NewGuid(), user.Id, role.Id);
        seedContext.UserRoles.Add(userRole);
        await seedContext.SaveChangesAsync();

        var (repo, db) = CreateScopedRepository<IUserRoleRepository, IdentityDbContext>();
        var toDelete = await db.UserRoles.FindAsync(userRole.Id);
        toDelete.Should().NotBeNull();

        repo.Delete(toDelete!);
        await db.SaveChangesAsync();

        await using var verifyContext = CreateDbContext<IdentityDbContext>();
        var deleted = await verifyContext.UserRoles.FindAsync(userRole.Id);

        deleted.Should().BeNull();
    }

    [Fact]
    public async Task GetUserRolesWithRoleAsync_ShouldReturnRolesWithRoleNavigationLoaded()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var user = UserFactory.CreateVerifiedActive();
        var roleA = RoleFactory.Create("testroleA");
        var roleB = RoleFactory.Create("testroleB");
        seedContext.Users.Add(user);
        seedContext.Roles.AddRange(roleA, roleB);
        await seedContext.SaveChangesAsync();

        var userRoleA = UserRoleEntity.CreateBootstrap(Guid.NewGuid(), user.Id, roleA.Id);
        var userRoleB = UserRoleEntity.CreateBootstrap(Guid.NewGuid(), user.Id, roleB.Id);
        seedContext.UserRoles.AddRange(userRoleA, userRoleB);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IUserRoleRepository>();

        var results = await repo.GetUserRolesWithRoleAsync(user.Id);

        results.Should().HaveCount(2);
        results
            .Should()
            .AllSatisfy(ur =>
            {
                ur.Role.Should().NotBeNull();
                ur.UserId.Should().Be(user.Id);
            });
        results.Select(ur => ur.Role.Name).Should().Contain("testroleA");
        results.Select(ur => ur.Role.Name).Should().Contain("testroleB");
    }
}
