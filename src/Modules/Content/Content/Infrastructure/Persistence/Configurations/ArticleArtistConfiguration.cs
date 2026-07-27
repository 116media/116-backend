using _116.Content.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _116.Content.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the <see cref="ArticleArtistEntity" />.
/// Defines the table structure, constraints, and relationships for article-artist tagging.
/// </summary>
public class ArticleArtistConfiguration : IEntityTypeConfiguration<ArticleArtistEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ArticleArtistEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.ArticleId, x.ArtistId }).IsUnique();

        // The composite above leads with ArticleId, so it cannot serve "all articles for
        // this artist" — the only direction the public page reads. Without this the news tab
        // and the article term of contentCount scan the whole join table.
        builder.HasIndex(x => x.ArtistId);

        builder.HasOne(x => x.Article).WithMany().HasForeignKey(x => x.ArticleId).OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Artist).WithMany().HasForeignKey(x => x.ArtistId).OnDelete(DeleteBehavior.Cascade);
    }
}
