using _116.Shared.Application.Extensions;
using _116.Shared.Application.Jobs;
using _116.Unit.Tests.Common.Helpers;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using Xunit;

namespace _116.Unit.Tests.Shared.Application.Extensions;

/// <summary>
/// Unit tests for <see cref="QuartzExtension" />: a scheduled job is registered so both the
/// scheduler and the container can build it, and the clustered store points every instance at
/// one shared trigger timeline.
/// </summary>
[Collection("EnvironmentVariable")]
public class QuartzExtensionTests : IDisposable
{
    private readonly TestDatabaseEnvironment _databaseEnvironment = new();

    /// <inheritdoc />
    public void Dispose()
    {
        _databaseEnvironment.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// A job standing in for a module's scheduled work.
    /// </summary>
    private class TestScheduledJob : IScheduledJob
    {
        /// <inheritdoc />
        public Task Execute(IJobExecutionContext context) => Task.CompletedTask;
    }

    [Fact]
    public void AddScheduledJob_ShouldRegisterTheJobForDirectResolution()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddScheduledJob<TestScheduledJob>(cronExpression: "0 0 * * * ?");

        // Assert
        ServiceDescriptor descriptor = services
            .Should()
            .ContainSingle(s => s.ServiceType == typeof(TestScheduledJob))
            .Which;
        descriptor.Lifetime.Should().Be(ServiceLifetime.Transient);
    }

    [Fact]
    public void AddScheduledJob_ShouldRegisterTheHostedSchedulerOnce()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddScheduledJob<TestScheduledJob>(cronExpression: "0 0 * * * ?");

        // Assert
        services.Should().Contain(s => s.ServiceType == typeof(IHostedService));
    }

    [Fact]
    public void AddScheduledJob_CalledTwiceForOneJob_ShouldNotDuplicateTheRegistration()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddScheduledJob<TestScheduledJob>(cronExpression: "0 0 * * * ?");
        services.AddScheduledJob<TestScheduledJob>(cronExpression: "0 30 * * * ?");

        // Assert
        services.Count(s => s.ServiceType == typeof(TestScheduledJob)).Should().Be(1);
    }

    [Fact]
    public void AddScheduledJob_ShouldReturnServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        IServiceCollection result = services.AddScheduledJob<TestScheduledJob>(cronExpression: "0 0 * * * ?");

        // Assert
        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddClusteredQuartzStore_ShouldRegisterTheSchedulerOptions()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddClusteredQuartzStore();

        // Assert
        services.Should().Contain(s => s.ServiceType == typeof(ISchedulerFactory));
    }

    [Fact]
    public void AddClusteredQuartzStore_ShouldReturnServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        IServiceCollection result = services.AddClusteredQuartzStore();

        // Assert
        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddClusteredQuartzStore_ShouldComposeWithAScheduledJob()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddClusteredQuartzStore();
        services.AddScheduledJob<TestScheduledJob>(cronExpression: "0 0 * * * ?");

        // Assert
        services.Should().Contain(s => s.ServiceType == typeof(TestScheduledJob));
        services.Should().Contain(s => s.ServiceType == typeof(ISchedulerFactory));
    }
}
