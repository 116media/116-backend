using _116.Shared.Application.Extensions;
using AwesomeAssertions;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Xunit;

namespace _116.Unit.Tests.Shared.Application.Extensions;

/// <summary>
/// Unit tests for <see cref="EnumReferenceDocumentFilter"/>.
/// </summary>
public class EnumReferenceDocumentFilterTests
{
    private readonly EnumReferenceDocumentFilter _sut = new();

    [Fact]
    public void Apply_WithInlinedEnumProperty_ShouldReplaceWithRef()
    {
        // Arrange
        OpenApiDocument doc = CreateDocument(
            componentEnumName: "EnumOrderStatus",
            componentEnumValues: ["Draft", "PendingPayment", "Paid", "Cancelled"],
            dtoName: "ContentOrderSummaryDto",
            propertyName: "status",
            inlineEnumValues: ["Draft", "PendingPayment", "Paid", "Cancelled"]
        );

        // Act
        _sut.Apply(doc, CreateContext());

        // Assert
        OpenApiSchema statusProp = doc.Components.Schemas["ContentOrderSummaryDto"].Properties["status"];
        statusProp.Reference.Should().NotBeNull();
        statusProp.Reference!.Id.Should().Be("EnumOrderStatus");
        statusProp.Enum.Should().BeEmpty();
    }

    [Fact]
    public void Apply_WithNonEnumProperty_ShouldNotModify()
    {
        // Arrange
        OpenApiDocument doc = CreateDocument(
            componentEnumName: "EnumOrderStatus",
            componentEnumValues: ["Draft", "PendingPayment", "Paid", "Cancelled"],
            dtoName: "ContentOrderSummaryDto",
            propertyName: "customerName",
            inlineEnumValues: null
        );

        // Act
        _sut.Apply(doc, CreateContext());

        // Assert
        OpenApiSchema customerProp = doc.Components.Schemas["ContentOrderSummaryDto"].Properties["customerName"];
        customerProp.Type.Should().Be("string");
        customerProp.Reference.Should().BeNull();
    }

    [Fact]
    public void Apply_WithPropertyAlreadyUsingRef_ShouldNotModify()
    {
        // Arrange
        var doc = new OpenApiDocument
        {
            Components = new OpenApiComponents
            {
                Schemas = new Dictionary<string, OpenApiSchema>
                {
                    ["EnumPaymentMethod"] = CreateEnumSchema("BankTransfer", "MobileMoney", "Cash"),
                    ["PaymentDto"] = new OpenApiSchema
                    {
                        Type = "object",
                        Properties = new Dictionary<string, OpenApiSchema>
                        {
                            ["paymentMethod"] = new OpenApiSchema
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.Schema,
                                    Id = "EnumPaymentMethod",
                                },
                            },
                        },
                    },
                },
            },
        };

        // Act
        _sut.Apply(doc, CreateContext());

        // Assert
        OpenApiSchema prop = doc.Components.Schemas["PaymentDto"].Properties["paymentMethod"];
        prop.Reference.Should().NotBeNull();
        prop.Reference!.Id.Should().Be("EnumPaymentMethod");
    }

    [Fact]
    public void Apply_WithMultipleInlinedProperties_ShouldReplaceAll()
    {
        // Arrange
        var doc = new OpenApiDocument
        {
            Components = new OpenApiComponents
            {
                Schemas = new Dictionary<string, OpenApiSchema>
                {
                    ["EnumOrderStatus"] = CreateEnumSchema("Draft", "PendingPayment", "Paid", "Cancelled"),
                    ["EnumPaymentStatus"] = CreateEnumSchema("Pending", "Verified", "Rejected"),
                    ["SomeDto"] = new OpenApiSchema
                    {
                        Type = "object",
                        Properties = new Dictionary<string, OpenApiSchema>
                        {
                            ["orderStatus"] = CreateEnumSchema("Draft", "PendingPayment", "Paid", "Cancelled"),
                            ["paymentStatus"] = CreateEnumSchema("Pending", "Verified", "Rejected"),
                        },
                    },
                },
            },
        };

        // Act
        _sut.Apply(doc, CreateContext());

        // Assert
        OpenApiSchema orderProp = doc.Components.Schemas["SomeDto"].Properties["orderStatus"];
        orderProp.Reference.Should().NotBeNull();
        orderProp.Reference!.Id.Should().Be("EnumOrderStatus");

        OpenApiSchema paymentProp = doc.Components.Schemas["SomeDto"].Properties["paymentStatus"];
        paymentProp.Reference.Should().NotBeNull();
        paymentProp.Reference!.Id.Should().Be("EnumPaymentStatus");
    }

    [Fact]
    public void Apply_WithInlinedEnumNotMatchingAnyComponent_ShouldNotModify()
    {
        // Arrange
        var doc = new OpenApiDocument
        {
            Components = new OpenApiComponents
            {
                Schemas = new Dictionary<string, OpenApiSchema>
                {
                    ["SomeDto"] = new OpenApiSchema
                    {
                        Type = "object",
                        Properties = new Dictionary<string, OpenApiSchema>
                        {
                            ["mystery"] = CreateEnumSchema("Alpha", "Beta", "Gamma"),
                        },
                    },
                },
            },
        };

        // Act
        _sut.Apply(doc, CreateContext());

        // Assert
        OpenApiSchema prop = doc.Components.Schemas["SomeDto"].Properties["mystery"];
        prop.Reference.Should().BeNull();
        prop.Enum.Should().HaveCount(3);
    }

    [Fact]
    public void Apply_WithEmptyDocument_ShouldNotThrow()
    {
        // Arrange
        var doc = new OpenApiDocument
        {
            Components = new OpenApiComponents { Schemas = new Dictionary<string, OpenApiSchema>() },
        };

        // Act
        Action act = () => _sut.Apply(doc, CreateContext());

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Apply_WithSchemaWithNullProperties_ShouldNotThrow()
    {
        // Arrange — force Properties to null to cover the null-guard branch
        var schemaWithNullProps = new OpenApiSchema { Type = "object" };
        schemaWithNullProps.Properties = null!;

        var doc = new OpenApiDocument
        {
            Components = new OpenApiComponents
            {
                Schemas = new Dictionary<string, OpenApiSchema>
                {
                    ["EnumStatus"] = CreateEnumSchema("Draft", "Paid"),
                    ["NullPropsSchema"] = schemaWithNullProps,
                },
            },
        };

        // Act
        Action act = () => _sut.Apply(doc, CreateContext());

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Apply_ShouldMatchByValuesRegardlessOfOrder()
    {
        // Arrange — component has values in a different order than the inline property
        var doc = new OpenApiDocument
        {
            Components = new OpenApiComponents
            {
                Schemas = new Dictionary<string, OpenApiSchema>
                {
                    ["EnumStatus"] = CreateEnumSchema("Cancelled", "Draft", "Paid"),
                    ["Dto"] = new OpenApiSchema
                    {
                        Type = "object",
                        Properties = new Dictionary<string, OpenApiSchema>
                        {
                            ["status"] = CreateEnumSchema("Draft", "Paid", "Cancelled"),
                        },
                    },
                },
            },
        };

        // Act
        _sut.Apply(doc, CreateContext());

        // Assert
        OpenApiSchema prop = doc.Components.Schemas["Dto"].Properties["status"];
        prop.Reference.Should().NotBeNull();
        prop.Reference!.Id.Should().Be("EnumStatus");
    }

    private static OpenApiDocument CreateDocument(
        string componentEnumName,
        string[] componentEnumValues,
        string dtoName,
        string propertyName,
        string[]? inlineEnumValues
    )
    {
        var schemas = new Dictionary<string, OpenApiSchema>
        {
            [componentEnumName] = CreateEnumSchema(componentEnumValues),
        };

        var properties = new Dictionary<string, OpenApiSchema>();

        if (inlineEnumValues != null)
        {
            properties[propertyName] = CreateEnumSchema(inlineEnumValues);
        }
        else
        {
            properties[propertyName] = new OpenApiSchema { Type = "string" };
        }

        schemas[dtoName] = new OpenApiSchema { Type = "object", Properties = properties };

        return new OpenApiDocument { Components = new OpenApiComponents { Schemas = schemas } };
    }

    private static OpenApiSchema CreateEnumSchema(params string[] values)
    {
        var schema = new OpenApiSchema { Type = "string" };

        foreach (string value in values)
        {
            schema.Enum.Add(new OpenApiString(value));
        }

        return schema;
    }

    private static DocumentFilterContext CreateContext()
    {
        var jsonOptions = new System.Text.Json.JsonSerializerOptions();
        var generatorOptions = new SchemaGeneratorOptions();
        var generator = new SchemaGenerator(generatorOptions, new JsonSerializerDataContractResolver(jsonOptions));
        var repository = new SchemaRepository();

        return new DocumentFilterContext([], generator, repository);
    }
}
