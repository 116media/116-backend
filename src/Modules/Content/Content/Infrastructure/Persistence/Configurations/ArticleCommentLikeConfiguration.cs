using _116.Content.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _116.Content.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for <see cref="ArticleCommentLikeEntity" />.
/// </summary>
public class ArticleCommentLikeConfiguration : IEntityTypeConfiguration<ArticleCommentLikeEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ArticleCommentLikeEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId).IsRequired();

        builder.Property(x => x.CommentId).IsRequired();

        builder.HasOne(x => x.Comment).WithMany().HasForeignKey(x => x.CommentId).OnDelete(DeleteBehavior.Cascade);

        builder
            .HasIndex(x => new { x.CommentId, x.UserId })
            .IsUnique()
            .HasDatabaseName("ix_article_comment_likes_comment_user");
    }
}
