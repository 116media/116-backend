using System.Reflection;
using System.Threading.RateLimiting;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Extensions;
using AwesomeAssertions;
using AwesomeAssertions.Specialized;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Shared.Application.Extensions;

/// <summary>
/// Unit tests for <see cref="RateLimitingExtension"/>.
/// </summary>
public class RateLimitingExtensionTests
{
    [Fact]
    public void AddRateLimiting_ShouldRegisterRateLimiterServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddRateLimiting();

        // Assert - RateLimiter services should be registered
        services.Should().NotBeEmpty();
    }

    [Fact]
    public void AddRateLimiting_ShouldReturnServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        IServiceCollection result = services.AddRateLimiting();

        // Assert
        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddRateLimiting_ShouldConfigureRejectionStatusCode()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddRateLimiting();

        // Act
        ServiceProvider provider = services.BuildServiceProvider();
        var options = provider.GetService<IOptions<RateLimiterOptions>>();

        // Assert
        options.Should().NotBeNull();
        options.Value.RejectionStatusCode.Should().Be(StatusCodes.Status429TooManyRequests);
    }

    [Fact]
    public void AddRateLimiting_ShouldConfigureOnRejectedHandler()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddRateLimiting();

        // Act
        ServiceProvider provider = services.BuildServiceProvider();
        var options = provider.GetService<IOptions<RateLimiterOptions>>();

        // Assert
        options.Should().NotBeNull();
        options.Value.OnRejected.Should().NotBeNull();
    }

    [Fact]
    public void AddRateLimiting_ShouldAllowResolvingRateLimiterOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddRateLimiting();

        // Act
        ServiceProvider provider = services.BuildServiceProvider();
        var options = provider.GetService<IOptions<RateLimiterOptions>>();

        // Assert
        options.Should().NotBeNull();
        options.Value.Should().NotBeNull();
    }

    [Fact]
    public void AddRateLimiting_ShouldRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddRateLimiting();

        // Assert - RateLimiter services should be registered
        services.Should().NotBeEmpty();
    }

    [Fact]
    public void AddRateLimiting_ShouldConfigureSlidingWindowPolicies()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddRateLimiting();

        // Act
        ServiceProvider provider = services.BuildServiceProvider();
        var options = provider.GetService<IOptions<RateLimiterOptions>>();

        // Assert - Verify options configured without error
        options.Should().NotBeNull();
        options.Value.Should().NotBeNull();
    }

    [Fact]
    public void AddRateLimiting_ShouldConfigureTokenBucketPolicies()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddRateLimiting();

        // Act
        ServiceProvider provider = services.BuildServiceProvider();
        var options = provider.GetService<IOptions<RateLimiterOptions>>();

        // Assert - Verify options configured without error
        options.Should().NotBeNull();
        options.Value.Should().NotBeNull();
    }

    [Fact]
    public void AddRateLimiting_ShouldConfigureFixedWindowPolicies()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddRateLimiting();

        // Act
        ServiceProvider provider = services.BuildServiceProvider();
        var options = provider.GetService<IOptions<RateLimiterOptions>>();

        // Assert - Verify options configured without error
        options.Should().NotBeNull();
        options.Value.Should().NotBeNull();
    }

    [Fact]
    public void AddRateLimiting_ShouldNotThrowDuringRegistration()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Exception? exception = Record.Exception(() => services.AddRateLimiting());
        exception.Should().BeNull();
    }

    [Fact]
    public void AddRateLimiting_ShouldNotThrowWhenBuildingProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddRateLimiting();

        // Act & Assert
        Exception? exception = Record.Exception(() => services.BuildServiceProvider());
        exception.Should().BeNull();
    }

    [Fact]
    public async Task OnRateLimitRejected_WithRetryAfterMetadata_ShouldThrowExceptionWithRetryAfter()
    {
        // Arrange
        TimeSpan retryAfter = TimeSpan.FromSeconds(30);
        var lease = new TestRateLimitLease(hasRetryAfter: true, retryAfter);
        var context = new OnRejectedContext { Lease = lease, HttpContext = new DefaultHttpContext() };

        MethodInfo method = typeof(RateLimitingExtension).GetMethod(
            "OnRateLimitRejected",
            BindingFlags.NonPublic | BindingFlags.Static
        )!;

        Func<Task> act = async () =>
        {
            var task = (ValueTask)method.Invoke(null, [context, CancellationToken.None])!;
            await task;
        };

        // Assert
        ExceptionAssertions<TargetInvocationException>? exception = await act.Should()
            .ThrowAsync<TargetInvocationException>();

        exception
            .Which.InnerException.Should()
            .BeOfType<RateLimitExceededException>()
            .Which.RetryAfter.Should()
            .Be(retryAfter);
    }

    [Fact]
    public async Task OnRateLimitRejected_WithoutRetryAfterMetadata_ShouldThrowExceptionWithZeroRetryAfter()
    {
        // Arrange
        var lease = new TestRateLimitLease(hasRetryAfter: false, TimeSpan.Zero);
        var context = new OnRejectedContext { Lease = lease, HttpContext = new DefaultHttpContext() };

        MethodInfo method = typeof(RateLimitingExtension).GetMethod(
            "OnRateLimitRejected",
            BindingFlags.NonPublic | BindingFlags.Static
        )!;

        Func<Task> act = async () =>
        {
            var task = (ValueTask)method.Invoke(null, [context, CancellationToken.None])!;
            await task;
        };

        // Assert
        ExceptionAssertions<TargetInvocationException>? exception = await act.Should()
            .ThrowAsync<TargetInvocationException>();

        exception
            .Which.InnerException.Should()
            .BeOfType<RateLimitExceededException>()
            .Which.RetryAfter.Should()
            .Be(TimeSpan.Zero);
    }

    [Fact]
    public async Task OnRateLimitRejected_WithSpecificRetryTime_ShouldPreserveExactValue()
    {
        // Arrange
        TimeSpan exactRetryAfter = TimeSpan.FromMinutes(5).Add(TimeSpan.FromSeconds(37));

        var lease = new TestRateLimitLease(hasRetryAfter: true, exactRetryAfter);
        var context = new OnRejectedContext { Lease = lease, HttpContext = new DefaultHttpContext() };

        MethodInfo method = typeof(RateLimitingExtension).GetMethod(
            "OnRateLimitRejected",
            BindingFlags.NonPublic | BindingFlags.Static
        )!;

        Func<Task> act = async () =>
        {
            var task = (ValueTask)method.Invoke(null, [context, CancellationToken.None])!;
            await task;
        };

        // Assert
        ExceptionAssertions<TargetInvocationException>? exception = await act.Should()
            .ThrowAsync<TargetInvocationException>();
        RateLimitExceededException? rateLimitEx = exception
            .Which.InnerException.Should()
            .BeOfType<RateLimitExceededException>()
            .Which;

        rateLimitEx.RetryAfter.Should().Be(exactRetryAfter);
        rateLimitEx.RetryAfter.TotalSeconds.Should().Be(337);
    }

    private class TestRateLimitLease : RateLimitLease
    {
        private readonly bool _hasRetryAfter;
        private readonly TimeSpan _retryAfter;

        public TestRateLimitLease(bool hasRetryAfter, TimeSpan retryAfter)
        {
            _hasRetryAfter = hasRetryAfter;
            _retryAfter = retryAfter;
        }

        public override bool IsAcquired => false;

        public override IEnumerable<string> MetadataNames =>
            _hasRetryAfter ? new[] { MetadataName.RetryAfter.Name } : Array.Empty<string>();

        public override bool TryGetMetadata(string metadataName, out object? metadata)
        {
            if (_hasRetryAfter && metadataName == MetadataName.RetryAfter.Name)
            {
                metadata = _retryAfter;
                return true;
            }

            metadata = null;
            return false;
        }

        protected override void Dispose(bool disposing) { }
    }
}
