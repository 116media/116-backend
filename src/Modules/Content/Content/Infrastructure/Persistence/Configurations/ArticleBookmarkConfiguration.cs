using _116.Content.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _116.Content.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for <see cref="ArticleBookmarkEntity" />.
/// </summary>
public class ArticleBookmarkConfiguration : IEntityTypeConfiguration<ArticleBookmarkEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ArticleBookmarkEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId).IsRequired();

        builder.Property(x => x.ArticleId).IsRequired();

        builder.HasIndex(x => new { x.UserId, x.ArticleId }).IsUnique();

        builder.HasOne(x => x.Article).WithMany().HasForeignKey(x => x.ArticleId).OnDelete(DeleteBehavior.Cascade);
    }
}
