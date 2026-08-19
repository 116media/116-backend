using _116.Shared.Infrastructure;
using _116.Shared.Infrastructure.interceptors;
using _116.Shared.Infrastructure.Seed;
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
    private sealed class RecordingSeeder : IDataSeeder
    {
        public bool WasExecuted { get; private set; }

        public Task SeedAllAsync()
        {
            WasExecuted = true;
            return Task.CompletedTask;
        }
    }

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
    public void AddModuleDatabase_WithConnectionPooling_ShouldRegisterDbContextPool()
    {
        // Arrange
        var services = new ServiceCollection();
        var options = new ModuleOptions<TestDbContext> { ModuleName = "Test", UseConnectionPooling = true };

        // Act
        services.AddModuleDatabase(options);

        // Assert
        ServiceDescriptor? descriptor = services.FirstOrDefault(s => s.ServiceType == typeof(TestDbContext));
        descriptor.Should().NotBeNull();
    }

    [Fact]
    public void AddModuleDatabase_WithoutConnectionPooling_ShouldRegisterDbContext()
    {
        // Arrange
        var services = new ServiceCollection();
        var options = new ModuleOptions<TestDbContext> { ModuleName = "Test", UseConnectionPooling = false };

        // Act
        services.AddModuleDatabase(options);

        // Assert
        ServiceDescriptor? descriptor = services.FirstOrDefault(s => s.ServiceType == typeof(TestDbContext));
        descriptor.Should().NotBeNull();
        descriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);
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
    public void AddModuleDatabase_WhenInterceptorsAlreadyRegistered_ShouldNotRegisterDuplicates()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        services.AddSingleton<ISaveChangesInterceptor>(sp => new DispatchDomainEventsInterceptor(
            sp.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<DispatchDomainEventsInterceptor>.Instance
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

    [Fact]
    public void UseModuleDatabase_WithMigrationsDisabled_ShouldReturnAppBuilder()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDbContext<TestDbContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

        ServiceProvider serviceProvider = services.BuildServiceProvider();
        var app = new ApplicationBuilder(serviceProvider);

        var options = new ModuleOptions<TestDbContext>
        {
            ModuleName = "Test",
            EnableMigrations = false,
            EnableSeeding = false,
        };

        // Act
        IApplicationBuilder result = app.UseModuleDatabase(options);

        // Assert
        result.Should().BeSameAs(app, "method should return the app builder for chaining");
    }

    /// <summary>
    /// Verifies that migrations enabled on the options run the module's
    /// migration step against the registered context and still hand the builder
    /// back for chaining.
    /// </summary>
    [Fact]
    public void UseModuleDatabase_WithMigrationsEnabled_ShouldMigrateAndReturnAppBuilder()
    {
        // Arrange — a relational provider, so the migration step is the real one.
        var services = new ServiceCollection();
        services.AddDbContext<TestDbContext>(options => options.UseSqlite("DataSource=:memory:"));

        ServiceProvider serviceProvider = services.BuildServiceProvider();
        var app = new ApplicationBuilder(serviceProvider);

        var options = new ModuleOptions<TestDbContext>
        {
            ModuleName = "Test",
            EnableMigrations = true,
            EnableSeeding = false,
        };

        // Act
        IApplicationBuilder result = app.UseModuleDatabase(options);

        // Assert
        result.Should().BeSameAs(app);
    }

    /// <summary>
    /// Verifies that seeding enabled on the options executes every registered
    /// seeder.
    /// </summary>
    [Fact]
    public void UseModuleDatabase_WithSeedingEnabled_ShouldRunTheRegisteredSeeders()
    {
        // Arrange
        var seeder = new RecordingSeeder();
        var services = new ServiceCollection();
        services.AddDbContext<TestDbContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        services.AddScoped<IDataSeeder>(_ => seeder);

        ServiceProvider serviceProvider = services.BuildServiceProvider();
        var app = new ApplicationBuilder(serviceProvider);

        var options = new ModuleOptions<TestDbContext>
        {
            ModuleName = "Test",
            EnableMigrations = false,
            EnableSeeding = true,
        };

        // Act
        IApplicationBuilder result = app.UseModuleDatabase(options);

        // Assert
        seeder.WasExecuted.Should().BeTrue();
        result.Should().BeSameAs(app);
    }

    [Fact]
    public void UseModuleDatabase_WithSeedingDisabled_ShouldReturnAppBuilder()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDbContext<TestDbContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

        ServiceProvider serviceProvider = services.BuildServiceProvider();
        var app = new ApplicationBuilder(serviceProvider);

        var options = new ModuleOptions<TestDbContext>
        {
            ModuleName = "Test",
            EnableMigrations = false,
            EnableSeeding = false,
        };

        // Act
        IApplicationBuilder result = app.UseModuleDatabase(options);

        // Assert
        result.Should().BeSameAs(app);
    }
}
