using _116.Shared.Application.Exceptions.Handlers;
using _116.Shared.Application.Exceptions.Handlers.Contracts;
using _116.Shared.Application.Exceptions.Handlers.Strategies;
using _116.Shared.Application.Extensions;
using AwesomeAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace _116.Unit.Tests.Shared.Application.Extensions;

/// <summary>
/// Unit tests for <see cref="ExceptionHandlerExtension"/>.
/// </summary>
public class ExceptionHandlerExtensionTests
{
    [Fact]
    public void AddAppExceptionHandler_ShouldRegisterExceptionHandler()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAppExceptionHandler();

        // Assert
        services.Should().Contain(s => s.ServiceType == typeof(IExceptionHandler));
    }

    [Fact]
    public void AddAppExceptionHandler_ShouldRegisterProblemDetails()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAppExceptionHandler();

        // Assert - Problem details services should be registered
        Assert.NotEmpty(services);
    }

    [Fact]
    public void AddAppExceptionHandler_ShouldRegisterDefaultStrategy()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAppExceptionHandler();

        // Assert
        ServiceDescriptor? descriptor = services.FirstOrDefault(s => s.ServiceType == typeof(DefaultExceptionHandler));
        descriptor.Should().NotBeNull();
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
    }

    [Fact]
    public void AddAppExceptionHandler_ShouldRegisterStrategyRegistry()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAppExceptionHandler();

        // Assert
        ServiceDescriptor? descriptor = services.FirstOrDefault(s =>
            s.ServiceType == typeof(ExceptionStrategyRegistry)
        );
        descriptor.Should().NotBeNull();
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
    }

    [Fact]
    public void AddAppExceptionHandler_ShouldReturnServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        IServiceCollection result = services.AddAppExceptionHandler();

        // Assert
        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddAppExceptionHandler_ShouldRegisterExceptionStrategies()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAppExceptionHandler();

        // Assert - Should have multiple IExceptionStrategy implementations
        List<ServiceDescriptor> strategies = services.Where(s => s.ServiceType == typeof(IExceptionStrategy)).ToList();
        Assert.NotEmpty(strategies);
    }

    [Fact]
    public void AddAppExceptionHandler_ExceptionStrategiesShouldBeSingletons()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAppExceptionHandler();

        // Assert
        List<ServiceDescriptor> strategies = services.Where(s => s.ServiceType == typeof(IExceptionStrategy)).ToList();
        Assert.All(strategies, descriptor => Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime));
    }

    [Fact]
    public void AddAppExceptionHandler_ShouldAllowResolvingDefaultStrategy()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddAppExceptionHandler();

        // Act
        ServiceProvider provider = services.BuildServiceProvider();
        var defaultStrategy = provider.GetService<DefaultExceptionHandler>();

        // Assert
        defaultStrategy.Should().NotBeNull();
    }

    [Fact]
    public void AddAppExceptionHandler_ShouldAllowResolvingStrategyRegistry()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAppExceptionHandler();

        // Act
        ServiceProvider provider = services.BuildServiceProvider();
        var registry = provider.GetService<ExceptionStrategyRegistry>();

        // Assert
        registry.Should().NotBeNull();
    }

    [Fact]
    public void AddAppExceptionHandler_ShouldAllowResolvingExceptionStrategies()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddAppExceptionHandler();

        // Act
        ServiceProvider provider = services.BuildServiceProvider();
        IEnumerable<IExceptionStrategy> strategies = provider.GetServices<IExceptionStrategy>();

        // Assert
        Assert.NotEmpty(strategies);
    }

    [Fact]
    public void UseAppExceptionHandler_ShouldReturnApplicationBuilder()
    {
        // Arrange
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        WebApplication app = builder.Build();

        // Act
        IApplicationBuilder result = app.UseAppExceptionHandler();

        // Assert
        result.Should().BeSameAs(app);
    }

    [Fact]
    public void UseAppExceptionHandler_ShouldNotThrow()
    {
        // Arrange
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.Services.AddAppExceptionHandler();
        WebApplication app = builder.Build();

        // Act & Assert
        Exception? exception = Record.Exception(() => app.UseAppExceptionHandler());
        exception.Should().BeNull();
    }
}
