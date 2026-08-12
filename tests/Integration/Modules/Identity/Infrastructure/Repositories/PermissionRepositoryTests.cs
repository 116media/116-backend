using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Infrastructure.Repositories;

/// <summary>
/// Integration tests for <see cref="IPermissionRepository"/> verifying
/// CRUD operations, pagination, filtering, and error handling against
/// a real PostgreSQL database.
/// </summary>
[Collection("Database")]
public class PermissionRepositoryTests(PostgresFixture postgres) : BaseRepositoryTest(postgres)
{
    [Fact]
    public async Task GetAllWithPaginationAsync_ShouldReturnPaginatedResults()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var permissions = PermissionFactory.CreateMany(5);
        seedContext.Permissions.AddRange(permissions);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IPermissionRepository>();

        var (result, totalCount) = await repo.GetAllWithPaginationAsync(1, 3);

        totalCount.Should().Be(5);
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetPermissionByIdOrThrowAsync_ExistingId_ShouldReturnEntity()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var permission = PermissionFactory.Create("users", "read");
        seedContext.Permissions.Add(permission);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IPermissionRepository>();

        var result = await repo.GetPermissionByIdOrThrowAsync(permission.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(permission.Id);
        result.Resource.Should().Be("users");
        result.Action.Should().Be("read");
    }

    [Fact]
    public async Task GetPermissionByIdOrThrowAsync_NonExistentId_ShouldThrowNotFoundException()
    {
        var repo = Resolve<IPermissionRepository>();
        var nonExistentId = Guid.NewGuid();

        var act = () => repo.GetPermissionByIdOrThrowAsync(nonExistentId);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task AddAsync_ShouldPersistPermissionToDatabase()
    {
        var (repo, db) = CreateScopedRepository<IPermissionRepository, IdentityDbContext>();

        var permission = PermissionFactory.Create("articles", "create");
        await repo.AddAsync(permission);
        await db.SaveChangesAsync();

        await using var verifyContext = CreateDbContext<IdentityDbContext>();
        var saved = await verifyContext.Permissions.FindAsync(permission.Id);

        saved.Should().NotBeNull();
        saved!.Resource.Should().Be("articles");
        saved.Action.Should().Be("create");
    }

    [Fact]
    public async Task Delete_ShouldRemovePermissionFromDatabase()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var permission = PermissionFactory.Create("sessions", "delete");
        seedContext.Permissions.Add(permission);
        await seedContext.SaveChangesAsync();

        var (repo, db) = CreateScopedRepository<IPermissionRepository, IdentityDbContext>();
        var toDelete = await db.Permissions.FindAsync(permission.Id);
        toDelete.Should().NotBeNull();

        repo.Delete(toDelete!);
        await db.SaveChangesAsync();

        await using var verifyContext = CreateDbContext<IdentityDbContext>();
        var deleted = await verifyContext.Permissions.FindAsync(permission.Id);

        deleted.Should().BeNull();
    }

    [Fact]
    public async Task ExistsByResourceAndActionAsync_ExistingPair_ShouldReturnTrue()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var permission = PermissionFactory.Create("roles", "update");
        seedContext.Permissions.Add(permission);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IPermissionRepository>();

        var exists = await repo.ExistsByResourceAndActionAsync("roles", "update");

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByResourceAndActionAsync_NonExistentPair_ShouldReturnFalse()
    {
        var repo = Resolve<IPermissionRepository>();

        var exists = await repo.ExistsByResourceAndActionAsync("nonexistent", "action");

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllWithPaginationAsync_FilterByIsActive_ShouldReturnOnlyMatchingResults()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var activePermission = PermissionFactory.Create("reports", "read");
        var inactivePermission = PermissionFactory.CreateInactive();
        seedContext.Permissions.AddRange(activePermission, inactivePermission);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IPermissionRepository>();

        var (activeResults, _) = await repo.GetAllWithPaginationAsync(1, 50, isActive: true);
        var (inactiveResults, _) = await repo.GetAllWithPaginationAsync(1, 50, isActive: false);

        activeResults.Should().OnlyContain(p => p.IsActive);
        inactiveResults.Should().OnlyContain(p => !p.IsActive);
    }

    [Fact]
    public async Task GetAllWithPaginationAsync_FilterBySearchTerm_ShouldReturnMatchingResults()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var permission = PermissionFactory.Create("invoices", "approve");
        seedContext.Permissions.Add(permission);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IPermissionRepository>();

        var (results, totalCount) = await repo.GetAllWithPaginationAsync(1, 50, search: "invoices");

        totalCount.Should().Be(1);
        results.Should().Contain(p => p.Resource == "invoices");
    }
}
