using _116.Content.Domain.Constants;
using _116.Content.Infrastructure.Persistence;
using _116.Content.Infrastructure.Persistence.Projections;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace _116.Content.Unit.Tests.Infrastructure.Persistence;

/// <summary>
/// Unit tests for <see cref="Configurations.SearchDocumentConfiguration" />. The read model is
/// mapped only under the Npgsql provider, so the model is built against it; EF resolves the
/// mapping from the provider alone and never opens the connection.
/// </summary>
public class SearchDocumentConfigurationTests
{
    /// <summary>
    /// A well-formed connection string that is never dialled.
    /// </summary>
    private const string Unused = "Host=unit.invalid;Database=unit;Username=unit;Password=unit";

    /// <summary>
    /// The Npgsql model, built once and frozen, so each fact reads the same snapshot.
    /// </summary>
    private static readonly IModel PostgresModel = BuildModel();

    private static readonly IEntityType Document =
        PostgresModel.FindEntityType(typeof(SearchDocument))
        ?? throw new InvalidOperationException("SearchDocument is absent from the Npgsql model.");

    private static IModel BuildModel()
    {
        DbContextOptions<ContentDbContext> options = new DbContextOptionsBuilder<ContentDbContext>()
            .UseNpgsql(Unused)
            .UseSnakeCaseNamingConvention()
            .Options;

        using var context = new ContentDbContext(options);
        return context.Model;
    }

    private static IProperty Property(string name) =>
        Document.FindProperty(name) ?? throw new InvalidOperationException($"{name} is not mapped.");

    [Fact]
    public void Configure_ShouldMapTheReadModelOntoItsOwnTable()
    {
        Document.GetTableName().Should().Be("search_documents");
        Document.GetSchema().Should().Be(ContentConstants.SchemaName);
    }

    [Fact]
    public void Configure_ShouldKeyOnTheTypeAndTheSourceIdentifier()
    {
        IReadOnlyList<IProperty>? key = Document.FindPrimaryKey()?.Properties;

        key.Should().NotBeNull();
        key!
            .Select(property => property.Name)
            .Should()
            .Equal(nameof(SearchDocument.Type), nameof(SearchDocument.EntityId));
    }

    [Fact]
    public void Configure_ShouldStoreTheContentTypeAsText()
    {
        Property(nameof(SearchDocument.Type))
            .GetProviderClrType()
            .Should()
            .Be<string>("the column is read by hand in migrations and backfills");
    }

    [Theory]
    [InlineData(nameof(SearchDocument.Type), "type")]
    [InlineData(nameof(SearchDocument.EntityId), "entity_id")]
    [InlineData(nameof(SearchDocument.Slug), "slug")]
    [InlineData(nameof(SearchDocument.IsVisible), "is_visible")]
    [InlineData(nameof(SearchDocument.Title), "title")]
    [InlineData(nameof(SearchDocument.Summary), "summary")]
    [InlineData(nameof(SearchDocument.Keywords), "keywords")]
    [InlineData(nameof(SearchDocument.Body), "body")]
    [InlineData(nameof(SearchDocument.ImageFileId), "image_file_id")]
    [InlineData(nameof(SearchDocument.CategoryId), "category_id")]
    [InlineData(nameof(SearchDocument.PublishedAt), "published_at")]
    [InlineData(nameof(SearchDocument.SearchVector), "search_vector")]
    public void Configure_ShouldNameEveryColumnExplicitly(string property, string column)
    {
        Property(property).GetColumnName().Should().Be(column);
    }

    [Theory]
    [InlineData(nameof(SearchDocument.Slug))]
    [InlineData(nameof(SearchDocument.Title))]
    [InlineData(nameof(SearchDocument.Summary))]
    [InlineData(nameof(SearchDocument.Keywords))]
    [InlineData(nameof(SearchDocument.Body))]
    public void Configure_ShouldRequireEveryIndexedTextField(string property)
    {
        Property(property).IsNullable.Should().BeFalse();
    }

    [Fact]
    public void Configure_ShouldTreatTheVectorAsGeneratedByTheDatabase()
    {
        IProperty vector = Property(nameof(SearchDocument.SearchVector));

        vector.GetColumnType().Should().Be("tsvector");
        vector.ValueGenerated.Should().Be(ValueGenerated.OnAddOrUpdate);
    }

    [Fact]
    public void Configure_ShouldNeverSendTheVectorOnAWrite()
    {
        IProperty vector = Property(nameof(SearchDocument.SearchVector));

        vector.GetBeforeSaveBehavior().Should().Be(PropertySaveBehavior.Ignore);
        vector
            .GetAfterSaveBehavior()
            .Should()
            .Be(PropertySaveBehavior.Ignore, "the column is generated, so an update naming it is rejected");
    }

    [Fact]
    public void Configure_ShouldNotMapTheReadModelOnAnotherProvider()
    {
        using var inMemory = new ContentDbContext(
            new DbContextOptionsBuilder<ContentDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options
        );

        inMemory.Model.FindEntityType(typeof(SearchDocument)).Should().BeNull("tsvector is understood by Npgsql alone");
    }
}
