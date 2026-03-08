using _116.Identity;
using _116.Identity.Application.Adapters.Wangkanai.Detection;
using _116.Identity.Application.Auth.Repositories;
using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Session.Repositories;
using _116.Identity.Application.Session.Services;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Infrastructure.Persistence.Seeds.SuperAdmin;
using _116.Identity.Infrastructure.Persistence.Seeds.Visitor;
using AwesomeAssertions;
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
        descriptor.Should().NotBeNull();
        descriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);
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
        descriptor.Should().NotBeNull();
        descriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);
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
        services.Should().Contain(s => s.ServiceType == typeof(IJwtService));
        services.Should().Contain(s => s.ServiceType == typeof(IPasswordService));
        services.Should().Contain(s => s.ServiceType == typeof(IRefreshTokenService));
        services.Should().Contain(s => s.ServiceType == typeof(IOtpService));
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
        services.Should().Contain(s => s.ServiceType == typeof(IAuthRepository));
        services.Should().Contain(s => s.ServiceType == typeof(IRoleRepository));
        services.Should().Contain(s => s.ServiceType == typeof(IPermissionRepository));
        services.Should().Contain(s => s.ServiceType == typeof(IRolePermissionRepository));
        services.Should().Contain(s => s.ServiceType == typeof(IUserRoleRepository));
        services.Should().Contain(s => s.ServiceType == typeof(IOtpRepository));
        services.Should().Contain(s => s.ServiceType == typeof(ISessionRepository));
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
        services.Should().Contain(s => s.ServiceType == typeof(ISessionMetadataService));
        services.Should().Contain(s => s.ServiceType == typeof(ISessionExportService));
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
        services.Should().Contain(s => s.ServiceType == typeof(SuperAdminSeeder));
        services.Should().Contain(s => s.ServiceType == typeof(VisitorRoleSeeder));
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
        authenticationDescriptor.Should().NotBeNull();
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
        result.Should().BeSameAs(services);
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
        services.Should().Contain(s => s.ServiceType.Name.Contains("HttpContextAccessor"));
    }

    // Removed UseIdentityModule test - requires full host configuration

    [Fact]
    public void AddIdentityModule_ShouldNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act & Assert
        Exception? exception = Record.Exception(() => services.AddIdentityModule());
        exception.Should().BeNull();
    }
}
