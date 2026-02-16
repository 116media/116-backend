using _116.Core;
using _116.Core.Application.Shared.Persistence;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Application.Shared.Services;
using _116.Core.Infrastructure.Persistence;
using _116.Core.Infrastructure.Repositories;
using _116.Core.Infrastructure.Services;
using _116.Shared.Application.Configurations;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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

        _cloudinarySettings = new CloudinarySettings
        {
            CloudName = "test-cloud",
            ApiKey = "test-key",
            ApiSecret = "test-secret",
        };
    }

    public void Dispose()
    {
        // Cleanup if needed
    }

    [Fact]
    public void AddCoreModule_ShouldRegisterCoreDbContext()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<CoreDbContext>(options => options.UseInMemoryDatabase("TestDb"));

        var cloudinarySettings = new CloudinarySettings
        {
            CloudName = "test-cloud",
            ApiKey = "test-key",
            ApiSecret = "test-secret",
        };
        services.AddSingleton(cloudinarySettings);

        // Act
        services.AddCoreModule();
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
        services.AddDbContext<CoreDbContext>(options => options.UseInMemoryDatabase("TestDb"));

        var cloudinarySettings = new CloudinarySettings
        {
            CloudName = "test-cloud",
            ApiKey = "test-key",
            ApiSecret = "test-secret",
        };
        services.AddSingleton(cloudinarySettings);

        // Act
        services.AddCoreModule();
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
        services.AddDbContext<CoreDbContext>(options => options.UseInMemoryDatabase("TestDb"));

        var cloudinarySettings = new CloudinarySettings
        {
            CloudName = "test-cloud",
            ApiKey = "test-key",
            ApiSecret = "test-secret",
        };
        services.AddSingleton(cloudinarySettings);

        // Act
        services.AddCoreModule();
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
        services.AddDbContext<CoreDbContext>(options => options.UseInMemoryDatabase("TestDb"));

        var cloudinarySettings = new CloudinarySettings
        {
            CloudName = "test-cloud",
            ApiKey = "test-key",
            ApiSecret = "test-secret",
        };
        services.AddSingleton(cloudinarySettings);

        // Act
        services.AddCoreModule();
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
        services.AddDbContext<CoreDbContext>(options => options.UseInMemoryDatabase("TestDb"));

        var cloudinarySettings = new CloudinarySettings
        {
            CloudName = "test-cloud",
            ApiKey = "test-key",
            ApiSecret = "test-secret",
        };
        services.AddSingleton(cloudinarySettings);

        // Act
        services.AddCoreModule();
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
        IServiceCollection result = _services.AddCoreModule();

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
        services.AddSingleton(_cloudinarySettings);

        // Act
        services.AddCoreModule();
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Assert
        var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
        httpClientFactory.Should().NotBeNull();

        HttpClient httpClient = httpClientFactory!.CreateClient(nameof(FileService));
        httpClient.Should().NotBeNull();
    }

    [Fact]
    public void AddCoreModule_ShouldRegisterAllServices()
    {
        // Arrange & Act
        _services.AddSingleton(_cloudinarySettings);
        IServiceCollection result = _services.AddCoreModule();

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
