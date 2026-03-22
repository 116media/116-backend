using _116.Identity.Application.Auth.Services;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Identity.Infrastructure.Persistence.Seeds.SuperAdmin;
using AwesomeAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Infrastructure.Persistence.Seeds.SuperAdmin;

/// <summary>
/// Collection definition to prevent parallel test execution for SuperAdminSeeder tests.
/// </summary>
[CollectionDefinition("SuperAdminSeeder", DisableParallelization = true)]
public class SuperAdminSeederCollection { }

/// <summary>
/// Unit tests for <see cref="SuperAdminSeeder"/> using SQLite in-memory database,
/// which supports transactions unlike the EF Core InMemory provider.
/// </summary>
[Collection("SuperAdminSeeder")]
public class SuperAdminSeederTests : IDisposable
{
    private readonly Mock<ILogger<SuperAdminSeeder>> _seederLoggerMock;
    private readonly Mock<ILogger<SuperAdminRepositoryManager>> _repositoryLoggerMock;
    private readonly Mock<ILogger<SuperAdminSeedingStrategy>> _strategyLoggerMock;
    private readonly Mock<IPasswordService> _passwordServiceMock;
    private readonly string? _originalPassword;

    public SuperAdminSeederTests()
    {
        _seederLoggerMock = new Mock<ILogger<SuperAdminSeeder>>();
        _repositoryLoggerMock = new Mock<ILogger<SuperAdminRepositoryManager>>();
        _strategyLoggerMock = new Mock<ILogger<SuperAdminSeedingStrategy>>();
        _passwordServiceMock = new Mock<IPasswordService>();
        _passwordServiceMock.Setup(x => x.Hash(It.IsAny<string>())).Returns("hashedPassword");

        _originalPassword = Environment.GetEnvironmentVariable("DEFAULT_USER_PASSWORD");
        Environment.SetEnvironmentVariable("DEFAULT_USER_PASSWORD", "TestPassword123!");
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable("DEFAULT_USER_PASSWORD", _originalPassword);
        GC.SuppressFinalize(this);
    }

    private static (SqliteConnection connection, DbContextOptions<IdentityDbContext> options) CreateSqliteOptions()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        DbContextOptions<IdentityDbContext> options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseSqlite(connection)
            .UseSnakeCaseNamingConvention()
            .Options;
        return (connection, options);
    }

    private SuperAdminSeeder CreateSeeder(IdentityDbContext context)
    {
        return new SuperAdminSeeder(
            context,
            _passwordServiceMock.Object,
            _seederLoggerMock.Object,
            _repositoryLoggerMock.Object,
            _strategyLoggerMock.Object
        );
    }

    #region SeedAllAsync — Happy Path (SuperAdmin does not exist)

    [Fact]
    public async Task SeedAllAsync_WhenSuperAdminDoesNotExist_ShouldCreateSuperAdminUser()
    {
        // Arrange
        var (connection, options) = CreateSqliteOptions();
        await using var _ = connection;
        await using var context = new IdentityDbContext(options);
        await context.Database.EnsureCreatedAsync();

        SuperAdminSeeder seeder = CreateSeeder(context);

        // Act
        await seeder.SeedAllAsync();

        // Assert
        UserEntity? user = await context.Users.FirstOrDefaultAsync(u => u.Email == SuperAdminConfiguration.Email);
        user.Should().NotBeNull();
        user!.UserName.Should().Be(SuperAdminConfiguration.Username);
    }

    [Fact]
    public async Task SeedAllAsync_WhenSuperAdminDoesNotExist_ShouldCreateSuperAdminRole()
    {
        // Arrange
        var (connection, options) = CreateSqliteOptions();
        await using var _ = connection;
        await using var context = new IdentityDbContext(options);
        await context.Database.EnsureCreatedAsync();

        SuperAdminSeeder seeder = CreateSeeder(context);

        // Act
        await seeder.SeedAllAsync();

        // Assert
        RoleEntity? role = await context.Roles.FirstOrDefaultAsync(r => r.Name == SuperAdminConfiguration.RoleName);
        role.Should().NotBeNull();
    }

    [Fact]
    public async Task SeedAllAsync_WhenSuperAdminDoesNotExist_ShouldCreateSystemPermission()
    {
        // Arrange
        var (connection, options) = CreateSqliteOptions();
        await using var _ = connection;
        await using var context = new IdentityDbContext(options);
        await context.Database.EnsureCreatedAsync();

        SuperAdminSeeder seeder = CreateSeeder(context);

        // Act
        await seeder.SeedAllAsync();

        // Assert
        PermissionEntity? permission = await context.Permissions.FirstOrDefaultAsync(p =>
            p.Resource == SuperAdminConfiguration.PermissionResource
            && p.Action == SuperAdminConfiguration.PermissionAction
        );
        permission.Should().NotBeNull();
    }

    [Fact]
    public async Task SeedAllAsync_WhenSuperAdminDoesNotExist_ShouldLogCompletionMessage()
    {
        // Arrange
        var (connection, options) = CreateSqliteOptions();
        await using var _ = connection;
        await using var context = new IdentityDbContext(options);
        await context.Database.EnsureCreatedAsync();

        SuperAdminSeeder seeder = CreateSeeder(context);

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
            Times.AtLeastOnce
        );
    }

    [Fact]
    public async Task SeedAllAsync_WhenSuperAdminDoesNotExist_ShouldHashPassword()
    {
        // Arrange
        var (connection, options) = CreateSqliteOptions();
        await using var _ = connection;
        await using var context = new IdentityDbContext(options);
        await context.Database.EnsureCreatedAsync();

        SuperAdminSeeder seeder = CreateSeeder(context);

        // Act
        await seeder.SeedAllAsync();

        // Assert
        _passwordServiceMock.Verify(x => x.Hash(It.IsAny<string>()), Times.Once);
    }

    #endregion

    #region SeedAllAsync — Already Exists (skip path)

    [Fact]
    public async Task SeedAllAsync_WhenSuperAdminAlreadyExists_ShouldSkipSeeding()
    {
        // Arrange
        var (connection, options) = CreateSqliteOptions();
        await using var _ = connection;
        await using var context = new IdentityDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var existingUser = UserEntity.Create(
            Guid.NewGuid(),
            SuperAdminConfiguration.Email,
            "existingadmin",
            "hashedPassword"
        );
        await context.Users.AddAsync(existingUser);
        await context.SaveChangesAsync();

        SuperAdminSeeder seeder = CreateSeeder(context);

        // Act
        await seeder.SeedAllAsync();

        // Assert — user count stays at 1, no new seeding occurred
        int userCount = await context.Users.CountAsync();
        userCount.Should().Be(1);
    }

    [Fact]
    public async Task SeedAllAsync_WhenSuperAdminAlreadyExists_ShouldLogSkipMessage()
    {
        // Arrange
        var (connection, options) = CreateSqliteOptions();
        await using var _ = connection;
        await using var context = new IdentityDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var existingUser = UserEntity.Create(
            Guid.NewGuid(),
            SuperAdminConfiguration.Email,
            "existingadmin",
            "hashedPassword"
        );
        await context.Users.AddAsync(existingUser);
        await context.SaveChangesAsync();

        SuperAdminSeeder seeder = CreateSeeder(context);

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

    #endregion

    #region SeedAllAsync — Exception / Rollback Path

    [Fact]
    public async Task SeedAllAsync_WhenExceptionOccurs_ShouldRethrowException()
    {
        // Arrange — force exception inside the transaction via Hash()
        _passwordServiceMock
            .Setup(x => x.Hash(It.IsAny<string>()))
            .Throws(new InvalidOperationException("Hash failed"));

        var (connection, options) = CreateSqliteOptions();
        await using var _ = connection;
        await using var context = new IdentityDbContext(options);
        await context.Database.EnsureCreatedAsync();

        SuperAdminSeeder seeder = CreateSeeder(context);

        // Act & Assert
        Func<Task> act = async () => await seeder.SeedAllAsync();
        await act.Should().ThrowExactlyAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task SeedAllAsync_WhenExceptionOccurs_ShouldLogError()
    {
        // Arrange
        _passwordServiceMock
            .Setup(x => x.Hash(It.IsAny<string>()))
            .Throws(new InvalidOperationException("Hash failed"));

        var (connection, options) = CreateSqliteOptions();
        await using var _ = connection;
        await using var context = new IdentityDbContext(options);
        await context.Database.EnsureCreatedAsync();

        SuperAdminSeeder seeder = CreateSeeder(context);

        // Act
        try
        {
            await seeder.SeedAllAsync();
        }
        catch
        { /* expected */
        }

        // Assert — outer catch logs "Failed to seed Super Admin data"
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

    [Fact]
    public async Task SeedAllAsync_WhenExceptionOccursDuringTransaction_ShouldLogRollback()
    {
        // Arrange
        _passwordServiceMock
            .Setup(x => x.Hash(It.IsAny<string>()))
            .Throws(new InvalidOperationException("Hash failed"));

        var (connection, options) = CreateSqliteOptions();
        await using var _ = connection;
        await using var context = new IdentityDbContext(options);
        await context.Database.EnsureCreatedAsync();

        SuperAdminSeeder seeder = CreateSeeder(context);

        // Act
        try
        {
            await seeder.SeedAllAsync();
        }
        catch
        { /* expected */
        }

        // Assert — inner catch inside ExecuteSeedingWithTransactionAsync logs rollback
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
