using _116.Shared.Infrastructure.Extensions;
using _116.Shared.Infrastructure.Seed;
using AwesomeAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Shared.Infrastructure.Extensions;

// Minimal DbContext used only to exercise UseMigration<T> — no entities, no migration files.
// MigrateAsync() on SQLite in-memory with an empty context simply creates __EFMigrationsHistory.
file sealed class TestMigrationDbContext(DbContextOptions<TestMigrationDbContext> options) : DbContext(options);

/// <summary>
/// Unit tests for <see cref="ApplicationBuilderExtension"/>.
/// </summary>
public class ApplicationBuilderExtensionTests
{
    [Fact]
    public void UseMigration_ShouldReturnSameApplicationBuilder()
    {
        // Arrange — SQLite in-memory is a relational provider, so MigrateAsync() is supported.
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var services = new ServiceCollection();
        services.AddDbContext<TestMigrationDbContext>(o => o.UseSqlite(connection));
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        var app = new ApplicationBuilder(serviceProvider);

        // Act
        IApplicationBuilder result = app.UseMigration<TestMigrationDbContext>();

        // Assert
        result.Should().BeSameAs(app);
    }

    [Fact]
    public void UseMigration_ShouldNotThrow()
    {
        // Arrange
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var services = new ServiceCollection();
        services.AddDbContext<TestMigrationDbContext>(o => o.UseSqlite(connection));
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        var app = new ApplicationBuilder(serviceProvider);

        // Act
        Action act = () => app.UseMigration<TestMigrationDbContext>();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void UseSeed_WithNoRegisteredSeeders_ShouldNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        var app = new ApplicationBuilder(serviceProvider);

        // Act
        Func<IApplicationBuilder> act = () => app.UseSeed();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void UseSeed_ShouldReturnSameApplicationBuilder()
    {
        // Arrange
        var services = new ServiceCollection();
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        var app = new ApplicationBuilder(serviceProvider);

        // Act
        IApplicationBuilder result = app.UseSeed();

        // Assert
        result.Should().BeSameAs(app, "method should return the same app builder for chaining");
    }

    [Fact]
    public void UseSeed_WithRegisteredSeeders_ShouldExecuteAllSeeders()
    {
        // Arrange
        var services = new ServiceCollection();
        var seeder1Mock = new Mock<IDataSeeder>();
        var seeder2Mock = new Mock<IDataSeeder>();

        seeder1Mock.Setup(s => s.SeedAllAsync()).Returns(Task.CompletedTask);
        seeder2Mock.Setup(s => s.SeedAllAsync()).Returns(Task.CompletedTask);

        services.AddSingleton(seeder1Mock.Object);
        services.AddSingleton(seeder2Mock.Object);

        ServiceProvider serviceProvider = services.BuildServiceProvider();
        var app = new ApplicationBuilder(serviceProvider);

        // Act
        app.UseSeed();

        // Assert
        seeder1Mock.Verify(s => s.SeedAllAsync(), Times.Once);
        seeder2Mock.Verify(s => s.SeedAllAsync(), Times.Once);
    }

    [Fact]
    public void UseSeed_WithMultipleSeeders_ShouldExecuteInSequence()
    {
        // Arrange
        var services = new ServiceCollection();
        var executionOrder = new List<int>();

        var seeder1Mock = new Mock<IDataSeeder>();
        seeder1Mock
            .Setup(s => s.SeedAllAsync())
            .Returns(() =>
            {
                executionOrder.Add(1);
                return Task.CompletedTask;
            });

        var seeder2Mock = new Mock<IDataSeeder>();
        seeder2Mock
            .Setup(s => s.SeedAllAsync())
            .Returns(() =>
            {
                executionOrder.Add(2);
                return Task.CompletedTask;
            });

        services.AddSingleton(seeder1Mock.Object);
        services.AddSingleton(seeder2Mock.Object);

        ServiceProvider serviceProvider = services.BuildServiceProvider();
        var app = new ApplicationBuilder(serviceProvider);

        // Act
        app.UseSeed();

        // Assert
        executionOrder.Should().Equal([1, 2], "seeders should execute in registration order");
    }

    [Fact]
    public void UseSeed_WhenSeederThrowsException_ShouldPropagateException()
    {
        // Arrange
        var services = new ServiceCollection();
        var seederMock = new Mock<IDataSeeder>();
        seederMock.Setup(s => s.SeedAllAsync()).ThrowsAsync(new InvalidOperationException("Seeding failed"));

        services.AddSingleton(seederMock.Object);
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        var app = new ApplicationBuilder(serviceProvider);

        // Act
        Func<IApplicationBuilder> act = () => app.UseSeed();

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("Seeding failed");
    }
}
