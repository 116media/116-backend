using Mapster;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace _116.Shared.Infrastructure;

/// <summary>
/// Builds and compiles the merged Mapster config when the host starts, so an invalid mapping
/// fails the boot instead of the first request.
/// </summary>
/// <param name="serviceProvider">The application service provider.</param>
internal sealed class ModuleMappingsCompiler(IServiceProvider serviceProvider) : IHostedService
{
    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        serviceProvider.GetRequiredService<TypeAdapterConfig>();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
