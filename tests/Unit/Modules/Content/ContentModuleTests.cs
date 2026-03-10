using _116.Content;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Infrastructure.Persistence;
using _116.Content.Infrastructure.Persistence.Seeds.ContentTypes;
using _116.Content.Infrastructure.Repositories;
using AwesomeAssertions;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content;

/// <summary>
/// Unit tests for <see cref="ContentModule"/>.
/// </summary>
public class ContentModuleTests : IDisposable
{
    private readonly ServiceCollection _services;

    public ContentModuleTests()
    {
        _services = [];
        _services.AddLogging();
        _services.AddDbContext<ContentDbContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
    }

    public void Dispose()
    {
        // Cleanup if needed
    }

    [Fact]
    public void AddContentModule_ShouldRegisterContentUnitOfWork()
    {
        // Act
        _services.AddContentModule();
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
        _services.AddContentModule();
        ServiceProvider serviceProvider = _services.BuildServiceProvider();

        // Assert
        var repository = serviceProvider.GetService<ILookupRepository>();
        repository.Should().NotBeNull();
        repository.Should().BeOfType<LookupRepository>();
    }

    [Fact]
    public void AddContentModule_ShouldRegisterCategoryRepository()
    {
        // Act
        _services.AddContentModule();
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
        _services.AddContentModule();
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
        _services.AddContentModule();
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
        _services.AddContentModule();
        ServiceProvider serviceProvider = _services.BuildServiceProvider();

        // Assert
        var seeder = serviceProvider.GetService<ContentTypeSeeder>();
        seeder.Should().NotBeNull();
    }

    [Fact]
    public void AddContentModule_ShouldRegisterMapperConfiguration()
    {
        // Act
        _services.AddContentModule();
        ServiceProvider serviceProvider = _services.BuildServiceProvider();

        // Assert
        var config = serviceProvider.GetService<TypeAdapterConfig>();
        config.Should().NotBeNull();
    }

    [Fact]
    public void AddContentModule_ShouldRegisterMapper()
    {
        // Act
        _services.AddContentModule();
        ServiceProvider serviceProvider = _services.BuildServiceProvider();

        // Assert
        var mapper = serviceProvider.GetService<IMapper>();
        mapper.Should().NotBeNull();
    }

    [Fact]
    public void AddContentModule_ShouldReturnServiceCollection()
    {
        // Act
        IServiceCollection result = _services.AddContentModule();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(_services);
    }

    [Fact]
    public void AddContentModule_ShouldRegisterAllServices()
    {
        // Arrange & Act
        IServiceCollection result = _services.AddContentModule();
        ServiceProvider serviceProvider = _services.BuildServiceProvider();

        // Assert
        serviceProvider.GetService<ContentDbContext>().Should().NotBeNull();
        serviceProvider.GetService<IContentUnitOfWork>().Should().NotBeNull();
        serviceProvider.GetService<ILookupRepository>().Should().NotBeNull();
        serviceProvider.GetService<ICategoryRepository>().Should().NotBeNull();
        serviceProvider.GetService<ICustomerRepository>().Should().NotBeNull();
        serviceProvider.GetService<IPackageRepository>().Should().NotBeNull();
        serviceProvider.GetService<ContentTypeSeeder>().Should().NotBeNull();
        result.Should().BeSameAs(_services);
    }

    [Fact]
    public void AddContentModule_WithTestingEnvironment_ShouldDisableSeedingButStillRegisterServices()
    {
        // Arrange — set environment to Testing to hit the enableSeeding=false branch
        string? previousEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");

        try
        {
            // Act
            _services.AddContentModule();
            ServiceProvider serviceProvider = _services.BuildServiceProvider();

            // Assert — all services still registered regardless of seeding flag
            serviceProvider.GetService<IContentUnitOfWork>().Should().NotBeNull();
            serviceProvider.GetService<ILookupRepository>().Should().NotBeNull();
            serviceProvider.GetService<ContentTypeSeeder>().Should().NotBeNull();
        }
        finally
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", previousEnv);
        }
    }

    [Fact]
    public void UseContentModule_WithTestingEnvironment_ShouldReturnAppBuilderEarly()
    {
        // Arrange — Testing env sets EnableMigrations=false and EnableSeeding=false, so
        // UseModuleDatabase is a no-op and the method returns app at the EnableSeeding guard.
        string? previousEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");

        try
        {
            var appBuilderMock = new Mock<IApplicationBuilder>();

            // Act
            IApplicationBuilder result = appBuilderMock.Object.UseContentModule();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeSameAs(appBuilderMock.Object);
        }
        finally
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", previousEnv);
        }
    }
}
