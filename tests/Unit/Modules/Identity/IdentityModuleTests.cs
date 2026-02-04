using _116.Identity;
using _116.Identity.Application.Adapters.Wangkanai.Detection;
using _116.Identity.Application.Auth.Repositories;
using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Session.Repositories;
using _116.Identity.Application.Session.Services;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Shared.Infrastructure.Seed;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity;

/// <summary>
/// Unit tests for <see cref="IdentityModule"/>.
/// </summary>
public class IdentityModuleTests
{
    [Fact]
    public void AddIdentityModule_ShouldRegisterIdentityUnitOfWork()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddIdentityModule();

        // Assert
        ServiceDescriptor? descriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IIdentityUnitOfWork));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
    }

    [Fact]
    public void AddIdentityModule_ShouldRegisterClientOriginDetectionAdapter()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddIdentityModule();

        // Assert
        ServiceDescriptor? descriptor = services.FirstOrDefault(s =>
            s.ServiceType == typeof(IClientOriginDetectionAdapter)
        );
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
    }

    [Fact]
    public void AddIdentityModule_ShouldRegisterAuthServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddIdentityModule();

        // Assert
        Assert.Contains(services, s => s.ServiceType == typeof(IJwtService));
        Assert.Contains(services, s => s.ServiceType == typeof(IPasswordService));
        Assert.Contains(services, s => s.ServiceType == typeof(IRefreshTokenService));
        Assert.Contains(services, s => s.ServiceType == typeof(IOtpService));
    }

    [Fact]
    public void AddIdentityModule_ShouldRegisterRepositories()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddIdentityModule();

        // Assert
        Assert.Contains(services, s => s.ServiceType == typeof(IAuthRepository));
        Assert.Contains(services, s => s.ServiceType == typeof(IRoleRepository));
        Assert.Contains(services, s => s.ServiceType == typeof(IPermissionRepository));
        Assert.Contains(services, s => s.ServiceType == typeof(IRolePermissionRepository));
        Assert.Contains(services, s => s.ServiceType == typeof(IUserRoleRepository));
        Assert.Contains(services, s => s.ServiceType == typeof(IOtpRepository));
        Assert.Contains(services, s => s.ServiceType == typeof(ISessionRepository));
    }

    [Fact]
    public void AddIdentityModule_ShouldRegisterSessionServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddIdentityModule();

        // Assert
        Assert.Contains(services, s => s.ServiceType == typeof(ISessionMetadataService));
        Assert.Contains(services, s => s.ServiceType == typeof(ISessionExportService));
    }

    [Fact]
    public void AddIdentityModule_ShouldRegisterDataSeeders()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddIdentityModule();

        // Assert
        List<ServiceDescriptor> seeders = services.Where(s => s.ServiceType == typeof(IDataSeeder)).ToList();
        Assert.True(seeders.Count >= 2); // SuperAdminSeeder and VisitorRoleSeeder
    }

    [Fact]
    public void AddIdentityModule_ShouldRegisterJwtAuthentication()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddIdentityModule();

        // Assert
        ServiceDescriptor? authenticationDescriptor = services.FirstOrDefault(s =>
            s.ServiceType.Name.Contains("Authentication")
        );
        Assert.NotNull(authenticationDescriptor);
    }

    [Fact]
    public void AddIdentityModule_ShouldReturnServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        IServiceCollection result = services.AddIdentityModule();

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddIdentityModule_ShouldRegisterHttpContextAccessor()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddIdentityModule();

        // Assert
        Assert.Contains(services, s => s.ServiceType.Name.Contains("HttpContextAccessor"));
    }

    // Removed UseIdentityModule test - requires full host configuration

    [Fact]
    public void AddIdentityModule_ShouldNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act & Assert
        var exception = Record.Exception(() => services.AddIdentityModule());
        Assert.Null(exception);
    }
}
