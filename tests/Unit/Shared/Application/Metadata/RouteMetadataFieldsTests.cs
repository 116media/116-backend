using System.Reflection;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPublishedArticles;
using _116.Identity.Application.Auth.UseCases.Public.Commands.Login;
using _116.Shared.Application.Metadata;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Shared.Application.Metadata;

/// <summary>
/// Reflection-based tests covering every <see cref="RouteMetadata"/> static field declared
/// by a <c>*MetaField</c> class across the Identity and Content modules. Discovery is dynamic,
/// so newly added endpoints are covered automatically without editing this file.
/// </summary>
public class RouteMetadataFieldsTests
{
    private const int MinSummaryLength = 10;
    private const int MinDescriptionLength = 50;

    /// <summary>
    /// The module assemblies scanned for <c>*MetaField</c> classes, anchored by a known
    /// metafield type in each so the reference survives assembly renames.
    /// </summary>
    private static readonly Assembly[] MetaFieldAssemblies =
    [
        typeof(PublicLoginMetaField).Assembly,
        typeof(PublicGetPublishedArticlesMetaField).Assembly,
    ];

    /// <summary>
    /// Yields every static class whose name ends with <c>MetaField</c> in the scanned assemblies.
    /// </summary>
    private static IEnumerable<Type> MetaFieldClasses()
    {
        return MetaFieldAssemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsClass && type.Name.EndsWith("MetaField", StringComparison.Ordinal));
    }

    /// <summary>
    /// Provides one theory case per <see cref="RouteMetadata"/> field, identified by the
    /// declaring type's assembly-qualified name and the field name.
    /// </summary>
    public static IEnumerable<object[]> AllRouteMetadataFields()
    {
        foreach (Type type in MetaFieldClasses())
        {
            foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (field.FieldType == typeof(RouteMetadata))
                {
                    yield return [type.AssemblyQualifiedName!, field.Name];
                }
            }
        }
    }

    /// <summary>
    /// Resolves every declared <see cref="RouteMetadata"/> value alongside a human-readable
    /// owner label (<c>Class.Field</c>) for whole-set assertions.
    /// </summary>
    private static IEnumerable<(string Owner, RouteMetadata Metadata)> AllRouteMetadata()
    {
        foreach (Type type in MetaFieldClasses())
        {
            foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (field.FieldType == typeof(RouteMetadata))
                {
                    yield return ($"{type.Name}.{field.Name}", (RouteMetadata)field.GetValue(null)!);
                }
            }
        }
    }

    [Theory]
    [MemberData(nameof(AllRouteMetadataFields))]
    public void RouteMetadataField_ShouldBePopulatedReadonly(string assemblyQualifiedTypeName, string fieldName)
    {
        // Arrange
        Type? type = Type.GetType(assemblyQualifiedTypeName);
        type.Should().NotBeNull();

        FieldInfo? field = type!.GetField(fieldName, BindingFlags.Public | BindingFlags.Static);
        field.Should().NotBeNull();

        // Act
        field!.IsInitOnly.Should().BeTrue($"{type.Name}.{fieldName} should be readonly");
        var metadata = (RouteMetadata)field.GetValue(null)!;

        // Assert
        metadata.Name.Should().NotBeNullOrWhiteSpace();
        metadata.Summary.Should().NotBeNullOrWhiteSpace();
        metadata.Description.Should().NotBeNullOrWhiteSpace();
        metadata
            .Summary.Length.Should()
            .BeGreaterThan(MinSummaryLength, $"{type.Name}.{fieldName} summary should be descriptive");
        metadata
            .Description.Length.Should()
            .BeGreaterThan(MinDescriptionLength, $"{type.Name}.{fieldName} description should be detailed");
    }

    [Fact]
    public void MetaFieldClasses_ShouldBeDiscoveredAndStatic()
    {
        // Arrange
        List<Type> types = MetaFieldClasses().ToList();

        // Assert
        types.Should().NotBeEmpty("both module assemblies should expose *MetaField classes");
        types.Should().OnlyContain(type => type.IsAbstract && type.IsSealed, "every *MetaField class should be static");
    }

    [Fact]
    public void RouteMetadataFields_ShouldBeDiscovered()
    {
        // Arrange
        List<object[]> fields = AllRouteMetadataFields().ToList();

        // Assert
        fields.Should().NotBeEmpty("every *MetaField class should expose at least one RouteMetadata field");
    }

    [Fact]
    public void RouteMetadataNames_ShouldBeUnique()
    {
        // Act — a duplicated Name is a colliding OpenAPI operationId
        List<string> duplicates = AllRouteMetadata()
            .GroupBy(entry => entry.Metadata.Name, StringComparer.Ordinal)
            .Where(group => group.Count() > 1)
            .Select(group => $"\"{group.Key}\" used by {string.Join(", ", group.Select(entry => entry.Owner))}")
            .ToList();

        // Assert
        duplicates.Should().BeEmpty("route metadata Name is the OpenAPI operationId and must be unique");
    }

    [Fact]
    public void RouteMetadataNames_ShouldNotContainWhitespace()
    {
        // Act — the Name maps to a route/operation identifier, which cannot contain whitespace
        List<string> offenders = AllRouteMetadata()
            .Where(entry => entry.Metadata.Name.Any(char.IsWhiteSpace))
            .Select(entry => $"{entry.Owner} => \"{entry.Metadata.Name}\"")
            .ToList();

        // Assert
        offenders.Should().BeEmpty("route metadata Name must be a whitespace-free operation identifier");
    }
}
