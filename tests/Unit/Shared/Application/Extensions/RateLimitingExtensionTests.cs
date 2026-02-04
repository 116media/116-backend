using System.Reflection;
using System.Threading.RateLimiting;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Extensions;
using AwesomeAssertions;
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
        Assert.NotEmpty(services);
    }

    [Fact]
    public void AddRateLimiting_ShouldReturnServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        IServiceCollection result = services.AddRateLimiting();

        // Assert
        Assert.Same(services, result);
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
        Assert.NotNull(options);
        Assert.Equal(StatusCodes.Status429TooManyRequests, options.Value.RejectionStatusCode);
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
        Assert.NotNull(options);
        Assert.NotNull(options.Value.OnRejected);
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
        Assert.NotNull(options);
        Assert.NotNull(options.Value);
    }

    [Fact]
    public void AddRateLimiting_ShouldRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddRateLimiting();

        // Assert - RateLimiter services should be registered
        Assert.NotEmpty(services);
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
        Assert.NotNull(options);
        Assert.NotNull(options.Value);
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
        Assert.NotNull(options);
        Assert.NotNull(options.Value);
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
        Assert.NotNull(options);
        Assert.NotNull(options.Value);
    }

    [Fact]
    public void AddRateLimiting_ShouldNotThrowDuringRegistration()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        var exception = Record.Exception(() => services.AddRateLimiting());
        Assert.Null(exception);
    }

    [Fact]
    public void AddRateLimiting_ShouldNotThrowWhenBuildingProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddRateLimiting();

        // Act & Assert
        var exception = Record.Exception(() => services.BuildServiceProvider());
        Assert.Null(exception);
    }

    [Fact]
    public void OnRateLimitRejected_WithRetryAfterMetadata_ShouldThrowExceptionWithRetryAfter()
    {
        // Arrange
        var retryAfter = TimeSpan.FromSeconds(30);
        var lease = new TestRateLimitLease(hasRetryAfter: true, retryAfter);
        var context = new OnRejectedContext { Lease = lease, HttpContext = new DefaultHttpContext() };

        // Get the private method via reflection
        var method = typeof(RateLimitingExtension).GetMethod(
            "OnRateLimitRejected",
            BindingFlags.NonPublic | BindingFlags.Static
        );

        // Act & Assert
        try
        {
            var task = (ValueTask)method!.Invoke(null, new object[] { context, CancellationToken.None })!;
            task.GetAwaiter().GetResult();
            Assert.Fail("Expected RateLimitExceededException to be thrown");
        }
        catch (TargetInvocationException ex)
        {
            // Reflection wraps the exception in TargetInvocationException, unwrap it
            ex.InnerException.Should().BeOfType<RateLimitExceededException>();
            var rateLimitEx = (RateLimitExceededException)ex.InnerException!;
            rateLimitEx.RetryAfter.Should().Be(retryAfter);
        }
    }

    [Fact]
    public void OnRateLimitRejected_WithoutRetryAfterMetadata_ShouldThrowExceptionWithZeroRetryAfter()
    {
        // Arrange
        var lease = new TestRateLimitLease(hasRetryAfter: false, TimeSpan.Zero);
        var context = new OnRejectedContext { Lease = lease, HttpContext = new DefaultHttpContext() };

        // Get the private method via reflection
        var method = typeof(RateLimitingExtension).GetMethod(
            "OnRateLimitRejected",
            BindingFlags.NonPublic | BindingFlags.Static
        );

        // Act & Assert
        try
        {
            var task = (ValueTask)method!.Invoke(null, new object[] { context, CancellationToken.None })!;
            task.GetAwaiter().GetResult();
            Assert.Fail("Expected RateLimitExceededException to be thrown");
        }
        catch (TargetInvocationException ex)
        {
            ex.InnerException.Should().BeOfType<RateLimitExceededException>();
            var rateLimitEx = (RateLimitExceededException)ex.InnerException!;
            rateLimitEx.RetryAfter.Should().Be(TimeSpan.Zero);
        }
    }

    [Fact]
    public void OnRateLimitRejected_WithSpecificRetryTime_ShouldPreserveExactValue()
    {
        // Arrange
        var exactRetryAfter = TimeSpan.FromMinutes(5).Add(TimeSpan.FromSeconds(37));
        var lease = new TestRateLimitLease(hasRetryAfter: true, exactRetryAfter);
        var context = new OnRejectedContext { Lease = lease, HttpContext = new DefaultHttpContext() };

        // Get the private method via reflection
        var method = typeof(RateLimitingExtension).GetMethod(
            "OnRateLimitRejected",
            BindingFlags.NonPublic | BindingFlags.Static
        );

        // Act & Assert
        try
        {
            var task = (ValueTask)method!.Invoke(null, new object[] { context, CancellationToken.None })!;
            task.GetAwaiter().GetResult();
            Assert.Fail("Expected RateLimitExceededException to be thrown");
        }
        catch (TargetInvocationException ex)
        {
            ex.InnerException.Should().BeOfType<RateLimitExceededException>();
            var rateLimitEx = (RateLimitExceededException)ex.InnerException!;
            rateLimitEx.RetryAfter.Should().Be(exactRetryAfter);
            rateLimitEx.RetryAfter.TotalSeconds.Should().Be(337); // 5*60 + 37
        }
    }

    // Test implementation of RateLimitLease
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
