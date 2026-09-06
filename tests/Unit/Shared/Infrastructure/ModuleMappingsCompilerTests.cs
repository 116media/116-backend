using _116.Shared.Infrastructure;
using AwesomeAssertions;
using Mapster;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Shared.Infrastructure;

/// <summary>
/// Unit tests for <see cref="ModuleMappingsCompiler"/>.
/// </summary>
public class ModuleMappingsCompilerTests
{
    [Fact]
    public async Task StartAsync_ShouldResolveTheMergedConfig()
    {
        // Arrange
        var config = new TypeAdapterConfig();
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(TypeAdapterConfig))).Returns(config);
        var compiler = new ModuleMappingsCompiler(serviceProviderMock.Object);

        // Act
        await compiler.StartAsync(CancellationToken.None);

        // Assert
        serviceProviderMock.Verify(sp => sp.GetService(typeof(TypeAdapterConfig)), Times.Once);
    }

    [Fact]
    public async Task StartAsync_WhenTheConfigCannotResolve_ShouldFailTheBoot()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(TypeAdapterConfig))).Returns(null!);
        var compiler = new ModuleMappingsCompiler(serviceProviderMock.Object);

        // Act
        Func<Task> starting = () => compiler.StartAsync(CancellationToken.None);

        // Assert
        await starting.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task StopAsync_ShouldCompleteWithoutTouchingTheProvider()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>(MockBehavior.Strict);
        var compiler = new ModuleMappingsCompiler(serviceProviderMock.Object);

        // Act
        await compiler.StopAsync(CancellationToken.None);

        // Assert
        serviceProviderMock.VerifyNoOtherCalls();
    }
}
