using _116.Core;
using _116.Core.Application.Shared.Persistence;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Application.Shared.Services;
using _116.Core.Infrastructure.Persistence;
using _116.Core.Infrastructure.Repositories;
using _116.Core.Infrastructure.Services;
using _116.Shared.Application.Configurations;
using _116.Unit.Tests.Common;
using AwesomeAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Core;

/// <summary>
/// Unit tests for <see cref="CoreModule"/>.
/// </summary>
public class CoreModuleTests : IDisposable
{
    private readonly ServiceCollection _services;
    private readonly CloudinarySettings _cloudinarySettings;

    public CoreModuleTests()
    {
        _services = [];
        _services.AddLogging();
        _services.AddLocalization();

        _cloudinarySettings = new CloudinarySettings
        {
            CloudName = "test-cloud",
            ApiKey = "test-key",
            ApiSecret = "test-secret",
        };
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Builds a host environment stub reporting the given name.
    /// </summary>
    /// <param name="name">The environment name the stub reports.</param>
    /// <returns>The stubbed host environment.</returns>
    private static IHostEnvironment HostEnvironment(string name)
    {
        var environment = new Mock<IHostEnvironment>();
        environment.SetupGet(host => host.EnvironmentName).Returns(name);

        return environment.Object;
    }

    [Fact]
    public void AddCoreModule_ShouldRegisterCoreDbContext()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLocalization();
        services.AddDbContext<CoreDbContext>(options => options.UseInMemoryDatabase("TestDb"));

        var cloudinarySettings = new CloudinarySettings
        {
            CloudName = "test-cloud",
            ApiKey = "test-key",
            ApiSecret = "test-secret",
        };
        services.AddSingleton(cloudinarySettings);

        // Act
        services.AddCoreModule(HostEnvironment("Testing"));
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Assert
        var dbContext = serviceProvider.GetService<CoreDbContext>();
        dbContext.Should().NotBeNull();
    }

    [Fact]
    public void AddCoreModule_ShouldRegisterCoreUnitOfWork()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLocalization();
        services.AddDbContext<CoreDbContext>(options => options.UseInMemoryDatabase("TestDb"));

        var cloudinarySettings = new CloudinarySettings
        {
            CloudName = "test-cloud",
            ApiKey = "test-key",
            ApiSecret = "test-secret",
        };
        services.AddSingleton(cloudinarySettings);

        // Act
        services.AddCoreModule(HostEnvironment("Testing"));
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Assert
        var unitOfWork = serviceProvider.GetService<ICoreUnitOfWork>();
        unitOfWork.Should().NotBeNull();
        unitOfWork.Should().BeOfType<CoreUnitOfWork>();
    }

    [Fact]
    public void AddCoreModule_ShouldRegisterFileRepository()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLocalization();
        services.AddDbContext<CoreDbContext>(options => options.UseInMemoryDatabase("TestDb"));

        var cloudinarySettings = new CloudinarySettings
        {
            CloudName = "test-cloud",
            ApiKey = "test-key",
            ApiSecret = "test-secret",
        };
        services.AddSingleton(cloudinarySettings);

        // Act
        services.AddCoreModule(HostEnvironment("Testing"));
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Assert
        var repository = serviceProvider.GetService<IFileRepository>();
        repository.Should().NotBeNull();
        repository.Should().BeOfType<FileRepository>();
    }

    [Fact]
    public void AddCoreModule_ShouldRegisterFileService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLocalization();
        services.AddDbContext<CoreDbContext>(options => options.UseInMemoryDatabase("TestDb"));

        var cloudinarySettings = new CloudinarySettings
        {
            CloudName = "test-cloud",
            ApiKey = "test-key",
            ApiSecret = "test-secret",
        };
        services.AddSingleton(cloudinarySettings);

        // Act
        services.AddCoreModule(HostEnvironment("Testing"));
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Assert
        var fileService = serviceProvider.GetService<IFileService>();
        fileService.Should().NotBeNull();
        fileService.Should().BeOfType<FileService>();
    }

    [Fact]
    public void AddCoreModule_ShouldRegisterCloudinaryService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLocalization();
        services.AddDbContext<CoreDbContext>(options => options.UseInMemoryDatabase("TestDb"));

        var cloudinarySettings = new CloudinarySettings
        {
            CloudName = "test-cloud",
            ApiKey = "test-key",
            ApiSecret = "test-secret",
        };
        services.AddSingleton(cloudinarySettings);

        // Act
        services.AddCoreModule(HostEnvironment("Testing"));
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Assert
        var cloudinaryService = serviceProvider.GetService<ICloudinaryService>();
        cloudinaryService.Should().NotBeNull();
        cloudinaryService.Should().BeOfType<CloudinaryService>();
    }

    [Fact]
    public void AddCoreModule_ShouldReturnServiceCollection()
    {
        // Arrange
        _services.AddSingleton(_cloudinarySettings);

        // Act
        IServiceCollection result = _services.AddCoreModule(HostEnvironment("Testing"));

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(_services);
    }

    [Fact]
    public void AddCoreModule_ShouldRegisterHttpClient()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLocalization();
        services.AddSingleton(_cloudinarySettings);

        // Act
        services.AddCoreModule(HostEnvironment("Testing"));
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Assert
        var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
        httpClientFactory.Should().NotBeNull();

        HttpClient httpClient = httpClientFactory.CreateClient(nameof(FileService));
        httpClient.Should().NotBeNull();
    }

    [Fact]
    public void UseCoreModule_ShouldRunTheMigrationStepAndReturnAppBuilder()
    {
        // Arrange — the migrator is replaced so the startup migration completes
        // without a database while the rest of the pipeline runs for real.
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLocalization();
        services.AddSingleton(_cloudinarySettings);
        services.AddDbContext<CoreDbContext>(options =>
            options
                .UseNpgsql("Host=localhost;Port=5432;Database=unit;Username=unit;Password=unit")
                .ReplaceService<IMigrator, NoOpMigrator>()
        );

        services.AddSingleton<IHostEnvironment>(HostEnvironment("Development"));
        services.AddCoreModule(HostEnvironment("Development"));

        ServiceProvider serviceProvider = services.BuildServiceProvider();
        var app = new ApplicationBuilder(serviceProvider);

        // Act
        IApplicationBuilder result = app.UseCoreModule();

        // Assert
        result.Should().BeSameAs(app);
    }

    [Fact]
    public void AddCoreModule_ShouldRegisterAllServices()
    {
        // Arrange & Act
        _services.AddSingleton(_cloudinarySettings);
        IServiceCollection result = _services.AddCoreModule(HostEnvironment("Testing"));

        ServiceProvider serviceProvider = _services.BuildServiceProvider();

        // Assert - verify all services are registered
        serviceProvider.GetService<CoreDbContext>().Should().NotBeNull();
        serviceProvider.GetService<ICoreUnitOfWork>().Should().NotBeNull();
        serviceProvider.GetService<IFileRepository>().Should().NotBeNull();
        serviceProvider.GetService<IFileService>().Should().NotBeNull();
        serviceProvider.GetService<ICloudinaryService>().Should().NotBeNull();
        result.Should().BeSameAs(_services);
    }
}
