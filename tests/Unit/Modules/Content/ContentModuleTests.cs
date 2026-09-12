using _116.Content;
using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Application.Shared.Services;
using _116.Content.Infrastructure.Persistence;
using _116.Content.Infrastructure.Persistence.Seeds.ContentTypes;
using _116.Content.Infrastructure.Repositories;
using _116.Content.Infrastructure.Services;
using _116.Shared.Infrastructure.Seed;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Helpers;
using AwesomeAssertions;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content;

/// <summary>
/// Unit tests for <see cref="ContentModule"/>.
/// </summary>
public class ContentModuleTests : IDisposable
{
    private readonly ServiceCollection _services;
    private readonly TestDatabaseEnvironment _environment = new();

    public ContentModuleTests()
    {
        _services = [];
        _services.AddLogging();
        _services.AddLocalization();
        _services.AddDbContext<ContentDbContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
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
    public void AddContentModule_ShouldRegisterContentUnitOfWork()
    {
        // Act
        _services.AddContentModule(HostEnvironment("Testing"));
        ServiceProvider serviceProvider = _services.BuildServiceProvider();

        // Assert
        var unitOfWork = serviceProvider.GetService<IContentUnitOfWork>();
        unitOfWork.Should().NotBeNull();
        unitOfWork.Should().BeOfType<ContentUnitOfWork>();
    }

    [Fact]
    public void AddContentModule_ShouldRegisterLookupRepository()
    {
        // Act
        _services.AddContentModule(HostEnvironment("Testing"));
        ServiceProvider serviceProvider = _services.BuildServiceProvider();

        // Assert
        serviceProvider.GetService<IContentTypeRepository>().Should().BeOfType<ContentTypeRepository>();
        serviceProvider.GetService<IPricingTierRepository>().Should().BeOfType<PricingTierRepository>();
        serviceProvider.GetService<IPromotionLevelRepository>().Should().BeOfType<PromotionLevelRepository>();
        serviceProvider.GetService<ITagRepository>().Should().BeOfType<TagRepository>();
    }

    [Fact]
    public void AddContentModule_ShouldRegisterCategoryRepository()
    {
        // Act
        _services.AddContentModule(HostEnvironment("Testing"));
        ServiceProvider serviceProvider = _services.BuildServiceProvider();

        // Assert
        var repository = serviceProvider.GetService<ICategoryRepository>();
        repository.Should().NotBeNull();
        repository.Should().BeOfType<CategoryRepository>();
    }

    [Fact]
    public void AddContentModule_ShouldRegisterCustomerRepository()
    {
        // Act
        _services.AddContentModule(HostEnvironment("Testing"));
        ServiceProvider serviceProvider = _services.BuildServiceProvider();

        // Assert
        var repository = serviceProvider.GetService<ICustomerRepository>();
        repository.Should().NotBeNull();
        repository.Should().BeOfType<CustomerRepository>();
    }

    [Fact]
    public void AddContentModule_ShouldRegisterPackageRepository()
    {
        // Act
        _services.AddContentModule(HostEnvironment("Testing"));
        ServiceProvider serviceProvider = _services.BuildServiceProvider();

        // Assert
        var repository = serviceProvider.GetService<IPackageRepository>();
        repository.Should().NotBeNull();
        repository.Should().BeOfType<PackageRepository>();
    }

    [Fact]
    public void AddContentModule_ShouldRegisterContentTypeSeeder()
    {
        // Act
        _services.AddContentModule(HostEnvironment("Testing"));
        ServiceProvider serviceProvider = _services.BuildServiceProvider();

        // Assert
        var seeder = serviceProvider.GetService<ContentTypeSeeder>();
        seeder.Should().NotBeNull();
    }

    [Fact]
    public void AddContentModule_ShouldRegisterMapperConfiguration()
    {
        // Act
        _services.AddContentModule(HostEnvironment("Testing"));
        ServiceProvider serviceProvider = _services.BuildServiceProvider();

        // Assert
        var config = serviceProvider.GetService<TypeAdapterConfig>();
        config.Should().NotBeNull();
    }

    [Fact]
    public void AddContentModule_ShouldRegisterMapper()
    {
        // Act
        _services.AddContentModule(HostEnvironment("Testing"));
        ServiceProvider serviceProvider = _services.BuildServiceProvider();

        // Assert
        var mapper = serviceProvider.GetService<IMapper>();
        mapper.Should().NotBeNull();
    }

    [Fact]
    public void AddContentModule_ShouldReturnServiceCollection()
    {
        // Act
        IServiceCollection result = _services.AddContentModule(HostEnvironment("Testing"));

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(_services);
    }

    [Fact]
    public void AddContentModule_ShouldRegisterAllServices()
    {
        // Arrange & Act
        IServiceCollection result = _services.AddContentModule(HostEnvironment("Testing"));
        ServiceProvider serviceProvider = _services.BuildServiceProvider();

        // Assert
        serviceProvider.GetService<ContentDbContext>().Should().NotBeNull();
        serviceProvider.GetService<IContentUnitOfWork>().Should().NotBeNull();
        serviceProvider.GetService<ITagRepository>().Should().NotBeNull();
        serviceProvider.GetService<ICategoryRepository>().Should().NotBeNull();
        serviceProvider.GetService<ICustomerRepository>().Should().NotBeNull();
        serviceProvider.GetService<IPackageRepository>().Should().NotBeNull();
        serviceProvider.GetService<ContentTypeSeeder>().Should().NotBeNull();
        result.Should().BeSameAs(_services);
    }

    [Fact]
    public void AddContentModule_WithTestingEnvironment_ShouldDisableSeedingButStillRegisterServices()
    {
        _services.AddContentModule(HostEnvironment("Testing"));
        ServiceProvider serviceProvider = _services.BuildServiceProvider();

        serviceProvider.GetService<IContentUnitOfWork>().Should().NotBeNull();
        serviceProvider.GetService<ITagRepository>().Should().NotBeNull();
        serviceProvider.GetService<ContentTypeSeeder>().Should().NotBeNull();
    }

    [Fact]
    public void AddContentModule_ShouldRegisterStreamingLinkErrors()
    {
        // Act
        _services.AddContentModule(HostEnvironment("Testing"));
        ServiceProvider serviceProvider = _services.BuildServiceProvider();

        // Assert
        serviceProvider.GetService<StreamingLinkErrors>().Should().NotBeNull();
        serviceProvider.GetService<StreamingLinkErrorMessage>().Should().NotBeNull();
    }

    [Fact]
    public void AddContentModule_ShouldRegisterOdesliResolutionServiceWithItsTypedClient()
    {
        // Arrange
        _services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        // Act — resolving the port builds the typed HttpClient, which runs the timeout
        // configuration the registration declares.
        _services.AddContentModule(HostEnvironment("Testing"));
        ServiceProvider serviceProvider = _services.BuildServiceProvider();
        var service = serviceProvider.GetService<IStreamingLinkResolutionService>();

        // Assert
        service.Should().NotBeNull();
        service.Should().BeOfType<OdesliStreamingLinkResolutionService>();
    }

    [Fact]
    public void AddContentModule_WithTestingEnvironment_ShouldRegisterNoDataSeeder()
    {
        // Arrange & Act — Testing hosts seed through the test harness, never the hosted service
        _services.AddContentModule(HostEnvironment("Testing"));

        // Assert
        _services.Should().NotContain(descriptor => descriptor.ServiceType == typeof(IDataSeeder));
    }

    [Fact]
    public void AddContentModule_OutsideTheTestingEnvironment_ShouldRegisterTheContentTypeSeeder()
    {
        // Arrange & Act
        _services.AddContentModule(HostEnvironment("Development"));

        // Assert
        _services.Should().Contain(descriptor => descriptor.ServiceType == typeof(IDataSeeder));
    }
}
