using _116.Content.Domain.Constants;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _116.Content.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the <see cref="ArticleEntity" />.
/// Defines the table structure, constraints, relationships, and indexes for articles.
/// </summary>
public class ArticleConfiguration : IEntityTypeConfiguration<ArticleEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ArticleEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title).HasMaxLength(ContentConstants.MaxTitleLength).IsRequired();

        builder
            .Property(x => x.Slug)
            .HasConversion(slug => slug.Value, value => new Slug(value))
            .HasMaxLength(ContentConstants.MaxSlugLength)
            .IsRequired();

        builder
            .Property(x => x.Headline)
            .HasMaxLength(ContentConstants.MaxHeadlineLength)
            .HasDefaultValue(string.Empty)
            .IsRequired();

        builder.Property(x => x.Body).HasDefaultValue(string.Empty).IsRequired();

        builder.Property(x => x.CoverImageFileId).IsRequired(false);

        builder.Property(x => x.AuthorId).IsRequired();

        builder.Property(x => x.Status).HasConversion<string>().HasDefaultValue(EnumContentStatus.Draft).IsRequired();

        builder
            .Property(x => x.RejectionReason)
            .HasMaxLength(ContentConstants.MaxRejectionReasonLength)
            .IsRequired(false);

        builder.Property(x => x.MetaTitle).HasMaxLength(ContentConstants.MaxMetaTitleLength).IsRequired(false);

        builder
            .Property(x => x.MetaDescription)
            .HasMaxLength(ContentConstants.MaxMetaDescriptionLength)
            .IsRequired(false);

        builder.Property(x => x.SocialBoost).HasDefaultValue(false).IsRequired();

        builder.Property(x => x.IsPromoted).HasDefaultValue(false).IsRequired();

        builder.Property(x => x.PromotionLevelId).IsRequired(false);

        builder.Property(x => x.UnpromotedAt).IsRequired(false);

        builder.Property(x => x.UnpromotedBy).IsRequired(false);

        builder.Property(x => x.UnpromotedReason).HasMaxLength(500).IsRequired(false);

        builder.Property(x => x.LikeCount).HasDefaultValue(0).IsRequired();

        builder.Property(x => x.CommentCount).HasDefaultValue(0).IsRequired();

        builder.Property(x => x.ShareCount).HasDefaultValue(0).IsRequired();

        builder.Property(x => x.BookmarkCount).HasDefaultValue(0).IsRequired();

        builder.HasIndex(x => x.Slug).IsUnique();
        builder.HasIndex(x => x.Title).IsUnique();

        // The artist content predicate joins article_artists to articles filtered on status;
        // without this the news term scans the articles table per artist row.
        builder.HasIndex(x => x.Status);

        // The published listings and the popular/promoted feeds all filter on status and order
        // by published_at; without the composite Postgres bitmap-scans status then full-sorts.
        builder
            .HasIndex(x => new { x.Status, x.PublishedAt })
            .HasDatabaseName("ix_articles_status_published_at")
            .IsDescending(false, true);

        builder
            .HasIndex(x => new
            {
                x.CategoryId,
                x.Status,
                x.PublishedAt,
            })
            .HasDatabaseName("ix_articles_category_status_published_at");

        // Promoted rows are a sliver of the table, so the homepage grid reads a filtered index.
        builder
            .HasIndex(x => x.PublishedAt)
            .HasDatabaseName("ix_articles_promoted_published_at")
            .HasFilter("is_promoted = true")
            .IsDescending(true);

        builder.HasOne<CategoryEntity>().WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne<CustomerEntity>()
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasOne<PromotionLevelEntity>()
            .WithMany()
            .HasForeignKey(x => x.PromotionLevelId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
