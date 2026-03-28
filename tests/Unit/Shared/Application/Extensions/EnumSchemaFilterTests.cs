using _116.Shared.Application.Extensions;
using AwesomeAssertions;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Xunit;

namespace _116.Unit.Tests.Shared.Application.Extensions;

/// <summary>
/// Unit tests for <see cref="EnumSchemaFilter"/>.
/// </summary>
public class EnumSchemaFilterTests
{
    private readonly EnumSchemaFilter _sut = new();

    private enum TestStatus
    {
        Active,
        Inactive,
        Pending,
    }

    private enum SingleValue
    {
        Only,
    }

    private enum EmptyEnum { }

    [Fact]
    public void Apply_WithEnumType_ShouldSetTypeToString()
    {
        // Arrange
        var schema = new OpenApiSchema { Type = "integer", Format = "int32" };
        SchemaFilterContext context = CreateContext(typeof(TestStatus));

        // Act
        _sut.Apply(schema, context);

        // Assert
        schema.Type.Should().Be("string");
    }

    [Fact]
    public void Apply_WithEnumType_ShouldClearFormat()
    {
        // Arrange
        var schema = new OpenApiSchema { Type = "integer", Format = "int32" };
        SchemaFilterContext context = CreateContext(typeof(TestStatus));

        // Act
        _sut.Apply(schema, context);

        // Assert
        schema.Format.Should().BeNull();
    }

    [Fact]
    public void Apply_WithEnumType_ShouldPopulateEnumValuesAsStrings()
    {
        // Arrange
        var schema = new OpenApiSchema();
        SchemaFilterContext context = CreateContext(typeof(TestStatus));

        // Act
        _sut.Apply(schema, context);

        // Assert
        schema.Enum.Should().HaveCount(3);
        schema
            .Enum.Cast<OpenApiString>()
            .Select(e => e.Value)
            .Should()
            .BeEquivalentTo(["Active", "Inactive", "Pending"]);
    }

    [Fact]
    public void Apply_WithEnumType_ShouldClearExistingEnumValues()
    {
        // Arrange
        var schema = new OpenApiSchema();
        schema.Enum.Add(new OpenApiInteger(0));
        schema.Enum.Add(new OpenApiInteger(1));
        SchemaFilterContext context = CreateContext(typeof(TestStatus));

        // Act
        _sut.Apply(schema, context);

        // Assert
        schema.Enum.Should().AllBeOfType<OpenApiString>();
    }

    [Fact]
    public void Apply_WithNonEnumType_ShouldNotModifySchema()
    {
        // Arrange
        var schema = new OpenApiSchema { Type = "integer", Format = "int32" };
        schema.Enum.Add(new OpenApiInteger(1));
        SchemaFilterContext context = CreateContext(typeof(string));

        // Act
        _sut.Apply(schema, context);

        // Assert
        schema.Type.Should().Be("integer");
        schema.Format.Should().Be("int32");
        schema.Enum.Should().HaveCount(1);
    }

    [Fact]
    public void Apply_WithSingleValueEnum_ShouldContainOneEntry()
    {
        // Arrange
        var schema = new OpenApiSchema();
        SchemaFilterContext context = CreateContext(typeof(SingleValue));

        // Act
        _sut.Apply(schema, context);

        // Assert
        schema.Enum.Should().ContainSingle();
        schema.Enum.Cast<OpenApiString>().Single().Value.Should().Be("Only");
    }

    [Fact]
    public void Apply_WithEmptyEnum_ShouldHaveNoEnumValues()
    {
        // Arrange
        var schema = new OpenApiSchema();
        SchemaFilterContext context = CreateContext(typeof(EmptyEnum));

        // Act
        _sut.Apply(schema, context);

        // Assert
        schema.Type.Should().Be("string");
        schema.Enum.Should().BeEmpty();
    }

    [Fact]
    public void Apply_WithEnumType_ShouldPreserveNameOrder()
    {
        // Arrange
        var schema = new OpenApiSchema();
        SchemaFilterContext context = CreateContext(typeof(TestStatus));

        // Act
        _sut.Apply(schema, context);

        // Assert
        string[] values = schema.Enum.Cast<OpenApiString>().Select(e => e.Value).ToArray();
        values.Should().ContainInOrder("Active", "Inactive", "Pending");
    }

    private static SchemaFilterContext CreateContext(Type type)
    {
        var generatorOptions = new SchemaGeneratorOptions();
        var jsonOptions = new System.Text.Json.JsonSerializerOptions();
        var generator = new SchemaGenerator(generatorOptions, new JsonSerializerDataContractResolver(jsonOptions));
        var repository = new SchemaRepository();

        return new SchemaFilterContext(type, generator, repository);
    }
}
