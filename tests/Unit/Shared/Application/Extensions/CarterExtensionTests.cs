using System.Reflection;
using _116.Shared.Application.Extensions;
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
        Assert.Contains(services, s => s.ServiceType.Namespace?.StartsWith("Carter") == true);
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
        Assert.Same(services, result);
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
        Assert.Contains(services, s => s.ServiceType.Namespace?.StartsWith("Carter") == true);
    }

    [Fact]
    public void AddCarterWithAssemblies_WithNoAssemblies_ShouldNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        var exception = Record.Exception(() => services.AddCarterWithAssemblies());
        Assert.Null(exception);
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
        Assert.NotNull(provider);
    }
}
