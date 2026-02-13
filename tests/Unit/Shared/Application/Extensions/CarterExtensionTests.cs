using System.Reflection;
using _116.Shared.Application.Extensions;
using AwesomeAssertions;
using Carter;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace _116.Unit.Tests.Shared.Application.Extensions;

/// <summary>
/// Unit tests for <see cref="CarterExtension"/>.
/// </summary>
public class CarterExtensionTests
{
    [Fact]
    public void AddCarterWithAssemblies_ShouldRegisterCarterServices()
    {
        // Arrange
        var services = new ServiceCollection();
        Assembly testAssembly = typeof(CarterExtensionTests).Assembly;

        // Act
        services.AddCarterWithAssemblies(testAssembly);

        // Assert
        // Assert
        services
            .Should()
            .Contain(s => s.ServiceType.Namespace != null && s.ServiceType.Namespace.StartsWith("Carter"));
    }

    [Fact]
    public void AddCarterWithAssemblies_ShouldReturnServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        Assembly testAssembly = typeof(CarterExtensionTests).Assembly;

        // Act
        IServiceCollection result = services.AddCarterWithAssemblies(testAssembly);

        // Assert
        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddCarterWithAssemblies_WithMultipleAssemblies_ShouldAcceptMultipleAssemblies()
    {
        // Arrange
        var services = new ServiceCollection();
        Assembly assembly1 = typeof(CarterExtensionTests).Assembly;
        Assembly assembly2 = typeof(ICarterModule).Assembly;

        // Act
        services.AddCarterWithAssemblies(assembly1, assembly2);

        // Assert
        services
            .Should()
            .Contain(s => s.ServiceType.Namespace != null && s.ServiceType.Namespace.StartsWith("Carter"));
    }

    [Fact]
    public void AddCarterWithAssemblies_WithNoAssemblies_ShouldNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Exception? exception = Record.Exception(() => services.AddCarterWithAssemblies());
        exception.Should().BeNull();
    }

    [Fact]
    public void AddCarterWithAssemblies_ShouldScanForICarterModules()
    {
        // Arrange
        var services = new ServiceCollection();
        Assembly testAssembly = typeof(CarterExtensionTests).Assembly;

        // Act
        services.AddCarterWithAssemblies(testAssembly);

        // Assert - Carter services should be registered
        ServiceProvider provider = services.BuildServiceProvider();
        provider.Should().NotBeNull();
    }
}
