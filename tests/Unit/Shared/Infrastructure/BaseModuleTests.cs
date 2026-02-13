using _116.Shared.Infrastructure;
using _116.Shared.Infrastructure.interceptors;
using AwesomeAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
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
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddModuleDatabase_WithCustomConnectionString_ShouldUseProvidedConnectionString()
    {
        // Arrange
        var services = new ServiceCollection();
        string customConnectionString =
            "Host=custom;Port=5433;Database=custom_db;Username=custom_user;Password=custom_pass;";
        var options = new ModuleOptions<TestDbContext>
        {
            ModuleName = "Test",
            ConnectionString = customConnectionString,
        };

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
        var options = new ModuleOptions<TestDbContext>
        {
            ModuleName = "Test",
            UseConnectionPooling = true,
            ConnectionString = "Host=localhost;Port=5432;Database=test;Username=test;Password=test;",
        };

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
        var options = new ModuleOptions<TestDbContext>
        {
            ModuleName = "Test",
            UseConnectionPooling = false,
            ConnectionString = "Host=localhost;Port=5432;Database=test;Username=test;Password=test;",
        };

        // Act
        services.AddModuleDatabase(options);

        // Assert
        ServiceDescriptor? descriptor = services.FirstOrDefault(s => s.ServiceType == typeof(TestDbContext));
        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddModuleDatabase_ShouldRegisterInterceptors()
    {
        // Arrange
        var services = new ServiceCollection();
        var options = new ModuleOptions<TestDbContext>
        {
            ModuleName = "Test",
            ConnectionString = "Host=localhost;Port=5432;Database=test;Username=test;Password=test;",
        };

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
            sp.GetRequiredService<IServiceScopeFactory>()
        ));

        var options = new ModuleOptions<TestDbContext>
        {
            ModuleName = "Test",
            ConnectionString = "Host=localhost;Port=5432;Database=test;Username=test;Password=test;",
        };

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
    public void AddModuleDatabase_WithCustomConfiguration_ShouldApplyConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();
        bool configurationApplied = false;
        var options = new ModuleOptions<TestDbContext>
        {
            ModuleName = "Test",
            ConnectionString = "Host=localhost;Port=5432;Database=test;Username=test;Password=test;",
            ConfigureDbContext = optionsBuilder =>
            {
                configurationApplied = true;
            },
        };

        // Act
        services.AddModuleDatabase(options);
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        var dbContext = serviceProvider.GetRequiredService<TestDbContext>();

        // Assert
        dbContext.Should().NotBeNull();
        configurationApplied.Should().BeTrue("custom configuration should be applied");
    }

    [Fact]
    public void AddModuleDatabase_ShouldReturnServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        var options = new ModuleOptions<TestDbContext>
        {
            ModuleName = "Test",
            ConnectionString = "Host=localhost;Port=5432;Database=test;Username=test;Password=test;",
        };

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
