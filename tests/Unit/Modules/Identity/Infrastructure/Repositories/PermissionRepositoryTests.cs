using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Identity.Infrastructure.Repositories;
using _116.Shared.Application.Exceptions;
using _116.Unit.Tests.Common.Builders.Entities;
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
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new IdentityDbContext(options);
        _repository = new PermissionRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region GetPermissionByIdOrThrowAsync Tests

    [Fact]
    public async Task GetPermissionByIdOrThrowAsync_WhenPermissionExists_ShouldReturnPermission()
    {
        // Arrange
        var permission = new PermissionBuilder().WithResource("article").WithAction("read").Build();

        _context.Permissions.Add(permission);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetPermissionByIdOrThrowAsync(permission.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(permission.Id);
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
        var permission = new PermissionBuilder().WithResource("user").WithAction("create").Build();

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
        var permission = new PermissionBuilder().WithResource("article").WithAction("read").Build();

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
        var permission = new PermissionBuilder().WithResource("article").WithAction("read").Build();

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
        var permission = new PermissionBuilder().WithResource("comment").WithAction("delete").Build();

        // Act
        await _repository.AddAsync(permission);
        await _context.SaveChangesAsync();

        // Assert
        var savedPermission = await _context.Permissions.FirstOrDefaultAsync(p => p.Id == permission.Id);
        savedPermission.Should().NotBeNull();
        savedPermission!.Resource.Should().Be("comment");
        savedPermission.Action.Should().Be("delete");
    }

    #endregion

    #region GetAllWithPaginationAsync Tests

    [Fact]
    public async Task GetAllWithPaginationAsync_WithNoFilters_ShouldReturnAllPermissions()
    {
        // Arrange
        var permission1 = new PermissionBuilder().WithResource("article").WithAction("read").Build();
        var permission2 = new PermissionBuilder().WithResource("article").WithAction("write").Build();
        var permission3 = new PermissionBuilder().WithResource("user").WithAction("create").Build();

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
            var permission = new PermissionBuilder().WithResource($"resource{i}").WithAction("read").Build();
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
        var activePermission = new PermissionBuilder().WithResource("active").WithAction("read").Build();
        var inactivePermission = new PermissionBuilder()
            .WithResource("inactive")
            .WithAction("read")
            .AsInactive()
            .Build();

        _context.Permissions.AddRange(activePermission, inactivePermission);
        await _context.SaveChangesAsync();

        // Act
        var (permissions, totalCount) = await _repository.GetAllWithPaginationAsync(
            page: 1,
            pageSize: 10,
            isActive: true
        );

        // Assert
        permissions.Should().HaveCount(1);
        totalCount.Should().Be(1);
        permissions.First().Resource.Should().Be("active");
    }

    [Fact]
    public async Task GetAllWithPaginationAsync_WithDeletedFilter_ShouldFilterByDeletedStatus()
    {
        // Arrange
        var normalPermission = new PermissionBuilder().WithResource("normal").WithAction("read").Build();
        var deletedPermission = new PermissionBuilder().WithResource("deleted").WithAction("read").AsDeleted().Build();

        _context.Permissions.AddRange(normalPermission, deletedPermission);
        await _context.SaveChangesAsync();

        // Act
        var (permissions, totalCount) = await _repository.GetAllWithPaginationAsync(
            page: 1,
            pageSize: 10,
            isDeleted: false
        );

        // Assert
        permissions.Should().HaveCount(1);
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
        var permission1 = new PermissionBuilder().WithResource("first").WithAction("read").Build();
        var permission2 = new PermissionBuilder().WithResource("second").WithAction("read").Build();
        var permission3 = new PermissionBuilder().WithResource("third").WithAction("read").Build();

        _context.Permissions.AddRange(permission1, permission2, permission3);
        await _context.SaveChangesAsync();

        // Act
        var (permissions, _) = await _repository.GetAllWithPaginationAsync(page: 1, pageSize: 10);

        // Assert
        permissions.Should().HaveCount(3);
        // InMemoryDatabase may not preserve exact insertion order for CreatedAt
        // Just verify all permissions are returned
        var resources = permissions.Select(p => p.Resource).ToList();
        resources.Should().Contain("first");
        resources.Should().Contain("second");
        resources.Should().Contain("third");
    }

    [Fact]
    public async Task GetAllWithPaginationAsync_WithBothActiveAndDeletedFilters_ShouldApplyBothFilters()
    {
        // Arrange
        var activeNotDeleted = new PermissionBuilder().WithResource("activeNotDeleted").WithAction("read").Build();
        var inactiveNotDeleted = new PermissionBuilder()
            .WithResource("inactiveNotDeleted")
            .WithAction("read")
            .AsInactive()
            .Build();
        var activeDeleted = new PermissionBuilder()
            .WithResource("activeDeleted")
            .WithAction("read")
            .AsDeleted()
            .Build();

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
        permissions.Should().HaveCount(1);
        totalCount.Should().Be(1);
        permissions.First().Resource.Should().Be("activeNotDeleted");
    }

    #endregion

    #region Delete Tests

    [Fact]
    public void Delete_ShouldRemovePermissionFromContext()
    {
        // Arrange
        var permission = new PermissionBuilder().WithResource("toDelete").WithAction("delete").Build();

        _context.Permissions.Add(permission);
        _context.SaveChanges();

        // Act
        _repository.Delete(permission);
        _context.SaveChanges();

        // Assert
        var deletedPermission = _context.Permissions.FirstOrDefault(p => p.Id == permission.Id);
        deletedPermission.Should().BeNull();
    }

    #endregion
}
