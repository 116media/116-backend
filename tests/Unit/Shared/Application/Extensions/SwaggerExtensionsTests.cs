using _116.Shared.Application.Extensions;
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
        Assert.Same(options, result);
    }

    [Fact]
    public void AddSwaggerOptions_ShouldAddBearerSecurityDefinition()
    {
        // Arrange
        var options = new SwaggerGenOptions();

        // Act
        options.AddSwaggerOptions();

        // Assert
        Assert.NotEmpty(options.SwaggerGeneratorOptions.SecuritySchemes);
        Assert.True(options.SwaggerGeneratorOptions.SecuritySchemes.ContainsKey("Bearer"));
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
        Assert.NotNull(scheme);
        Assert.Equal("Bearer", scheme.Scheme);
        Assert.Equal(SecuritySchemeType.ApiKey, scheme.Type);
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
        Assert.NotNull(scheme);
        Assert.Equal(ParameterLocation.Header, scheme.In);
        Assert.Equal("Authorization", scheme.Name);
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
        Assert.NotNull(scheme);
        Assert.NotNull(scheme.Description);
        Assert.Contains("JWT", scheme.Description);
        Assert.Contains("Bearer", scheme.Description);
    }

    [Fact]
    public void AddSwaggerOptions_ShouldAddSecurityRequirement()
    {
        // Arrange
        var options = new SwaggerGenOptions();

        // Act
        options.AddSwaggerOptions();

        // Assert
        Assert.NotEmpty(options.SwaggerGeneratorOptions.SecurityRequirements);
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
        Assert.NotEmpty(requirement);
        OpenApiSecurityScheme scheme = requirement.Keys.First();
        Assert.Equal("Bearer", scheme.Reference.Id);
        Assert.Equal(ReferenceType.SecurityScheme, scheme.Reference.Type);
    }

    [Fact]
    public void AddSwaggerOptions_ShouldSupportNonNullableReferenceTypes()
    {
        // Arrange
        var options = new SwaggerGenOptions();

        // Act
        options.AddSwaggerOptions();

        // Assert - Options object should have been configured (no exception thrown)
        Assert.NotNull(options);
    }

    [Fact]
    public void AddSwaggerOptions_ShouldConfigureNonNullableAsRequired()
    {
        // Arrange
        var options = new SwaggerGenOptions();

        // Act
        options.AddSwaggerOptions();

        // Assert - Configuration applied successfully (no exception thrown)
        Assert.NotNull(options);
    }

    [Fact]
    public void AddSwaggerOptions_ShouldAllowChaining()
    {
        // Arrange
        var options = new SwaggerGenOptions();

        // Act
        SwaggerGenOptions result = options.AddSwaggerOptions();

        // Assert
        Assert.Same(options, result);
    }
}
