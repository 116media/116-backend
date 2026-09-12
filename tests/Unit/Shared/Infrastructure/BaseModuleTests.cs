using System.Data.Common;
using _116.Shared.Infrastructure;
using _116.Shared.Infrastructure.interceptors;
using _116.Shared.Infrastructure.Seed;
using _116.Unit.Tests.Common.Helpers;
using AwesomeAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace _116.Unit.Tests.Shared.Infrastructure;

/// <summary>
/// Unit tests for <see cref="BaseModule"/>.
/// </summary>
public class BaseModuleTests
{
    private class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options)
            : base(options) { }
    }

    /// <summary>
    /// Seeder recording whether the pipeline executed it, standing in for a
    /// module's real seeders.
    /// </summary>
    [Fact]
    public void AddModuleDatabase_WithDefaultConnectionString_ShouldRegisterDbContext()
    {
        // Arrange
        var services = new ServiceCollection();
        var options = new ModuleOptions<TestDbContext> { ModuleName = "Test" };

        // Act
        services.AddModuleDatabase(options);

        // Assert
        ServiceDescriptor? descriptor = services.FirstOrDefault(s => s.ServiceType == typeof(TestDbContext));
        descriptor.Should().NotBeNull();
        descriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddModuleDatabase_ShouldResolveDbContextFromTheDefaultConnectionString()
    {
        // Arrange
        using var environment = new TestDatabaseEnvironment();
        var services = new ServiceCollection();
        var options = new ModuleOptions<TestDbContext> { ModuleName = "Test" };

        // Act
        services.AddModuleDatabase(options);

        // Assert
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        var dbContext = serviceProvider.GetRequiredService<TestDbContext>();
        dbContext.Should().NotBeNull();
    }

    [Fact]
    public void AddModuleDatabase_ShouldRegisterTheContextOverTheScopedSharedConnection()
    {
        // Arrange
        var services = new ServiceCollection();
        var options = new ModuleOptions<TestDbContext> { ModuleName = "Test" };

        // Act
        services.AddModuleDatabase(options);

        // Assert
        ServiceDescriptor? context = services.FirstOrDefault(s => s.ServiceType == typeof(TestDbContext));
        context.Should().NotBeNull();
        context!.Lifetime.Should().Be(ServiceLifetime.Scoped);

        ServiceDescriptor? connection = services.FirstOrDefault(s => s.ServiceType == typeof(DbConnection));
        connection.Should().NotBeNull();
        connection!.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddModuleDatabase_ShouldAlsoRegisterTheContextAsADbContext()
    {
        // Arrange
        using var environment = new TestDatabaseEnvironment();
        var services = new ServiceCollection();
        var options = new ModuleOptions<TestDbContext> { ModuleName = "Test" };

        // Act
        services.AddModuleDatabase(options);

        // Assert
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        using IServiceScope scope = serviceProvider.CreateScope();

        DbContext[] contexts = [.. scope.ServiceProvider.GetServices<DbContext>()];
        contexts.Should().ContainSingle().Which.Should().BeOfType<TestDbContext>();
    }

    [Fact]
    public void AddModuleDatabase_ShouldRegisterInterceptors()
    {
        // Arrange
        var services = new ServiceCollection();
        var options = new ModuleOptions<TestDbContext> { ModuleName = "Test" };

        // Act
        services.AddModuleDatabase(options);

        // Assert
        ServiceDescriptor? auditInterceptor = services.FirstOrDefault(s =>
            s.ServiceType == typeof(ISaveChangesInterceptor)
            && s.ImplementationType == typeof(AuditableEntityInterceptor)
        );
        auditInterceptor.Should().NotBeNull();

        ServiceDescriptor? domainEventInterceptor = services.FirstOrDefault(s =>
            s.ServiceType == typeof(ISaveChangesInterceptor)
            && s.ImplementationType == typeof(DispatchDomainEventsInterceptor)
        );
        domainEventInterceptor.Should().NotBeNull();
    }

    [Fact]
    public void AddModuleDatabase_ShouldRegisterTheSystemClockAsASingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        var options = new ModuleOptions<TestDbContext> { ModuleName = "Test" };

        // Act
        services.AddModuleDatabase(options);

        // Assert
        ServiceDescriptor descriptor = services
            .Should()
            .ContainSingle(s => s.ServiceType == typeof(TimeProvider))
            .Which;
        descriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);
        descriptor.ImplementationInstance.Should().BeSameAs(TimeProvider.System);
    }

    [Fact]
    public void AddModuleDatabase_WhenInterceptorsAlreadyRegistered_ShouldNotRegisterDuplicates()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        services.AddSingleton<ISaveChangesInterceptor>(sp => new DispatchDomainEventsInterceptor(
            sp.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<DispatchDomainEventsInterceptor>.Instance,
            TimeProvider.System
        ));

        var options = new ModuleOptions<TestDbContext> { ModuleName = "Test" };

        // Act
        services.AddModuleDatabase(options);

        // Assert
        List<ServiceDescriptor> auditInterceptors = services
            .Where(s =>
                s.ServiceType == typeof(ISaveChangesInterceptor)
                && s.ImplementationType == typeof(AuditableEntityInterceptor)
            )
            .ToList();
        auditInterceptors.Should().ContainSingle("interceptors should not be registered twice");
    }

    [Fact]
    public void AddModuleDatabase_ShouldReturnServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        var options = new ModuleOptions<TestDbContext> { ModuleName = "Test" };

        // Act
        IServiceCollection result = services.AddModuleDatabase(options);

        // Assert
        result.Should().BeSameAs(services, "method should return the service collection for chaining");
    }
}
