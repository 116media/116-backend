using _116.Content.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _116.Content.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for <see cref="ArticleShareEntity" />.
/// UserId is nullable (anonymous shares allowed); no FK to identity schema by design.
/// </summary>
public class ArticleShareConfiguration : IEntityTypeConfiguration<ArticleShareEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ArticleShareEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId).IsRequired(false);

        builder.Property(x => x.ArticleId).IsRequired();

        builder.Property(x => x.ShareChannel).HasConversion<string>().HasMaxLength(50).IsRequired(false);

        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasOne(x => x.Article).WithMany().HasForeignKey(x => x.ArticleId).OnDelete(DeleteBehavior.Cascade);

        builder
            .HasIndex(x => new
            {
                x.UserId,
                x.CreatedAt,
                x.ArticleId,
            })
            .IsDescending(false, true, false)
            .HasFilter("user_id IS NOT NULL")
            .HasDatabaseName("ix_article_shares_user_created_article");
    }
}
