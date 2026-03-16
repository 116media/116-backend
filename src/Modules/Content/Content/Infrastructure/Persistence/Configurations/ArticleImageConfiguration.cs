using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _116.Content.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the <see cref="ArticleImageEntity" />.
/// Tracks every image asset (cover + body images) associated with an article.
/// </summary>
public class ArticleImageConfiguration : IEntityTypeConfiguration<ArticleImageEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ArticleImageEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.StorageKey).IsRequired();

        builder.Property(x => x.Url).HasMaxLength(500).IsRequired();

        builder
            .Property(x => x.ImageType)
            .HasConversion<string>()
            .HasDefaultValue(EnumArticleImageType.Body)
            .IsRequired();

        builder.HasIndex(x => x.ArticleId);

        builder
            .HasOne(x => x.Article)
            .WithMany(a => a.Images)
            .HasForeignKey(x => x.ArticleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
