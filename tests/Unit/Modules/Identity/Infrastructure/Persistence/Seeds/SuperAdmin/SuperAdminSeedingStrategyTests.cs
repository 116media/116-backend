using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Shared.Errors;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Identity.Infrastructure.Persistence.Seeds.SuperAdmin;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Infrastructure.Persistence.Seeds.SuperAdmin;

/// <summary>
/// Unit tests for <see cref="SuperAdminSeedingStrategy"/>.
/// </summary>
[Collection("EnvironmentVariable")]
public class SuperAdminSeedingStrategyTests : IDisposable
{
    private const string DefaultPasswordVariable = "DEFAULT_USER_PASSWORD";

    private readonly Mock<ILogger<SuperAdminRepositoryManager>> _repositoryLoggerMock;
    private readonly Mock<ILogger<SuperAdminSeedingStrategy>> _loggerMock;
    private readonly Mock<IPasswordService> _passwordServiceMock;
    private readonly UserErrors _userErrors = TestErrorsFactory.CreateUserErrors();
    private readonly string? _originalPassword;

    public SuperAdminSeedingStrategyTests()
    {
        _repositoryLoggerMock = new Mock<ILogger<SuperAdminRepositoryManager>>();
        _loggerMock = new Mock<ILogger<SuperAdminSeedingStrategy>>();
        _passwordServiceMock = new Mock<IPasswordService>();

        _passwordServiceMock.Setup(x => x.Hash(It.IsAny<string>())).Returns("hashedPassword");

        // Ensure password env variable is always set for tests
        _originalPassword = Environment.GetEnvironmentVariable(DefaultPasswordVariable);
        Environment.SetEnvironmentVariable(DefaultPasswordVariable, "TestPassword123!");
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Environment.SetEnvironmentVariable(DefaultPasswordVariable, _originalPassword);
        GC.SuppressFinalize(this);
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
        var entityFactory = new SuperAdminEntityFactory(_passwordServiceMock.Object, _userErrors);
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
        var entityFactory = new SuperAdminEntityFactory(_passwordServiceMock.Object, _userErrors);
        var strategy = new SuperAdminSeedingStrategy(entityFactory, repositoryManager, _loggerMock.Object);

        // Act
        await strategy.ExecuteSeedingAsync();
        await repositoryManager.SaveChangesAsync();

        // Assert
        PermissionEntity? permission = await context.Permissions.FirstOrDefaultAsync(p =>
            p.Resource == "system" && p.Action == "all"
        );
        permission.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteSeedingAsync_WhenPermissionExists_ShouldNotCreateDuplicatePermission()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);

        // Pre-seed permission
        var existingPermission = PermissionEntity.Create(Guid.NewGuid(), "system", "all", "Existing", _userErrors);
        await context.Permissions.AddAsync(existingPermission);
        await context.SaveChangesAsync();

        var repositoryManager = new SuperAdminRepositoryManager(context, _repositoryLoggerMock.Object);
        var entityFactory = new SuperAdminEntityFactory(_passwordServiceMock.Object, _userErrors);
        var strategy = new SuperAdminSeedingStrategy(entityFactory, repositoryManager, _loggerMock.Object);

        // Act
        await strategy.ExecuteSeedingAsync();
        await repositoryManager.SaveChangesAsync();

        // Assert
        int permissionCount = await context.Permissions.CountAsync(p => p.Resource == "system" && p.Action == "all");
        permissionCount.Should().Be(1); // Should still be just one
    }

    [Fact]
    public async Task ExecuteSeedingAsync_WhenRoleDoesNotExist_ShouldCreateNewRole()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);

        var repositoryManager = new SuperAdminRepositoryManager(context, _repositoryLoggerMock.Object);
        var entityFactory = new SuperAdminEntityFactory(_passwordServiceMock.Object, _userErrors);
        var strategy = new SuperAdminSeedingStrategy(entityFactory, repositoryManager, _loggerMock.Object);

        // Act
        await strategy.ExecuteSeedingAsync();
        await repositoryManager.SaveChangesAsync();

        // Assert
        RoleEntity? role = await context.Roles.FirstOrDefaultAsync(r => r.Name == "SuperAdmin");
        role.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteSeedingAsync_WhenRoleExists_ShouldNotCreateDuplicateRole()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);

        // Pre-seed role
        var existingRole = RoleEntity.Create(Guid.NewGuid(), "SuperAdmin", "Existing", _userErrors);
        await context.Roles.AddAsync(existingRole);
        await context.SaveChangesAsync();

        var repositoryManager = new SuperAdminRepositoryManager(context, _repositoryLoggerMock.Object);
        var entityFactory = new SuperAdminEntityFactory(_passwordServiceMock.Object, _userErrors);
        var strategy = new SuperAdminSeedingStrategy(entityFactory, repositoryManager, _loggerMock.Object);

        // Act
        await strategy.ExecuteSeedingAsync();
        await repositoryManager.SaveChangesAsync();

        // Assert
        int roleCount = await context.Roles.CountAsync(r => r.Name == "SuperAdmin");
        roleCount.Should().Be(1); // Should still be just one
    }

    [Fact]
    public async Task ExecuteSeedingAsync_WhenRolePermissionDoesNotExist_ShouldCreateAssociation()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);

        var repositoryManager = new SuperAdminRepositoryManager(context, _repositoryLoggerMock.Object);
        var entityFactory = new SuperAdminEntityFactory(_passwordServiceMock.Object, _userErrors);
        var strategy = new SuperAdminSeedingStrategy(entityFactory, repositoryManager, _loggerMock.Object);

        // Act
        await strategy.ExecuteSeedingAsync();
        await repositoryManager.SaveChangesAsync();

        // Assert
        RolePermissionEntity? rolePermission = await context.RolePermissions.FirstOrDefaultAsync();
        rolePermission.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteSeedingAsync_WhenRolePermissionExists_ShouldNotCreateDuplicateAssociation()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);

        // Pre-seed everything
        var permission = PermissionEntity.Create(Guid.NewGuid(), "system", "all", "Test", _userErrors);
        var role = RoleEntity.Create(Guid.NewGuid(), "SuperAdmin", "Test", _userErrors);
        var rolePermission = RolePermissionEntity.Create(Guid.NewGuid(), role.Id, permission.Id);
        await context.Permissions.AddAsync(permission);
        await context.Roles.AddAsync(role);
        await context.RolePermissions.AddAsync(rolePermission);
        await context.SaveChangesAsync();

        var repositoryManager = new SuperAdminRepositoryManager(context, _repositoryLoggerMock.Object);
        var entityFactory = new SuperAdminEntityFactory(_passwordServiceMock.Object, _userErrors);
        var strategy = new SuperAdminSeedingStrategy(entityFactory, repositoryManager, _loggerMock.Object);

        // Act
        await strategy.ExecuteSeedingAsync();
        await repositoryManager.SaveChangesAsync();

        // Assert
        int rolePermissionCount = await context.RolePermissions.CountAsync();
        rolePermissionCount.Should().Be(1); // Should still be just one
    }

    [Fact]
    public async Task ExecuteSeedingAsync_ShouldCreateSuperAdminUser()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);

        var repositoryManager = new SuperAdminRepositoryManager(context, _repositoryLoggerMock.Object);
        var entityFactory = new SuperAdminEntityFactory(_passwordServiceMock.Object, _userErrors);
        var strategy = new SuperAdminSeedingStrategy(entityFactory, repositoryManager, _loggerMock.Object);

        // Act
        await strategy.ExecuteSeedingAsync();
        await repositoryManager.SaveChangesAsync();

        // Assert
        UserEntity? user = await context.Users.FirstOrDefaultAsync();
        user.Should().NotBeNull();
        user.UserName.Should().Be(SuperAdminConfiguration.Username);
    }

    [Fact]
    public async Task ExecuteSeedingAsync_ShouldCreateTheUserTokenState()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);

        var repositoryManager = new SuperAdminRepositoryManager(context, _repositoryLoggerMock.Object);
        var entityFactory = new SuperAdminEntityFactory(_passwordServiceMock.Object, _userErrors);
        var strategy = new SuperAdminSeedingStrategy(entityFactory, repositoryManager, _loggerMock.Object);

        // Act
        await strategy.ExecuteSeedingAsync();
        await repositoryManager.SaveChangesAsync();

        // Assert
        UserEntity user = await context.Users.FirstAsync();
        UserTokenStateEntity? tokenState = await context.UserTokenStates.FirstOrDefaultAsync();
        tokenState.Should().NotBeNull();
        tokenState!.Id.Should().Be(user.Id);
        tokenState.SecurityStamp.Should().NotBe(Guid.Empty);
        tokenState.TokenVersion.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteSeedingAsync_WhenUserRoleDoesNotExist_ShouldCreateAssociation()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);

        var repositoryManager = new SuperAdminRepositoryManager(context, _repositoryLoggerMock.Object);
        var entityFactory = new SuperAdminEntityFactory(_passwordServiceMock.Object, _userErrors);
        var strategy = new SuperAdminSeedingStrategy(entityFactory, repositoryManager, _loggerMock.Object);

        // Act
        await strategy.ExecuteSeedingAsync();
        await repositoryManager.SaveChangesAsync();

        // Assert
        UserRoleEntity? userRole = await context.UserRoles.FirstOrDefaultAsync();
        userRole.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteSeedingAsync_ShouldLogCompletionMessage()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);

        var repositoryManager = new SuperAdminRepositoryManager(context, _repositoryLoggerMock.Object);
        var entityFactory = new SuperAdminEntityFactory(_passwordServiceMock.Object, _userErrors);
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
        var entityFactory = new SuperAdminEntityFactory(_passwordServiceMock.Object, _userErrors);
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
        var entityFactory = new SuperAdminEntityFactory(_passwordServiceMock.Object, _userErrors);
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
        var entityFactory = new SuperAdminEntityFactory(_passwordServiceMock.Object, _userErrors);
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
