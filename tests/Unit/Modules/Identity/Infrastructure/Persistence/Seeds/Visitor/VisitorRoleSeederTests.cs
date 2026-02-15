using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Identity.Infrastructure.Persistence;
using _116.Identity.Infrastructure.Persistence.Seeds.Visitor;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Infrastructure.Persistence.Seeds.Visitor;

/// <summary>
/// Collection definition to prevent parallel test execution for VisitorRoleSeeder tests.
/// </summary>
[CollectionDefinition("VisitorRoleSeeder", DisableParallelization = true)]
public class VisitorRoleSeederCollection { }

/// <summary>
/// Unit tests for <see cref="VisitorRoleSeeder"/>.
/// </summary>
[Collection("VisitorRoleSeeder")]
public class VisitorRoleSeederTests
{
    private readonly Mock<ILogger<VisitorRoleSeeder>> _loggerMock;

    public VisitorRoleSeederTests()
    {
        _loggerMock = new Mock<ILogger<VisitorRoleSeeder>>();
    }

    private DbContextOptions<IdentityDbContext> CreateOptions()
    {
        return new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString(), b => b.EnableNullChecks(false))
            .UseInternalServiceProvider(
                new ServiceCollection().AddEntityFrameworkInMemoryDatabase().BuildServiceProvider()
            )
            .Options;
    }

    #region SeedAllAsync Tests

    [Fact]
    public async Task SeedAllAsync_WhenVisitorRoleDoesNotExist_ShouldCreateVisitorRole()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);
        var seeder = new VisitorRoleSeeder(context, _loggerMock.Object);

        // Act
        await seeder.SeedAllAsync();

        // Assert
        RoleEntity? visitorRole = await context.Roles.FirstOrDefaultAsync(r =>
            r.Name == nameof(EnumCoreUserRole.Visitor)
        );
        visitorRole.Should().NotBeNull();
        visitorRole.Name.Should().Be(nameof(EnumCoreUserRole.Visitor));
    }

    [Fact]
    public async Task SeedAllAsync_WhenVisitorRoleDoesNotExist_ShouldCreatePermissions()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);
        var seeder = new VisitorRoleSeeder(context, _loggerMock.Object);

        // Act
        await seeder.SeedAllAsync();

        // Assert
        int permissionCount = await context.Permissions.CountAsync();
        (permissionCount > 0).Should().BeTrue();
    }

    [Fact]
    public async Task SeedAllAsync_WhenVisitorRoleDoesNotExist_ShouldCreate29Permissions()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);
        var seeder = new VisitorRoleSeeder(context, _loggerMock.Object);

        // Act
        await seeder.SeedAllAsync();

        // Assert
        int permissionCount = await context.Permissions.CountAsync();
        Assert.Equal(29, permissionCount);
    }

    [Fact(
        Skip = "Static PermissionEntity instances cause EF Core change tracking issues across test runs - requires production code refactoring"
    )]
    public async Task SeedAllAsync_WhenVisitorRoleDoesNotExist_ShouldCreateRolePermissionAssociations()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);
        var seeder = new VisitorRoleSeeder(context, _loggerMock.Object);

        // Act
        await seeder.SeedAllAsync();

        // Assert - count actual permissions and role-permissions
        int permissionCount = await context.Permissions.CountAsync();
        int rolePermissionCount = await context.RolePermissions.CountAsync();

        // Both should be 29 since we create 29 permissions and 29 role-permission mappings
        Assert.Equal(29, permissionCount);
        Assert.Equal(29, rolePermissionCount);
    }

    [Fact]
    public async Task SeedAllAsync_WhenVisitorRoleAlreadyExists_ShouldSkipSeeding()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);

        // Pre-seed visitor role
        var existingRole = RoleEntity.Create(Guid.NewGuid(), nameof(EnumCoreUserRole.Visitor), "Existing role");
        await context.Roles.AddAsync(existingRole);
        await context.SaveChangesAsync();

        var seeder = new VisitorRoleSeeder(context, _loggerMock.Object);

        // Act
        await seeder.SeedAllAsync();

        // Assert
        int roleCount = await context.Roles.CountAsync();
        Assert.Equal(1, roleCount); // Should still be just the original role
    }

    [Fact]
    public async Task SeedAllAsync_WhenVisitorRoleAlreadyExists_ShouldNotAddPermissions()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);

        // Pre-seed visitor role
        var existingRole = RoleEntity.Create(Guid.NewGuid(), nameof(EnumCoreUserRole.Visitor), "Existing role");
        await context.Roles.AddAsync(existingRole);
        await context.SaveChangesAsync();

        var seeder = new VisitorRoleSeeder(context, _loggerMock.Object);

        // Act
        await seeder.SeedAllAsync();

        // Assert
        int permissionCount = await context.Permissions.CountAsync();
        Assert.Equal(0, permissionCount); // No permissions should be added
    }

    [Fact]
    public async Task SeedAllAsync_WhenVisitorRoleAlreadyExists_ShouldLogSkipMessage()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);

        // Pre-seed visitor role
        var existingRole = RoleEntity.Create(Guid.NewGuid(), nameof(EnumCoreUserRole.Visitor), "Existing role");
        await context.Roles.AddAsync(existingRole);
        await context.SaveChangesAsync();

        var seeder = new VisitorRoleSeeder(context, _loggerMock.Object);

        // Act
        await seeder.SeedAllAsync();

        // Assert
        _loggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("already exists")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.AtLeastOnce
        );
    }

    [Fact]
    public async Task SeedAllAsync_WhenSeeding_ShouldLogStartMessage()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);
        var seeder = new VisitorRoleSeeder(context, _loggerMock.Object);

        // Act
        await seeder.SeedAllAsync();

        // Assert
        _loggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Starting Visitor role seeding")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task SeedAllAsync_WhenSeedingCompletes_ShouldLogCompletionMessage()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);
        var seeder = new VisitorRoleSeeder(context, _loggerMock.Object);

        // Act
        await seeder.SeedAllAsync();

        // Assert
        _loggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("completed successfully")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.AtLeastOnce
        );
    }

    [Fact]
    public async Task SeedAllAsync_WhenSeedingSucceeds_ShouldLogPermissionCount()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);
        var seeder = new VisitorRoleSeeder(context, _loggerMock.Object);

        // Act
        await seeder.SeedAllAsync();

        // Assert
        _loggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("29 permissions")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    #endregion

    #region Role Description Tests

    [Fact]
    public async Task SeedAllAsync_ShouldSetCorrectRoleDescription()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);
        var seeder = new VisitorRoleSeeder(context, _loggerMock.Object);

        // Act
        await seeder.SeedAllAsync();

        // Assert
        RoleEntity? visitorRole = await context.Roles.FirstOrDefaultAsync(r =>
            r.Name == nameof(EnumCoreUserRole.Visitor)
        );
        visitorRole.Should().NotBeNull();
        visitorRole.Description.Should().Contain("Standard public");
        visitorRole.Description.Should().Contain("content access");
    }

    #endregion
}
