using _116.Content.Domain.Constants;
using _116.Content.Infrastructure.Persistence.Projections;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _116.Content.Infrastructure.Persistence.Configurations;

/// <summary>
/// Maps <see cref="SearchDocument" /> onto the search_documents table. The vector is a
/// generated column, so it is read back after every write and never sent.
/// </summary>
public class SearchDocumentConfiguration : IPostgresOnlyConfiguration, IEntityTypeConfiguration<SearchDocument>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<SearchDocument> builder)
    {
        builder.ToTable("search_documents", ContentConstants.SchemaName);

        builder.HasKey(document => new { document.Type, document.EntityId });

        builder.Property(document => document.Type).HasColumnName("type").HasConversion<string>();
        builder.Property(document => document.EntityId).HasColumnName("entity_id");
        builder.Property(document => document.Slug).HasColumnName("slug").IsRequired();
        builder.Property(document => document.IsVisible).HasColumnName("is_visible");
        builder.Property(document => document.Title).HasColumnName("title").IsRequired();
        builder.Property(document => document.Summary).HasColumnName("summary").IsRequired();
        builder.Property(document => document.Keywords).HasColumnName("keywords").IsRequired();
        builder.Property(document => document.Body).HasColumnName("body").IsRequired();
        builder.Property(document => document.ImageFileId).HasColumnName("image_file_id");
        builder.Property(document => document.CategoryId).HasColumnName("category_id");
        builder.Property(document => document.PublishedAt).HasColumnName("published_at");

        builder
            .Property(document => document.SearchVector)
            .HasColumnName("search_vector")
            .HasColumnType("tsvector")
            .ValueGeneratedOnAddOrUpdate();

        builder.Property(document => document.SearchVector).Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
        builder.Property(document => document.SearchVector).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
    }
}
