using _116.Identity.Application.Auth.Services;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Identity.Infrastructure.Persistence.Seeds.SuperAdmin;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Infrastructure.Persistence.Seeds.SuperAdmin;

/// <summary>
/// Unit tests for <see cref="SuperAdminSeedingStrategy"/>.
/// </summary>
public class SuperAdminSeedingStrategyTests
{
    private readonly Mock<ILogger<SuperAdminRepositoryManager>> _repositoryLoggerMock;
    private readonly Mock<ILogger<SuperAdminSeedingStrategy>> _loggerMock;
    private readonly Mock<IPasswordService> _passwordServiceMock;

    public SuperAdminSeedingStrategyTests()
    {
        _repositoryLoggerMock = new Mock<ILogger<SuperAdminRepositoryManager>>();
        _loggerMock = new Mock<ILogger<SuperAdminSeedingStrategy>>();
        _passwordServiceMock = new Mock<IPasswordService>();

        _passwordServiceMock.Setup(x => x.Hash(It.IsAny<string>())).Returns("hashedPassword");

        // Setup default password environment variable
        string? originalPassword = Environment.GetEnvironmentVariable("DEFAULT_USER_PASSWORD");
        if (string.IsNullOrWhiteSpace(originalPassword))
        {
            Environment.SetEnvironmentVariable("DEFAULT_USER_PASSWORD", "TestPassword123!");
        }
    }

    private DbContextOptions<IdentityDbContext> CreateOptions()
    {
        return new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    #region ExecuteSeedingAsync Tests

    [Fact]
    public async Task ExecuteSeedingAsync_ShouldLogStartMessage()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);

        var repositoryManager = new SuperAdminRepositoryManager(context, _repositoryLoggerMock.Object);
        var entityFactory = new SuperAdminEntityFactory(_passwordServiceMock.Object);
        var strategy = new SuperAdminSeedingStrategy(entityFactory, repositoryManager, _loggerMock.Object);

        // Act
        await strategy.ExecuteSeedingAsync();

        // Assert
        _loggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Executing Super Admin seeding strategy")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task ExecuteSeedingAsync_WhenPermissionDoesNotExist_ShouldCreateNewPermission()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);

        var repositoryManager = new SuperAdminRepositoryManager(context, _repositoryLoggerMock.Object);
        var entityFactory = new SuperAdminEntityFactory(_passwordServiceMock.Object);
        var strategy = new SuperAdminSeedingStrategy(entityFactory, repositoryManager, _loggerMock.Object);

        // Act
        await strategy.ExecuteSeedingAsync();
        await repositoryManager.SaveChangesAsync();

        // Assert
        var permission = await context.Permissions.FirstOrDefaultAsync(p =>
            p.Resource == "system" && p.Action == "all"
        );
        Assert.NotNull(permission);
    }

    [Fact]
    public async Task ExecuteSeedingAsync_WhenPermissionExists_ShouldNotCreateDuplicatePermission()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);

        // Pre-seed permission
        var existingPermission = PermissionEntity.Create(Guid.NewGuid(), "system", "all", "Existing");
        await context.Permissions.AddAsync(existingPermission);
        await context.SaveChangesAsync();

        var repositoryManager = new SuperAdminRepositoryManager(context, _repositoryLoggerMock.Object);
        var entityFactory = new SuperAdminEntityFactory(_passwordServiceMock.Object);
        var strategy = new SuperAdminSeedingStrategy(entityFactory, repositoryManager, _loggerMock.Object);

        // Act
        await strategy.ExecuteSeedingAsync();
        await repositoryManager.SaveChangesAsync();

        // Assert
        int permissionCount = await context.Permissions.CountAsync(p => p.Resource == "system" && p.Action == "all");
        Assert.Equal(1, permissionCount); // Should still be just one
    }

    [Fact]
    public async Task ExecuteSeedingAsync_WhenRoleDoesNotExist_ShouldCreateNewRole()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);

        var repositoryManager = new SuperAdminRepositoryManager(context, _repositoryLoggerMock.Object);
        var entityFactory = new SuperAdminEntityFactory(_passwordServiceMock.Object);
        var strategy = new SuperAdminSeedingStrategy(entityFactory, repositoryManager, _loggerMock.Object);

        // Act
        await strategy.ExecuteSeedingAsync();
        await repositoryManager.SaveChangesAsync();

        // Assert
        var role = await context.Roles.FirstOrDefaultAsync(r => r.Name == "SuperAdmin");
        Assert.NotNull(role);
    }

    [Fact]
    public async Task ExecuteSeedingAsync_WhenRoleExists_ShouldNotCreateDuplicateRole()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);

        // Pre-seed role
        var existingRole = RoleEntity.Create(Guid.NewGuid(), "SuperAdmin", "Existing");
        await context.Roles.AddAsync(existingRole);
        await context.SaveChangesAsync();

        var repositoryManager = new SuperAdminRepositoryManager(context, _repositoryLoggerMock.Object);
        var entityFactory = new SuperAdminEntityFactory(_passwordServiceMock.Object);
        var strategy = new SuperAdminSeedingStrategy(entityFactory, repositoryManager, _loggerMock.Object);

        // Act
        await strategy.ExecuteSeedingAsync();
        await repositoryManager.SaveChangesAsync();

        // Assert
        int roleCount = await context.Roles.CountAsync(r => r.Name == "SuperAdmin");
        Assert.Equal(1, roleCount); // Should still be just one
    }

    [Fact]
    public async Task ExecuteSeedingAsync_WhenRolePermissionDoesNotExist_ShouldCreateAssociation()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);

        var repositoryManager = new SuperAdminRepositoryManager(context, _repositoryLoggerMock.Object);
        var entityFactory = new SuperAdminEntityFactory(_passwordServiceMock.Object);
        var strategy = new SuperAdminSeedingStrategy(entityFactory, repositoryManager, _loggerMock.Object);

        // Act
        await strategy.ExecuteSeedingAsync();
        await repositoryManager.SaveChangesAsync();

        // Assert
        var rolePermission = await context.RolePermissions.FirstOrDefaultAsync();
        Assert.NotNull(rolePermission);
    }

    [Fact]
    public async Task ExecuteSeedingAsync_WhenRolePermissionExists_ShouldNotCreateDuplicateAssociation()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);

        // Pre-seed everything
        var permission = PermissionEntity.Create(Guid.NewGuid(), "system", "all", "Test");
        var role = RoleEntity.Create(Guid.NewGuid(), "SuperAdmin", "Test");
        var rolePermission = RolePermissionEntity.Create(Guid.NewGuid(), role.Id, permission.Id);
        await context.Permissions.AddAsync(permission);
        await context.Roles.AddAsync(role);
        await context.RolePermissions.AddAsync(rolePermission);
        await context.SaveChangesAsync();

        var repositoryManager = new SuperAdminRepositoryManager(context, _repositoryLoggerMock.Object);
        var entityFactory = new SuperAdminEntityFactory(_passwordServiceMock.Object);
        var strategy = new SuperAdminSeedingStrategy(entityFactory, repositoryManager, _loggerMock.Object);

        // Act
        await strategy.ExecuteSeedingAsync();
        await repositoryManager.SaveChangesAsync();

        // Assert
        int rolePermissionCount = await context.RolePermissions.CountAsync();
        Assert.Equal(1, rolePermissionCount); // Should still be just one
    }

    [Fact]
    public async Task ExecuteSeedingAsync_ShouldCreateSuperAdminUser()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);

        var repositoryManager = new SuperAdminRepositoryManager(context, _repositoryLoggerMock.Object);
        var entityFactory = new SuperAdminEntityFactory(_passwordServiceMock.Object);
        var strategy = new SuperAdminSeedingStrategy(entityFactory, repositoryManager, _loggerMock.Object);

        // Act
        await strategy.ExecuteSeedingAsync();
        await repositoryManager.SaveChangesAsync();

        // Assert
        var user = await context.Users.FirstOrDefaultAsync();
        Assert.NotNull(user);
        Assert.Equal(SuperAdminConfiguration.Username, user.UserName);
    }

    [Fact]
    public async Task ExecuteSeedingAsync_WhenUserRoleDoesNotExist_ShouldCreateAssociation()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);

        var repositoryManager = new SuperAdminRepositoryManager(context, _repositoryLoggerMock.Object);
        var entityFactory = new SuperAdminEntityFactory(_passwordServiceMock.Object);
        var strategy = new SuperAdminSeedingStrategy(entityFactory, repositoryManager, _loggerMock.Object);

        // Act
        await strategy.ExecuteSeedingAsync();
        await repositoryManager.SaveChangesAsync();

        // Assert
        var userRole = await context.UserRoles.FirstOrDefaultAsync();
        Assert.NotNull(userRole);
    }

    [Fact]
    public async Task ExecuteSeedingAsync_ShouldLogCompletionMessage()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);

        var repositoryManager = new SuperAdminRepositoryManager(context, _repositoryLoggerMock.Object);
        var entityFactory = new SuperAdminEntityFactory(_passwordServiceMock.Object);
        var strategy = new SuperAdminSeedingStrategy(entityFactory, repositoryManager, _loggerMock.Object);

        // Act
        await strategy.ExecuteSeedingAsync();

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
            Times.Once
        );
    }

    [Fact]
    public async Task ExecuteSeedingAsync_ShouldLogSystemPermissionReady()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);

        var repositoryManager = new SuperAdminRepositoryManager(context, _repositoryLoggerMock.Object);
        var entityFactory = new SuperAdminEntityFactory(_passwordServiceMock.Object);
        var strategy = new SuperAdminSeedingStrategy(entityFactory, repositoryManager, _loggerMock.Object);

        // Act
        await strategy.ExecuteSeedingAsync();

        // Assert
        _loggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("System permission ready")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task ExecuteSeedingAsync_ShouldLogSuperAdminRoleReady()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);

        var repositoryManager = new SuperAdminRepositoryManager(context, _repositoryLoggerMock.Object);
        var entityFactory = new SuperAdminEntityFactory(_passwordServiceMock.Object);
        var strategy = new SuperAdminSeedingStrategy(entityFactory, repositoryManager, _loggerMock.Object);

        // Act
        await strategy.ExecuteSeedingAsync();

        // Assert
        _loggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Super Admin role ready")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task ExecuteSeedingAsync_ShouldLogUserCreation()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);

        var repositoryManager = new SuperAdminRepositoryManager(context, _repositoryLoggerMock.Object);
        var entityFactory = new SuperAdminEntityFactory(_passwordServiceMock.Object);
        var strategy = new SuperAdminSeedingStrategy(entityFactory, repositoryManager, _loggerMock.Object);

        // Act
        await strategy.ExecuteSeedingAsync();

        // Assert
        _loggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Created Super Admin user")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    #endregion
}
