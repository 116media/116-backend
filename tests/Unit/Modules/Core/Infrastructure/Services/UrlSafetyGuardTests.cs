using _116.Core.Infrastructure.Services;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace _116.Unit.Tests.Modules.Core.Infrastructure.Services;

/// <summary>
/// Unit tests for <see cref="UrlSafetyGuard"/>. The guard rejects URLs that would make the server
/// dial itself or the private network, and (outside Development) any non-HTTPS scheme.
/// </summary>
public class UrlSafetyGuardTests
{
    private static UrlSafetyGuard Guard(string environmentName = "Production") =>
        new(new TestEnvironment(environmentName), TestErrorsFactory.CreateCoreI18n());

    [Theory]
    [InlineData("https://127.0.0.1/avatar.png")]
    [InlineData("https://10.0.0.5/avatar.png")]
    [InlineData("https://172.16.4.4/avatar.png")]
    [InlineData("https://192.168.1.10/avatar.png")]
    [InlineData("https://169.254.169.254/latest/meta-data")]
    [InlineData("https://224.0.0.1/avatar.png")] // IPv4 multicast
    [InlineData("https://[::1]/avatar.png")] // IPv6 loopback
    [InlineData("https://[fc00::1]/avatar.png")] // IPv6 unique-local
    [InlineData("https://[fe80::1]/avatar.png")] // IPv6 link-local
    [InlineData("https://[ff02::1]/avatar.png")] // IPv6 multicast
    public async Task EnsureSafeAsync_WithPrivateOrLoopbackAddress_Throws(string url)
    {
        // Act
        Func<Task> act = async () => await Guard().EnsureSafeAsync(new Uri(url), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InternalServerException>();
    }

    [Fact]
    public async Task EnsureSafeAsync_WithNonDefaultPort_Throws()
    {
        // Act
        Func<Task> act = async () =>
            await Guard().EnsureSafeAsync(new Uri("https://93.184.216.34:5432/x"), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InternalServerException>();
    }

    [Fact]
    public async Task EnsureSafeAsync_WithHttpOutsideDevelopment_Throws()
    {
        // Act
        Func<Task> act = async () =>
            await Guard(Environments.Production)
                .EnsureSafeAsync(new Uri("http://93.184.216.34/x"), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InternalServerException>();
    }

    [Fact]
    public async Task EnsureSafeAsync_WithPublicHttpsAddress_DoesNotThrow()
    {
        // Act — 93.184.216.34 (example.com) is a routable public address
        Func<Task> act = async () =>
            await Guard().EnsureSafeAsync(new Uri("https://93.184.216.34/avatar.png"), CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task EnsureSafeAsync_WithPublicIPv6Address_DoesNotThrow()
    {
        // Act — a routable public IPv6 literal (example.com) is outside every blocked range
        Func<Task> act = async () =>
            await Guard()
                .EnsureSafeAsync(
                    new Uri("https://[2606:2800:220:1:248:1893:25c8:1946]/avatar.png"),
                    CancellationToken.None
                );

        // Assert
        await act.Should().NotThrowAsync();
    }

    private sealed class TestEnvironment(string environmentName) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;
        public string ApplicationName { get; set; } = "Tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = null!;
    }
}
