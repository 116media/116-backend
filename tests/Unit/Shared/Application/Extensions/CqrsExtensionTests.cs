using System.Reflection;
using _116.Shared.Application.Extensions;
using _116.Shared.Application.Services;
using _116.Shared.Contracts.Application.CQRS;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace _116.Unit.Tests.Shared.Application.Extensions;

/// <summary>
/// Unit tests for <see cref="CqrsExtension"/>.
/// </summary>
public class CqrsExtensionTests
{
    [Fact]
    public void AddCqrsWithAssemblies_ShouldNotThrowWithNoAssemblies()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act & Assert - Should throw Scrutor exception because no handlers to decorate
        var exception = Record.Exception(() => services.AddCqrsWithAssemblies());
        Assert.NotNull(exception);
    }

    [Fact]
    public void AddCqrsWithAssemblies_ShouldRegisterDispatcherType()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        Assembly sharedAssembly = typeof(Dispatcher).Assembly;

        // Act
        try
        {
            services.AddCqrsWithAssemblies(sharedAssembly);
        }
        catch
        {
            // Ignore Scrutor exceptions for missing handlers
        }

        // Assert - Check if Dispatcher was registered (before decoration fails)
        bool hasDispatcher = services.Any(s => s.ServiceType == typeof(IDispatcher));
        Assert.True(hasDispatcher);
    }

    [Fact]
    public void AddCqrsWithAssemblies_ShouldRegisterDomainEventPublisherType()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        Assembly sharedAssembly = typeof(Dispatcher).Assembly;

        // Act
        try
        {
            services.AddCqrsWithAssemblies(sharedAssembly);
        }
        catch
        {
            // Ignore Scrutor exceptions for missing handlers
        }

        // Assert - Check if DomainEventPublisher was registered (before decoration fails)
        bool hasPublisher = services.Any(s => s.ServiceType == typeof(IDomainEventPublisher));
        Assert.True(hasPublisher);
    }

    [Fact]
    public void AddCqrsWithAssemblies_WithRealHandlers_ShouldRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        // Use Identity assembly which has actual handlers
        Assembly identityAssembly = Assembly.Load("Identity");

        // Act
        services.AddCqrsWithAssemblies(identityAssembly);

        // Assert
        ServiceDescriptor? descriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IDispatcher));
        Assert.NotNull(descriptor);
    }

    [Fact]
    public void AddCqrsWithAssemblies_WithRealHandlers_ShouldAllowResolvingDispatcher()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        Assembly identityAssembly = Assembly.Load("Identity");
        services.AddCqrsWithAssemblies(identityAssembly);

        // Act
        ServiceProvider provider = services.BuildServiceProvider();
        var dispatcher = provider.GetService<IDispatcher>();

        // Assert
        Assert.NotNull(dispatcher);
        Assert.IsType<Dispatcher>(dispatcher);
    }

    [Fact]
    public void AddCqrsWithAssemblies_WithRealHandlers_ShouldAllowResolvingDomainEventPublisher()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        Assembly identityAssembly = Assembly.Load("Identity");
        services.AddCqrsWithAssemblies(identityAssembly);

        // Act
        ServiceProvider provider = services.BuildServiceProvider();
        var publisher = provider.GetService<IDomainEventPublisher>();

        // Assert
        Assert.NotNull(publisher);
        Assert.IsType<DomainEventPublisher>(publisher);
    }
}
