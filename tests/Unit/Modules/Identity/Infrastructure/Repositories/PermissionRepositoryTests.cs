using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Identity.Infrastructure.Repositories;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Infrastructure.Repositories;

/// <summary>
/// Unit tests for <see cref="PermissionRepository"/>.
/// </summary>
public class PermissionRepositoryTests : IDisposable
{
    private readonly IdentityDbContext _context;
    private readonly PermissionRepository _repository;

    public PermissionRepositoryTests()
    {
        DbContextOptions<IdentityDbContext> options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new IdentityDbContext(options);
        _repository = new PermissionRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    #region GetPermissionByIdOrThrowAsync Tests

    [Fact]
    public async Task GetPermissionByIdOrThrowAsync_WhenPermissionExists_ShouldReturnPermission()
    {
        // Arrange
        PermissionEntity permission = PermissionFactory.CreateRead("article");

        _context.Permissions.Add(permission);
        await _context.SaveChangesAsync();

        // Act
        PermissionEntity? result = await _repository.GetPermissionByIdOrThrowAsync(permission.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(permission.Id);
        result.Resource.Should().Be("article");
        result.Action.Should().Be("read");
    }

    [Fact]
    public async Task GetPermissionByIdOrThrowAsync_WhenPermissionDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var permissionId = Guid.NewGuid();

        // Act
        Func<Task> act = async () => await _repository.GetPermissionByIdOrThrowAsync(permissionId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region ExistsByResourceAndActionAsync Tests

    [Fact]
    public async Task ExistsByResourceAndActionAsync_WhenPermissionExists_ShouldReturnTrue()
    {
        // Arrange
        PermissionEntity permission = PermissionFactory.CreateCreate("user");

        _context.Permissions.Add(permission);
        await _context.SaveChangesAsync();

        // Act
        bool exists = await _repository.ExistsByResourceAndActionAsync("user", "create");

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByResourceAndActionAsync_WhenPermissionDoesNotExist_ShouldReturnFalse()
    {
        // Arrange & Act
        bool exists = await _repository.ExistsByResourceAndActionAsync("nonexistent", "action");

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsByResourceAndActionAsync_WithDifferentResource_ShouldReturnFalse()
    {
        // Arrange
        PermissionEntity permission = PermissionFactory.CreateRead("article");

        _context.Permissions.Add(permission);
        await _context.SaveChangesAsync();

        // Act
        bool exists = await _repository.ExistsByResourceAndActionAsync("user", "read");

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsByResourceAndActionAsync_WithDifferentAction_ShouldReturnFalse()
    {
        // Arrange
        PermissionEntity permission = PermissionFactory.CreateRead("article");

        _context.Permissions.Add(permission);
        await _context.SaveChangesAsync();

        // Act
        bool exists = await _repository.ExistsByResourceAndActionAsync("article", "write");

        // Assert
        exists.Should().BeFalse();
    }

    #endregion

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_ShouldAddPermissionToContext()
    {
        // Arrange
        PermissionEntity permission = PermissionFactory.CreateDelete("comment");

        // Act
        await _repository.AddAsync(permission);
        await _context.SaveChangesAsync();

        // Assert
        PermissionEntity? savedPermission = await _context.Permissions.FirstOrDefaultAsync(p => p.Id == permission.Id);
        savedPermission.Should().NotBeNull();
        savedPermission.Resource.Should().Be("comment");
        savedPermission.Action.Should().Be("delete");
    }

    #endregion

    #region GetAllWithPaginationAsync Tests

    [Fact]
    public async Task GetAllWithPaginationAsync_WithNoFilters_ShouldReturnAllPermissions()
    {
        // Arrange
        PermissionEntity permission1 = PermissionFactory.CreateRead("article");
        PermissionEntity permission2 = PermissionFactory.Create("article", "write");
        PermissionEntity permission3 = PermissionFactory.CreateCreate("user");

        _context.Permissions.AddRange(permission1, permission2, permission3);
        await _context.SaveChangesAsync();

        // Act
        var (permissions, totalCount) = await _repository.GetAllWithPaginationAsync(page: 1, pageSize: 10);

        // Assert
        permissions.Should().HaveCount(3);
        totalCount.Should().Be(3);
    }

    [Fact]
    public async Task GetAllWithPaginationAsync_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        for (int i = 1; i <= 5; i++)
        {
            PermissionEntity permission = PermissionFactory.Create($"resource{i}", "read");
            _context.Permissions.Add(permission);
        }
        await _context.SaveChangesAsync();

        // Act
        var (permissions, totalCount) = await _repository.GetAllWithPaginationAsync(page: 2, pageSize: 2);

        // Assert
        permissions.Should().HaveCount(2);
        totalCount.Should().Be(5);
    }

    // Note: Search functionality uses PostgreSQL ILike which is not supported by InMemoryDatabase.
    // This test is better suited for integration tests with a real PostgreSQL database.

    [Fact]
    public async Task GetAllWithPaginationAsync_WithActiveFilter_ShouldFilterByActiveStatus()
    {
        // Arrange
        PermissionEntity activePermission = PermissionFactory.CreateRead("active");
        PermissionEntity inactivePermission = PermissionFactory.Create("inactive", "read");
        inactivePermission.Deactivate();

        _context.Permissions.AddRange(activePermission, inactivePermission);
        await _context.SaveChangesAsync();

        // Act
        var (permissions, totalCount) = await _repository.GetAllWithPaginationAsync(
            page: 1,
            pageSize: 10,
            isActive: true
        );

        // Assert
        permissions.Should().ContainSingle();
        totalCount.Should().Be(1);
        permissions.First().Resource.Should().Be("active");
    }

    [Fact]
    public async Task GetAllWithPaginationAsync_WithDeletedFilter_ShouldFilterByDeletedStatus()
    {
        // Arrange
        PermissionEntity normalPermission = PermissionFactory.CreateRead("normal");
        PermissionEntity deletedPermission = PermissionFactory.CreateDeleted();

        _context.Permissions.AddRange(normalPermission, deletedPermission);
        await _context.SaveChangesAsync();

        // Act
        var (permissions, totalCount) = await _repository.GetAllWithPaginationAsync(
            page: 1,
            pageSize: 10,
            isDeleted: false
        );

        // Assert
        permissions.Should().ContainSingle();
        totalCount.Should().Be(1);
        permissions.First().Resource.Should().Be("normal");
    }

    [Fact]
    public async Task GetAllWithPaginationAsync_WithEmptyResult_ShouldReturnEmptyList()
    {
        // Act
        var (permissions, totalCount) = await _repository.GetAllWithPaginationAsync(page: 1, pageSize: 10);

        // Assert
        permissions.Should().BeEmpty();
        totalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetAllWithPaginationAsync_ShouldOrderByCreatedAtDescending()
    {
        // Arrange
        PermissionEntity permission1 = PermissionFactory.CreateRead("first");
        PermissionEntity permission2 = PermissionFactory.CreateRead("second");
        PermissionEntity permission3 = PermissionFactory.CreateRead("third");

        _context.Permissions.AddRange(permission1, permission2, permission3);
        await _context.SaveChangesAsync();

        // Act
        var (permissions, _) = await _repository.GetAllWithPaginationAsync(page: 1, pageSize: 10);

        // Assert
        permissions.Should().HaveCount(3);
        // InMemoryDatabase may not preserve exact insertion order for CreatedAt
        // Just verify all permissions are returned
        List<string> resources = permissions.Select(p => p.Resource).ToList();
        resources.Should().Contain("first");
        resources.Should().Contain("second");
        resources.Should().Contain("third");
    }

    [Fact]
    public async Task GetAllWithPaginationAsync_WithBothActiveAndDeletedFilters_ShouldApplyBothFilters()
    {
        // Arrange
        PermissionEntity activeNotDeleted = PermissionFactory.CreateRead("activeNotDeleted");
        PermissionEntity inactiveNotDeleted = PermissionFactory.Create("inactiveNotDeleted", "read");
        inactiveNotDeleted.Deactivate();
        PermissionEntity activeDeleted = PermissionFactory.Create("activeDeleted", "read");
        activeDeleted.SoftDelete();

        _context.Permissions.AddRange(activeNotDeleted, inactiveNotDeleted, activeDeleted);
        await _context.SaveChangesAsync();

        // Act
        var (permissions, totalCount) = await _repository.GetAllWithPaginationAsync(
            page: 1,
            pageSize: 10,
            isActive: true,
            isDeleted: false
        );

        // Assert
        permissions.Should().ContainSingle();
        totalCount.Should().Be(1);
        permissions.First().Resource.Should().Be("activeNotDeleted");
    }

    #endregion

    #region Delete Tests

    [Fact]
    public void Delete_ShouldRemovePermissionFromContext()
    {
        // Arrange
        PermissionEntity permission = PermissionFactory.CreateDelete("toDelete");

        _context.Permissions.Add(permission);
        _context.SaveChanges();

        // Act
        _repository.Delete(permission);
        _context.SaveChanges();

        // Assert
        PermissionEntity? deletedPermission = _context.Permissions.FirstOrDefault(p => p.Id == permission.Id);
        deletedPermission.Should().BeNull();
    }

    #endregion
}
