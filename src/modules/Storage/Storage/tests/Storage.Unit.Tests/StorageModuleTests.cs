using _116.Storage.Application.Shared.Persistence;
using _116.Storage.Application.Shared.Repositories;
using _116.Storage.Application.Shared.Services;
using _116.Storage.Infrastructure;
using _116.Storage.Infrastructure.Persistence;
using _116.Storage.Infrastructure.Repositories;
using _116.Storage.Infrastructure.Services;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Xunit;

namespace _116.Storage.Unit.Tests;

/// <summary>
/// Unit tests for <see cref="StorageModule"/>.
/// </summary>
public class StorageModuleTests : IDisposable
{
    private readonly ServiceCollection _services;
    private readonly TestDatabaseEnvironment _environment = new();
    private readonly CloudinarySettings _cloudinarySettings;

    public StorageModuleTests()
    {
        _services = [];
        _services.AddLogging();
        _services.AddLocalization();
        _services.AddSingleton(HostEnvironment("Testing"));

        _cloudinarySettings = new CloudinarySettings
        {
            CloudName = "test-cloud",
            ApiKey = "test-key",
            ApiSecret = "test-secret",
        };
    }

    public void Dispose()
    {
        _environment.Dispose();
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
        services.AddDbContext<StorageDbContext>(options => options.UseInMemoryDatabase("TestDb"));

        var cloudinarySettings = new CloudinarySettings
        {
            CloudName = "test-cloud",
            ApiKey = "test-key",
            ApiSecret = "test-secret",
        };
        services.AddSingleton(cloudinarySettings);
        services.AddSingleton(HostEnvironment("Testing"));

        // Act
        services.AddCoreModule(HostEnvironment("Testing"));
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Assert
        var dbContext = serviceProvider.GetService<StorageDbContext>();
        dbContext.Should().NotBeNull();
    }

    [Fact]
    public void AddCoreModule_ShouldRegisterCoreUnitOfWork()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLocalization();
        services.AddDbContext<StorageDbContext>(options => options.UseInMemoryDatabase("TestDb"));

        var cloudinarySettings = new CloudinarySettings
        {
            CloudName = "test-cloud",
            ApiKey = "test-key",
            ApiSecret = "test-secret",
        };
        services.AddSingleton(cloudinarySettings);
        services.AddSingleton(HostEnvironment("Testing"));

        // Act
        services.AddCoreModule(HostEnvironment("Testing"));
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Assert
        var unitOfWork = serviceProvider.GetService<IStorageUnitOfWork>();
        unitOfWork.Should().NotBeNull();
        unitOfWork.Should().BeOfType<StorageUnitOfWork>();
    }

    [Fact]
    public void AddCoreModule_ShouldRegisterFileRepository()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLocalization();
        services.AddDbContext<StorageDbContext>(options => options.UseInMemoryDatabase("TestDb"));

        var cloudinarySettings = new CloudinarySettings
        {
            CloudName = "test-cloud",
            ApiKey = "test-key",
            ApiSecret = "test-secret",
        };
        services.AddSingleton(cloudinarySettings);
        services.AddSingleton(HostEnvironment("Testing"));

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
        services.AddDbContext<StorageDbContext>(options => options.UseInMemoryDatabase("TestDb"));

        var cloudinarySettings = new CloudinarySettings
        {
            CloudName = "test-cloud",
            ApiKey = "test-key",
            ApiSecret = "test-secret",
        };
        services.AddSingleton(cloudinarySettings);
        services.AddSingleton(HostEnvironment("Testing"));

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
        services.AddDbContext<StorageDbContext>(options => options.UseInMemoryDatabase("TestDb"));

        var cloudinarySettings = new CloudinarySettings
        {
            CloudName = "test-cloud",
            ApiKey = "test-key",
            ApiSecret = "test-secret",
        };
        services.AddSingleton(cloudinarySettings);
        services.AddSingleton(HostEnvironment("Testing"));

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
    public void AddCoreModule_ShouldRegisterAllServices()
    {
        // Arrange & Act
        _services.AddSingleton(_cloudinarySettings);
        IServiceCollection result = _services.AddCoreModule(HostEnvironment("Testing"));

        ServiceProvider serviceProvider = _services.BuildServiceProvider();

        // Assert - verify all services are registered
        serviceProvider.GetService<StorageDbContext>().Should().NotBeNull();
        serviceProvider.GetService<IStorageUnitOfWork>().Should().NotBeNull();
        serviceProvider.GetService<IFileRepository>().Should().NotBeNull();
        serviceProvider.GetService<IFileService>().Should().NotBeNull();
        serviceProvider.GetService<ICloudinaryService>().Should().NotBeNull();
        result.Should().BeSameAs(_services);
    }
}
