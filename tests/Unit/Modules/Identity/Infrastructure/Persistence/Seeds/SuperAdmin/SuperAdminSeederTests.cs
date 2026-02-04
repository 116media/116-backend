using _116.Identity.Application.Auth.Services;
using _116.Identity.Infrastructure.Persistence;
using _116.Identity.Infrastructure.Persistence.Seeds.SuperAdmin;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Infrastructure.Persistence.Seeds.SuperAdmin;

/// <summary>
/// Unit tests for <see cref="SuperAdminSeeder"/>.
/// </summary>
public class SuperAdminSeederTests
{
    private readonly Mock<ILogger<SuperAdminSeeder>> _seederLoggerMock;
    private readonly Mock<ILogger<SuperAdminRepositoryManager>> _repositoryLoggerMock;
    private readonly Mock<ILogger<SuperAdminSeedingStrategy>> _strategyLoggerMock;
    private readonly Mock<IPasswordService> _passwordServiceMock;

    public SuperAdminSeederTests()
    {
        _seederLoggerMock = new Mock<ILogger<SuperAdminSeeder>>();
        _repositoryLoggerMock = new Mock<ILogger<SuperAdminRepositoryManager>>();
        _strategyLoggerMock = new Mock<ILogger<SuperAdminSeedingStrategy>>();
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
            .EnableSensitiveDataLogging()
            .Options;
    }

    #region SeedAllAsync Tests

    // Note: SuperAdminSeeder uses database transactions via BeginTransactionAsync(), which are not supported by InMemory database.
    // These tests require integration testing with a real PostgreSQL database using Testcontainers or similar infrastructure.
    // All tests in this region are skipped for unit test execution.

    [Fact(Skip = "Requires real database with transaction support - InMemory database doesn't support transactions")]
    public async Task SeedAllAsync_ShouldLogStartMessage()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);

        var seeder = new SuperAdminSeeder(
            context,
            _passwordServiceMock.Object,
            _seederLoggerMock.Object,
            _repositoryLoggerMock.Object,
            _strategyLoggerMock.Object
        );

        // Act
        await seeder.SeedAllAsync();

        // Assert
        _seederLoggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Starting Super Admin seeding")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    [Fact(Skip = "Requires real database with transaction support - InMemory database doesn't support transactions")]
    public async Task SeedAllAsync_WhenSuperAdminDoesNotExist_ShouldExecuteSeeding()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);

        var seeder = new SuperAdminSeeder(
            context,
            _passwordServiceMock.Object,
            _seederLoggerMock.Object,
            _repositoryLoggerMock.Object,
            _strategyLoggerMock.Object
        );

        // Act
        await seeder.SeedAllAsync();

        // Assert
        _seederLoggerMock.Verify(
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

    [Fact(Skip = "Requires real database with transaction support - InMemory database doesn't support transactions")]
    public async Task SeedAllAsync_WhenSuperAdminAlreadyExists_ShouldSkipSeeding()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);

        // Pre-seed SuperAdmin user
        var existingUser = _116.Identity.Domain.Entities.UserEntity.Create(
            Guid.NewGuid(),
            SuperAdminConfiguration.Email,
            "existingadmin",
            "hashedPassword"
        );
        await context.Users.AddAsync(existingUser);
        await context.SaveChangesAsync();

        var seeder = new SuperAdminSeeder(
            context,
            _passwordServiceMock.Object,
            _seederLoggerMock.Object,
            _repositoryLoggerMock.Object,
            _strategyLoggerMock.Object
        );

        // Act
        await seeder.SeedAllAsync();

        // Assert
        _seederLoggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("already exists")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    [Fact(Skip = "Requires real database with transaction support - InMemory database doesn't support transactions")]
    public async Task SeedAllAsync_WhenSuperAdminAlreadyExists_ShouldNotCreateNewEntities()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);

        // Pre-seed SuperAdmin user
        var existingUser = _116.Identity.Domain.Entities.UserEntity.Create(
            Guid.NewGuid(),
            SuperAdminConfiguration.Email,
            "existingadmin",
            "hashedPassword"
        );
        await context.Users.AddAsync(existingUser);
        await context.SaveChangesAsync();

        var seeder = new SuperAdminSeeder(
            context,
            _passwordServiceMock.Object,
            _seederLoggerMock.Object,
            _repositoryLoggerMock.Object,
            _strategyLoggerMock.Object
        );

        // Act
        await seeder.SeedAllAsync();

        // Assert
        int userCount = await context.Users.CountAsync();
        Assert.Equal(1, userCount); // Should still be just the pre-seeded user
    }

    [Fact(Skip = "Requires real database with transaction support - InMemory database doesn't support transactions")]
    public async Task SeedAllAsync_WhenSeedingSucceeds_ShouldCreateSuperAdminUser()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);

        var seeder = new SuperAdminSeeder(
            context,
            _passwordServiceMock.Object,
            _seederLoggerMock.Object,
            _repositoryLoggerMock.Object,
            _strategyLoggerMock.Object
        );

        // Act
        await seeder.SeedAllAsync();

        // Assert
        var superAdminUser = await context.Users.FirstOrDefaultAsync(u => u.Email == SuperAdminConfiguration.Email);
        Assert.NotNull(superAdminUser);
        Assert.Equal(SuperAdminConfiguration.Username, superAdminUser.UserName);
    }

    [Fact(Skip = "Requires real database with transaction support - InMemory database doesn't support transactions")]
    public async Task SeedAllAsync_WhenSeedingSucceeds_ShouldCreateSuperAdminRole()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);

        var seeder = new SuperAdminSeeder(
            context,
            _passwordServiceMock.Object,
            _seederLoggerMock.Object,
            _repositoryLoggerMock.Object,
            _strategyLoggerMock.Object
        );

        // Act
        await seeder.SeedAllAsync();

        // Assert
        var superAdminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == SuperAdminConfiguration.RoleName);
        Assert.NotNull(superAdminRole);
    }

    [Fact(Skip = "Requires real database with transaction support - InMemory database doesn't support transactions")]
    public async Task SeedAllAsync_WhenSeedingSucceeds_ShouldCreateSystemPermission()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);

        var seeder = new SuperAdminSeeder(
            context,
            _passwordServiceMock.Object,
            _seederLoggerMock.Object,
            _repositoryLoggerMock.Object,
            _strategyLoggerMock.Object
        );

        // Act
        await seeder.SeedAllAsync();

        // Assert
        var systemPermission = await context.Permissions.FirstOrDefaultAsync(p =>
            p.Resource == "system" && p.Action == "all"
        );
        Assert.NotNull(systemPermission);
    }

    [Fact(Skip = "Requires real database with transaction support - InMemory database doesn't support transactions")]
    public async Task SeedAllAsync_WhenSeedingSucceeds_ShouldAssociateUserWithRole()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);

        var seeder = new SuperAdminSeeder(
            context,
            _passwordServiceMock.Object,
            _seederLoggerMock.Object,
            _repositoryLoggerMock.Object,
            _strategyLoggerMock.Object
        );

        // Act
        await seeder.SeedAllAsync();

        // Assert
        var userRole = await context.UserRoles.FirstOrDefaultAsync();
        Assert.NotNull(userRole);
    }

    [Fact(Skip = "Requires real database with transaction support - InMemory database doesn't support transactions")]
    public async Task SeedAllAsync_WhenSeedingSucceeds_ShouldAssociateRoleWithPermission()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);

        var seeder = new SuperAdminSeeder(
            context,
            _passwordServiceMock.Object,
            _seederLoggerMock.Object,
            _repositoryLoggerMock.Object,
            _strategyLoggerMock.Object
        );

        // Act
        await seeder.SeedAllAsync();

        // Assert
        var rolePermission = await context.RolePermissions.FirstOrDefaultAsync();
        Assert.NotNull(rolePermission);
    }

    [Fact(Skip = "Requires real database with transaction support - InMemory database doesn't support transactions")]
    public async Task SeedAllAsync_WhenSeedingSucceeds_ShouldLogTransactionCommitted()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);

        var seeder = new SuperAdminSeeder(
            context,
            _passwordServiceMock.Object,
            _seederLoggerMock.Object,
            _repositoryLoggerMock.Object,
            _strategyLoggerMock.Object
        );

        // Act
        await seeder.SeedAllAsync();

        // Assert
        _seederLoggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("transaction committed")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    [Fact(Skip = "Requires real database with transaction support - InMemory database doesn't support transactions")]
    public async Task SeedAllAsync_WhenSeedingSucceeds_ShouldLogCompletionMessage()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);

        var seeder = new SuperAdminSeeder(
            context,
            _passwordServiceMock.Object,
            _seederLoggerMock.Object,
            _repositoryLoggerMock.Object,
            _strategyLoggerMock.Object
        );

        // Act
        await seeder.SeedAllAsync();

        // Assert
        _seederLoggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Super Admin seeding completed successfully")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    [Fact(Skip = "Requires real database with transaction support - InMemory database doesn't support transactions")]
    public async Task SeedAllAsync_WhenSeedingSucceeds_ShouldMarkUserAsVerified()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);

        var seeder = new SuperAdminSeeder(
            context,
            _passwordServiceMock.Object,
            _seederLoggerMock.Object,
            _repositoryLoggerMock.Object,
            _strategyLoggerMock.Object
        );

        // Act
        await seeder.SeedAllAsync();

        // Assert
        var superAdminUser = await context.Users.FirstOrDefaultAsync(u => u.Email == SuperAdminConfiguration.Email);
        Assert.NotNull(superAdminUser);
        Assert.True(superAdminUser.IsVerified);
    }

    [Fact(Skip = "Requires real database with transaction support - InMemory database doesn't support transactions")]
    public async Task SeedAllAsync_WhenSeedingSucceeds_ShouldActivateUser()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);

        var seeder = new SuperAdminSeeder(
            context,
            _passwordServiceMock.Object,
            _seederLoggerMock.Object,
            _repositoryLoggerMock.Object,
            _strategyLoggerMock.Object
        );

        // Act
        await seeder.SeedAllAsync();

        // Assert
        var superAdminUser = await context.Users.FirstOrDefaultAsync(u => u.Email == SuperAdminConfiguration.Email);
        Assert.NotNull(superAdminUser);
        Assert.True(superAdminUser.IsActive);
    }

    [Fact(Skip = "Requires real database with transaction support - InMemory database doesn't support transactions")]
    public async Task SeedAllAsync_WhenSeedingSucceeds_ShouldHashPassword()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);

        var seeder = new SuperAdminSeeder(
            context,
            _passwordServiceMock.Object,
            _seederLoggerMock.Object,
            _repositoryLoggerMock.Object,
            _strategyLoggerMock.Object
        );

        // Act
        await seeder.SeedAllAsync();

        // Assert
        _passwordServiceMock.Verify(x => x.Hash(It.IsAny<string>()), Times.Once);
    }

    #endregion

    #region Error Handling Tests

    [Fact(Skip = "Requires real database with transaction support - InMemory database doesn't support transactions")]
    public async Task SeedAllAsync_WhenExceptionOccurs_ShouldLogError()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);

        // Force an exception by setting password service to throw
        _passwordServiceMock.Setup(x => x.Hash(It.IsAny<string>())).Throws(new Exception("Password hashing failed"));

        var seeder = new SuperAdminSeeder(
            context,
            _passwordServiceMock.Object,
            _seederLoggerMock.Object,
            _repositoryLoggerMock.Object,
            _strategyLoggerMock.Object
        );

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => seeder.SeedAllAsync());

        _seederLoggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to seed Super Admin")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    [Fact(Skip = "Requires real database with transaction support - InMemory database doesn't support transactions")]
    public async Task SeedAllAsync_WhenExceptionOccurs_ShouldRethrowException()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);

        _passwordServiceMock.Setup(x => x.Hash(It.IsAny<string>())).Throws(new Exception("Password hashing failed"));

        var seeder = new SuperAdminSeeder(
            context,
            _passwordServiceMock.Object,
            _seederLoggerMock.Object,
            _repositoryLoggerMock.Object,
            _strategyLoggerMock.Object
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => seeder.SeedAllAsync());
        Assert.Equal("Password hashing failed", exception.Message);
    }

    [Fact(Skip = "Requires real database with transaction support - InMemory database doesn't support transactions")]
    public async Task SeedAllAsync_WhenExceptionOccursDuringSeeding_ShouldLogRollback()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);

        _passwordServiceMock.Setup(x => x.Hash(It.IsAny<string>())).Throws(new Exception("Seeding error"));

        var seeder = new SuperAdminSeeder(
            context,
            _passwordServiceMock.Object,
            _seederLoggerMock.Object,
            _repositoryLoggerMock.Object,
            _strategyLoggerMock.Object
        );

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => seeder.SeedAllAsync());

        _seederLoggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Transaction rolled back")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    #endregion
}
