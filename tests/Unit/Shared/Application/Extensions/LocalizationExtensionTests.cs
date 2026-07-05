using _116.Shared.Application.Extensions;
using AwesomeAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace _116.Unit.Tests.Shared.Application.Extensions;

/// <summary>
/// Unit tests for <see cref="LocalizationExtension"/>.
/// </summary>
public class LocalizationExtensionTests
{
    [Fact]
    public void AddAppLocalization_ShouldReturnServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        IServiceCollection result = services.AddAppLocalization();

        // Assert
        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddAppLocalization_ShouldRegisterLocalizationServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAppLocalization();

        // Assert
        services.Should().NotBeEmpty();
    }

    [Fact]
    public void AddAppLocalization_ShouldNotThrowDuringRegistration()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Exception? exception = Record.Exception(() => services.AddAppLocalization());
        exception.Should().BeNull();
    }

    [Fact]
    public void AddAppLocalization_ShouldConfigureDefaultCultureToFrench()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddAppLocalization();

        // Act
        ServiceProvider provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<RequestLocalizationOptions>>();

        // Assert
        options.Value.DefaultRequestCulture.Culture.Name.Should().Be("fr");
    }

    [Fact]
    public void AddAppLocalization_ShouldConfigureDefaultUICultureToFrench()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddAppLocalization();

        // Act
        ServiceProvider provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<RequestLocalizationOptions>>();

        // Assert
        options.Value.DefaultRequestCulture.UICulture.Name.Should().Be("fr");
    }

    [Fact]
    public void AddAppLocalization_ShouldIncludeFrenchInSupportedCultures()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddAppLocalization();

        // Act
        ServiceProvider provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<RequestLocalizationOptions>>();
        List<string> cultureNames = options.Value.SupportedCultures!.Select(c => c.Name).ToList();

        // Assert
        cultureNames.Should().Contain("fr");
    }

    [Fact]
    public void AddAppLocalization_ShouldIncludeEnglishInSupportedCultures()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddAppLocalization();

        // Act
        ServiceProvider provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<RequestLocalizationOptions>>();
        List<string> cultureNames = options.Value.SupportedCultures!.Select(c => c.Name).ToList();

        // Assert
        cultureNames.Should().Contain("en");
    }

    [Fact]
    public void AddAppLocalization_ShouldIncludeFrenchInSupportedUICultures()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddAppLocalization();

        // Act
        ServiceProvider provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<RequestLocalizationOptions>>();
        List<string> cultureNames = options.Value.SupportedUICultures!.Select(c => c.Name).ToList();

        // Assert
        cultureNames.Should().Contain("fr");
    }

    [Fact]
    public void AddAppLocalization_ShouldIncludeEnglishInSupportedUICultures()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddAppLocalization();

        // Act
        ServiceProvider provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<RequestLocalizationOptions>>();
        List<string> cultureNames = options.Value.SupportedUICultures!.Select(c => c.Name).ToList();

        // Assert
        cultureNames.Should().Contain("en");
    }

    [Fact]
    public void AddAppLocalization_ShouldHaveExactlyOneCultureProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddAppLocalization();

        // Act
        ServiceProvider provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<RequestLocalizationOptions>>();

        // Assert
        options.Value.RequestCultureProviders.Should().ContainSingle();
    }

    [Fact]
    public void AddAppLocalization_ShouldUseAcceptLanguageHeaderProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddAppLocalization();

        // Act
        ServiceProvider provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<RequestLocalizationOptions>>();

        // Assert
        options.Value.RequestCultureProviders.First().Should().BeOfType<AcceptLanguageHeaderRequestCultureProvider>();
    }

    [Fact]
    public void UseAppLocalization_ShouldReturnApplicationBuilder()
    {
        // Arrange
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.Services.AddAppLocalization();
        WebApplication app = builder.Build();

        // Act
        IApplicationBuilder result = app.UseAppLocalization();

        // Assert
        result.Should().BeSameAs(app);
    }

    [Fact]
    public void UseAppLocalization_ShouldNotThrow()
    {
        // Arrange
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.Services.AddAppLocalization();
        WebApplication app = builder.Build();

        // Act & Assert
        Exception? exception = Record.Exception(() => app.UseAppLocalization());
        exception.Should().BeNull();
    }
}
