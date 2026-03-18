using _116.Content.Domain.Constants;
using _116.Content.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _116.Content.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for <see cref="ArticleCommentEntity" />.
/// </summary>
public class ArticleCommentConfiguration : IEntityTypeConfiguration<ArticleCommentEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ArticleCommentEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId).IsRequired();

        builder.Property(x => x.ArticleId).IsRequired();

        builder.Property(x => x.Body).HasMaxLength(ContentConstants.MaxCommentBodyLength).IsRequired();

        builder.Property(x => x.IsDeleted).HasDefaultValue(false).IsRequired();

        builder.Property(x => x.DeletedAt).IsRequired(false);

        builder.HasOne(x => x.Article).WithMany().HasForeignKey(x => x.ArticleId).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.ArticleId).HasDatabaseName("ix_article_comments_article");
    }
}
