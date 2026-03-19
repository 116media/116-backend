using _116.Content.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _116.Content.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the <see cref="ArticleTagEntity" /> junction table.
/// </summary>
public class ArticleTagConfiguration : IEntityTypeConfiguration<ArticleTagEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ArticleTagEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ArticleId).IsRequired();

        builder.Property(x => x.TagId).IsRequired();

        builder.HasIndex(x => new { x.ArticleId, x.TagId }).IsUnique();

        builder
            .HasOne(x => x.Article)
            .WithMany(a => a.Tags)
            .HasForeignKey(x => x.ArticleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Tag).WithMany().HasForeignKey(x => x.TagId).OnDelete(DeleteBehavior.Cascade);
    }
}
