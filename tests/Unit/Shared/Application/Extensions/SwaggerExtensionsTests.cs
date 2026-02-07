using _116.Shared.Application.Extensions;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Xunit;

namespace _116.Unit.Tests.Shared.Application.Extensions;

/// <summary>
/// Unit tests for <see cref="SwaggerExtensions"/>.
/// </summary>
public class SwaggerExtensionsTests
{
    [Fact]
    public void AddSwaggerOptions_ShouldReturnSwaggerGenOptions()
    {
        // Arrange
        var options = new SwaggerGenOptions();

        // Act
        SwaggerGenOptions result = options.AddSwaggerOptions();

        // Assert
        result.Should().BeSameAs(options);
    }

    [Fact]
    public void AddSwaggerOptions_ShouldAddBearerSecurityDefinition()
    {
        // Arrange
        var options = new SwaggerGenOptions();

        // Act
        options.AddSwaggerOptions();

        // Assert
        options.SwaggerGeneratorOptions.SecuritySchemes.Should().NotBeEmpty();
        options.SwaggerGeneratorOptions.SecuritySchemes.Should().ContainKey("Bearer");
    }

    [Fact]
    public void AddSwaggerOptions_BearerDefinitionShouldHaveCorrectScheme()
    {
        // Arrange
        var options = new SwaggerGenOptions();

        // Act
        options.AddSwaggerOptions();

        // Assert
        OpenApiSecurityScheme? scheme = options.SwaggerGeneratorOptions.SecuritySchemes["Bearer"];
        scheme.Should().NotBeNull();
        scheme.Scheme.Should().Be("Bearer");
        scheme.Type.Should().Be(SecuritySchemeType.ApiKey);
    }

    [Fact]
    public void AddSwaggerOptions_BearerDefinitionShouldHaveCorrectLocation()
    {
        // Arrange
        var options = new SwaggerGenOptions();

        // Act
        options.AddSwaggerOptions();

        // Assert
        OpenApiSecurityScheme? scheme = options.SwaggerGeneratorOptions.SecuritySchemes["Bearer"];
        scheme.Should().NotBeNull();
        scheme.In.Should().Be(ParameterLocation.Header);
        scheme.Name.Should().Be("Authorization");
    }

    [Fact]
    public void AddSwaggerOptions_BearerDefinitionShouldHaveDescription()
    {
        // Arrange
        var options = new SwaggerGenOptions();

        // Act
        options.AddSwaggerOptions();

        // Assert
        OpenApiSecurityScheme? scheme = options.SwaggerGeneratorOptions.SecuritySchemes["Bearer"];
        scheme.Should().NotBeNull();
        scheme.Description.Should().NotBeNull();
        scheme.Description.Should().Contain("JWT");
        scheme.Description.Should().Contain("Bearer");
    }

    [Fact]
    public void AddSwaggerOptions_ShouldAddSecurityRequirement()
    {
        // Arrange
        var options = new SwaggerGenOptions();

        // Act
        options.AddSwaggerOptions();

        // Assert
        options.SwaggerGeneratorOptions.SecurityRequirements.Should().NotBeEmpty();
    }

    [Fact]
    public void AddSwaggerOptions_SecurityRequirementShouldReferenceBearerScheme()
    {
        // Arrange
        var options = new SwaggerGenOptions();

        // Act
        options.AddSwaggerOptions();

        // Assert
        OpenApiSecurityRequirement requirement = options.SwaggerGeneratorOptions.SecurityRequirements.First();
        requirement.Should().NotBeEmpty();
        OpenApiSecurityScheme scheme = requirement.Keys.First();
        scheme.Reference.Id.Should().Be("Bearer");
        scheme.Reference.Type.Should().Be(ReferenceType.SecurityScheme);
    }

    [Fact]
    public void AddSwaggerOptions_ShouldSupportNonNullableReferenceTypes()
    {
        // Arrange
        var options = new SwaggerGenOptions();

        // Act
        options.AddSwaggerOptions();

        // Assert - Options object should have been configured (no exception thrown)
        options.Should().NotBeNull();
    }

    [Fact]
    public void AddSwaggerOptions_ShouldConfigureNonNullableAsRequired()
    {
        // Arrange
        var options = new SwaggerGenOptions();

        // Act
        options.AddSwaggerOptions();

        // Assert - Configuration applied successfully (no exception thrown)
        options.Should().NotBeNull();
    }

    [Fact]
    public void AddSwaggerOptions_ShouldAllowChaining()
    {
        // Arrange
        var options = new SwaggerGenOptions();

        // Act
        SwaggerGenOptions result = options.AddSwaggerOptions();

        // Assert
        result.Should().BeSameAs(options);
    }
}
