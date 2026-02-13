using _116.Shared.Application.Configurations;
using _116.Shared.Application.Extensions;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace _116.Unit.Tests.Shared.Application.Extensions;

/// <summary>
/// Unit tests for <see cref="CloudinaryExtensions"/>.
/// </summary>
public class CloudinaryExtensionsTests
{
    [Fact]
    public void AddCloudinaryConfiguration_ShouldRegisterCloudinarySettings()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCloudinaryConfiguration();

        // Assert
        ServiceDescriptor? descriptor = services.FirstOrDefault(s => s.ServiceType == typeof(CloudinarySettings));
        descriptor.Should().NotBeNull();
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
    }

    [Fact]
    public void AddCloudinaryConfiguration_ShouldReturnServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        IServiceCollection result = services.AddCloudinaryConfiguration();

        // Assert
        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddCloudinaryConfiguration_ShouldCreateSettingsWithEmptyStringsWhenEnvVarsNull()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCloudinaryConfiguration();
        ServiceProvider provider = services.BuildServiceProvider();
        var settings = provider.GetRequiredService<CloudinarySettings>();

        // Assert
        settings.Should().NotBeNull();
        settings.CloudName.Should().NotBeNull();
        settings.ApiKey.Should().NotBeNull();
        settings.ApiSecret.Should().NotBeNull();
    }

    [Fact]
    public void AddCloudinaryConfiguration_SettingsShouldBeResolvable()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddCloudinaryConfiguration();

        // Act
        ServiceProvider provider = services.BuildServiceProvider();
        var settings = provider.GetService<CloudinarySettings>();

        // Assert
        settings.Should().NotBeNull();
    }

    [Fact]
    public void AddCloudinaryConfiguration_ShouldRegisterAsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddCloudinaryConfiguration();
        ServiceProvider provider = services.BuildServiceProvider();

        // Act
        var settings1 = provider.GetService<CloudinarySettings>();
        var settings2 = provider.GetService<CloudinarySettings>();

        // Assert
        settings2.Should().BeSameAs(settings1);
    }
}
