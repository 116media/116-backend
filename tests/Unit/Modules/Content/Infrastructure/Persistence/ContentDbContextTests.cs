using _116.Content.Domain.Constants;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Infrastructure.Persistence;

/// <summary>
/// Unit tests for <see cref="ContentDbContext"/>.
/// </summary>
public class ContentDbContextTests
{
    /// <summary>
    /// The number of domain entity types the Content module declares. The count is asserted
    /// separately so a reflection query that stops matching cannot turn the theories below into
    /// silently passing runs with zero cases.
    /// </summary>
    private const int DomainEntityCount = 49;

    /// <summary>
    /// The model is built once for the whole class. <see cref="IModel"/> is a frozen snapshot, so
    /// it stays readable after the context that produced it is disposed.
    /// </summary>
    private static readonly IModel SharedModel = BuildModel();

    /// <summary>
    /// Every concrete type in the infrastructure assembly, scanned once so the configuration
    /// theory does not re-enumerate the assembly for each of its rows.
    /// </summary>
    private static readonly Type[] InfrastructureTypes = typeof(ContentDbContext)
        .Assembly.GetTypes()
        .Where(t => t is { IsClass: true, IsAbstract: false })
        .ToArray();

    /// <summary>
    /// Builds options backed by a private in-memory database.
    /// </summary>
    /// <returns>Options for a throwaway context instance.</returns>
    private static DbContextOptions<ContentDbContext> CreateOptions() =>
        new DbContextOptionsBuilder<ContentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

    /// <summary>
    /// Builds the Content model once so the theories below read a shared snapshot.
    /// </summary>
    /// <returns>The frozen model produced by <see cref="ContentDbContext.OnModelCreating"/>.</returns>
    private static IModel BuildModel()
    {
        using var context = new ContentDbContext(CreateOptions());
        return context.Model;
    }

    /// <summary>
    /// Enumerates the concrete domain entity types declared by the Content module from the
    /// assembly's type system rather than from a hand-written list. Reflection is used here to
    /// walk the type system, never to reach private state.
    /// </summary>
    /// <returns>The domain entity types, ordered by name for stable test output.</returns>
    private static IReadOnlyList<Type> DomainEntityTypes() =>
        typeof(ContentTypeEntity)
            .Assembly.GetTypes()
            .Where(t =>
                t is { IsClass: true, IsAbstract: false, IsPublic: true }
                && t.Namespace == typeof(ContentTypeEntity).Namespace
                && t.Name.EndsWith("Entity", StringComparison.Ordinal)
            )
            .OrderBy(t => t.Name)
            .ToList();

    /// <summary>
    /// Supplies one theory row per domain entity type, so an entity added to
    /// <c>Domain/Entities/</c> is covered without any change to this file.
    /// </summary>
    /// <returns>The domain entity types as theory rows.</returns>
    public static TheoryData<Type> DomainEntities() => new(DomainEntityTypes());

    #region Domain Entity Mapping

    [Fact]
    public void DomainEntities_ShouldDiscoverEveryDeclaredEntityType()
    {
        DomainEntityTypes().Count.Should().Be(DomainEntityCount);
    }

    [Theory]
    [MemberData(nameof(DomainEntities))]
    public void Model_ShouldMapEveryDomainEntityWithAPrimaryKeyInTheContentSchema(Type entityType)
    {
        IEntityType? mapped = SharedModel.FindEntityType(entityType);

        mapped.Should().NotBeNull($"{entityType.Name} is a domain entity and must be mapped");
        mapped!.FindPrimaryKey().Should().NotBeNull($"{entityType.Name} must declare a primary key");
        mapped.GetSchema().Should().Be(ContentConstants.SchemaName, $"{entityType.Name} belongs to the module schema");
    }

    [Theory]
    [MemberData(nameof(DomainEntities))]
    public void Model_ShouldApplyAnExplicitConfigurationForEveryDomainEntity(Type entityType)
    {
        Type configurationContract = typeof(IEntityTypeConfiguration<>).MakeGenericType(entityType);

        Type? configuration = Array.Find(InfrastructureTypes, configurationContract.IsAssignableFrom);

        configuration
            .Should()
            .NotBeNull($"{entityType.Name} must be configured explicitly rather than by EF Core convention");
    }

    [Fact]
    public void Model_ShouldNotMapAnyTypeOutsideTheDomainEntities()
    {
        IEnumerable<string> mapped = SharedModel.GetEntityTypes().Select(e => e.ClrType.Name);

        mapped.Should().BeEquivalentTo(DomainEntityTypes().Select(t => t.Name));
    }

    #endregion

    #region DbSet Properties

    /// <summary>
    /// Enumerates the public <see cref="DbSet{TEntity}"/> properties the context declares.
    /// Reflection walks the type system here, matching the discovery style of the theories above.
    /// </summary>
    /// <returns>The DbSet property infos, ordered by name for stable test output.</returns>
    private static IReadOnlyList<System.Reflection.PropertyInfo> DbSetProperties() =>
        typeof(ContentDbContext)
            .GetProperties()
            .Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
            .OrderBy(p => p.Name)
            .ToList();

    [Fact]
    public void DbSetProperties_ShouldExposeOneSetPerDomainEntity()
    {
        // Arrange & Act — the count guard keeps the getter sweep below from silently shrinking
        DbSetProperties().Count.Should().Be(DomainEntityCount);
    }

    [Fact]
    public void Context_ShouldReturnAUsableDbSetFromEveryDeclaredSetProperty()
    {
        // Arrange
        using var context = new ContentDbContext(CreateOptions());

        // Act & Assert
        foreach (System.Reflection.PropertyInfo property in DbSetProperties())
        {
            property.GetValue(context).Should().NotBeNull($"{property.Name} must expose a usable DbSet");
        }
    }

    #endregion

    #region Schema and Configuration

    [Fact]
    public void OnModelCreating_ShouldApplyConfigurationsFromAssembly()
    {
        IEntityType? articleEntityType = SharedModel.FindEntityType(typeof(ArticleEntity));

        articleEntityType.Should().NotBeNull();
        articleEntityType!
            .FindProperty(nameof(ArticleEntity.Title))!
            .GetMaxLength()
            .Should()
            .Be(ContentConstants.MaxTitleLength);
    }

    [Fact]
    public void Context_ShouldSetDefaultSchemaToContent()
    {
        SharedModel.GetDefaultSchema().Should().Be(ContentConstants.SchemaName);
    }

    #endregion
}
