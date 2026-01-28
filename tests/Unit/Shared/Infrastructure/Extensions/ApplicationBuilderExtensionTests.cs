using _116.Shared.Infrastructure.Extensions;
using _116.Shared.Infrastructure.Seed;
using AwesomeAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Shared.Infrastructure.Extensions;

/// <summary>
/// Unit tests for <see cref="ApplicationBuilderExtension"/>.
/// </summary>
/// <remarks>
/// Note: UseMigration method requires a relational database provider and is primarily
/// tested through integration tests when the application starts up and applies migrations.
/// Unit testing this method would require adding SQLite dependencies or mocking EF Core internals.
/// </remarks>
public class ApplicationBuilderExtensionTests
{
    [Fact]
    public void UseSeed_WithNoRegisteredSeeders_ShouldNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var app = new ApplicationBuilder(serviceProvider);

        // Act
        var act = () => app.UseSeed();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void UseSeed_ShouldReturnSameApplicationBuilder()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var app = new ApplicationBuilder(serviceProvider);

        // Act
        var result = app.UseSeed();

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

        var serviceProvider = services.BuildServiceProvider();
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
        seeder1Mock.Setup(s => s.SeedAllAsync()).Returns(Task.Run(() => executionOrder.Add(1)));

        var seeder2Mock = new Mock<IDataSeeder>();
        seeder2Mock.Setup(s => s.SeedAllAsync()).Returns(Task.Run(() => executionOrder.Add(2)));

        services.AddSingleton(seeder1Mock.Object);
        services.AddSingleton(seeder2Mock.Object);

        var serviceProvider = services.BuildServiceProvider();
        var app = new ApplicationBuilder(serviceProvider);

        // Act
        app.UseSeed();

        // Assert
        executionOrder.Should().Equal(new[] { 1, 2 }, "seeders should execute in registration order");
    }

    [Fact]
    public void UseSeed_WhenSeederThrowsException_ShouldPropagateException()
    {
        // Arrange
        var services = new ServiceCollection();
        var seederMock = new Mock<IDataSeeder>();
        seederMock.Setup(s => s.SeedAllAsync()).ThrowsAsync(new InvalidOperationException("Seeding failed"));

        services.AddSingleton(seederMock.Object);
        var serviceProvider = services.BuildServiceProvider();
        var app = new ApplicationBuilder(serviceProvider);

        // Act
        var act = () => app.UseSeed();

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("Seeding failed");
    }
}
