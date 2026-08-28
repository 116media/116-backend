using _116.Shared.Application.Exceptions.Messages;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace _116.Tests.Fixtures.Helpers;

/// <summary>
/// Shared test helpers for HTTP context and request/response tests.
/// </summary>
public static class HttpTestHelpers
{
    /// <summary>
    /// Creates a default HttpContext for testing exception handlers and middleware.
    /// Includes a service provider with localization, shared exception messages, and an
    /// <see cref="IHostEnvironment"/> reporting <paramref name="environmentName"/> (the exception
    /// fallback resolves it to decide whether to expose the raw error detail). Defaults to Development.
    /// </summary>
    /// <param name="environmentName">
    /// The hosting environment the resolved <see cref="IHostEnvironment"/> reports. Pass
    /// <see cref="Environments.Production"/> to exercise the sanitized, non-Development branch.
    /// </param>
    public static DefaultHttpContext CreateDefaultHttpContext(string environmentName = "Development")
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLocalization();
        services.AddScoped<SharedExceptionMessage>();
        services.AddSingleton<IHostEnvironment>(new TestHostEnvironment { EnvironmentName = environmentName });
        ServiceProvider provider = services.BuildServiceProvider();

        DefaultHttpContext context = new();
        context.Request.Path = "/api/test";
        context.Request.Method = "GET";
        context.TraceIdentifier = "test-trace-id";
        context.Response.Body = new MemoryStream();
        context.RequestServices = provider;
        return context;
    }

    /// <summary>
    /// Minimal <see cref="IHostEnvironment"/> reporting the Development environment for tests.
    /// </summary>
    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "Tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = null!;
    }
}
