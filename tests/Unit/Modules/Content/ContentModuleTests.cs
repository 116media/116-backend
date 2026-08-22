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
using _116.Unit.Tests.Common;
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

    public ContentModuleTests()
    {
        _services = [];
        _services.AddLogging();
        _services.AddLocalization();
        _services.AddDbContext<ContentDbContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
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
        var repository = serviceProvider.GetService<ILookupRepository>();
        repository.Should().NotBeNull();
        repository.Should().BeOfType<LookupRepository>();
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
        _services.AddContentModule(HostEnvironment("Testing"));
        ServiceProvider serviceProvider = _services.BuildServiceProvider();

        serviceProvider.GetService<IContentUnitOfWork>().Should().NotBeNull();
        serviceProvider.GetService<ILookupRepository>().Should().NotBeNull();
        serviceProvider.GetService<ContentTypeSeeder>().Should().NotBeNull();
    }

    [Fact]
    public void UseContentModule_WithTestingEnvironment_ShouldReturnAppBuilderEarly()
    {
        // Arrange — Testing env sets EnableMigrations=false and EnableSeeding=false, so
        // UseModuleDatabase is a no-op and the method returns app at the EnableSeeding guard.
        var services = new ServiceCollection();
        services.AddSingleton<IHostEnvironment>(HostEnvironment("Testing"));

        var appBuilderMock = new Mock<IApplicationBuilder>();
        appBuilderMock.Setup(builder => builder.ApplicationServices).Returns(services.BuildServiceProvider());

        // Act
        IApplicationBuilder result = appBuilderMock.Object.UseContentModule();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(appBuilderMock.Object);
    }

    [Fact]
    public void UseContentModule_OutsideTheTestingEnvironment_ShouldSeedTheContentTypes()
    {
        // Arrange — Development enables migrations and seeding; the migrator is
        // replaced so the startup migration completes without a database, and
        // the seeder is bound to an in-memory store it can write to.
        DbContextOptions<ContentDbContext> seedOptions = new DbContextOptionsBuilder<ContentDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var seedContext = new ContentDbContext(seedOptions);

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLocalization();
        services.AddDbContext<ContentDbContext>(options =>
            options
                .UseNpgsql("Host=localhost;Port=5432;Database=unit;Username=unit;Password=unit")
                .ReplaceService<IMigrator, NoOpMigrator>()
        );
        services.AddSingleton<IHostEnvironment>(HostEnvironment("Development"));
        services.AddContentModule(HostEnvironment("Development"));
        services.AddScoped(serviceProvider => new ContentTypeSeeder(
            seedContext,
            serviceProvider.GetRequiredService<ILogger<ContentTypeSeeder>>()
        ));

        ServiceProvider provider = services.BuildServiceProvider();
        var app = new ApplicationBuilder(provider);

        // Act
        IApplicationBuilder result = app.UseContentModule();

        // Assert
        result.Should().BeSameAs(app);
        seedContext.ContentTypes.Select(contentType => contentType.Name).Should().Contain("Article");
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
        // Arrange — the odesli adapter reads configuration, which the module does not register.
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
}
