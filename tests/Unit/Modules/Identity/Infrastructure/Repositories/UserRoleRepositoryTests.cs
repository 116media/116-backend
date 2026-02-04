using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Identity.Infrastructure.Repositories;
using _116.Unit.Tests.Common.Builders.Entities;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Infrastructure.Repositories;

/// <summary>
/// Unit tests for <see cref="UserRoleRepository"/>.
/// </summary>
public class UserRoleRepositoryTests : IDisposable
{
    private readonly IdentityDbContext _context;
    private readonly UserRoleRepository _repository;

    public UserRoleRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new IdentityDbContext(options);
        _repository = new UserRoleRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region ExistsByUserAndRoleAsync Tests

    [Fact]
    public async Task ExistsByUserAndRoleAsync_WhenUserRoleExists_ShouldReturnTrue()
    {
        // Arrange
        var role = new RoleBuilder().Build();
        var user = new UserBuilder().Build();
        var userRole = UserRoleEntity.Create(Guid.NewGuid(), user.Id, role.Id);

        _context.Roles.Add(role);
        _context.Users.Add(user);
        _context.UserRoles.Add(userRole);
        await _context.SaveChangesAsync();

        // Act
        bool exists = await _repository.ExistsByUserAndRoleAsync(user.Id, role.Id);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByUserAndRoleAsync_WhenUserRoleDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        // Act
        bool exists = await _repository.ExistsByUserAndRoleAsync(userId, roleId);

        // Assert
        exists.Should().BeFalse();
    }

    #endregion

    #region GetByUserAndRoleAsync Tests

    [Fact]
    public async Task GetByUserAndRoleAsync_WhenUserRoleExists_ShouldReturnUserRole()
    {
        // Arrange
        var role = new RoleBuilder().Build();
        var user = new UserBuilder().Build();
        var userRole = UserRoleEntity.Create(Guid.NewGuid(), user.Id, role.Id);

        _context.Roles.Add(role);
        _context.Users.Add(user);
        _context.UserRoles.Add(userRole);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByUserAndRoleAsync(user.Id, role.Id);

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be(user.Id);
        result.RoleId.Should().Be(role.Id);
    }

    [Fact]
    public async Task GetByUserAndRoleAsync_WhenUserRoleDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByUserAndRoleAsync(userId, roleId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetUserRolesWithRoleAsync Tests

    [Fact]
    public async Task GetUserRolesWithRoleAsync_WhenUserHasRoles_ShouldReturnUserRolesWithRoleData()
    {
        // Arrange
        var user = new UserBuilder().Build();
        var role1 = new RoleBuilder().WithName("Admin").Build();
        var role2 = new RoleBuilder().WithName("Visitor").Build();
        var userRole1 = UserRoleEntity.Create(Guid.NewGuid(), user.Id, role1.Id);
        var userRole2 = UserRoleEntity.Create(Guid.NewGuid(), user.Id, role2.Id);

        _context.Users.Add(user);
        _context.Roles.AddRange(role1, role2);
        _context.UserRoles.AddRange(userRole1, userRole2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetUserRolesWithRoleAsync(user.Id);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(ur => ur.Role.Should().NotBeNull());
        result.Should().Contain(ur => ur.Role.Name == "Admin");
        result.Should().Contain(ur => ur.Role.Name == "Visitor");
    }

    [Fact]
    public async Task GetUserRolesWithRoleAsync_WhenUserHasNoRoles_ShouldReturnEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var result = await _repository.GetUserRolesWithRoleAsync(userId);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_ShouldAddUserRoleToContext()
    {
        // Arrange
        var role = new RoleBuilder().Build();
        var user = new UserBuilder().Build();
        var userRole = UserRoleEntity.Create(Guid.NewGuid(), user.Id, role.Id);

        _context.Roles.Add(role);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        await _repository.AddAsync(userRole);
        await _context.SaveChangesAsync();

        // Assert
        var savedUserRole = await _context.UserRoles.FirstOrDefaultAsync(ur =>
            ur.UserId == user.Id && ur.RoleId == role.Id
        );
        savedUserRole.Should().NotBeNull();
        savedUserRole!.UserId.Should().Be(user.Id);
        savedUserRole.RoleId.Should().Be(role.Id);
    }

    #endregion

    #region Delete Tests

    [Fact]
    public void Delete_ShouldRemoveUserRoleFromContext()
    {
        // Arrange
        var role = new RoleBuilder().Build();
        var user = new UserBuilder().Build();
        var userRole = UserRoleEntity.Create(Guid.NewGuid(), user.Id, role.Id);

        _context.Roles.Add(role);
        _context.Users.Add(user);
        _context.UserRoles.Add(userRole);
        _context.SaveChanges();

        // Act
        _repository.Delete(userRole);
        _context.SaveChanges();

        // Assert
        var deletedUserRole = _context.UserRoles.FirstOrDefault(ur => ur.UserId == user.Id && ur.RoleId == role.Id);
        deletedUserRole.Should().BeNull();
    }

    #endregion
}
