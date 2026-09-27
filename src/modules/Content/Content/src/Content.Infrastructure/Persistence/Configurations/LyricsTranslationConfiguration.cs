using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _116.Content.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the <see cref="LyricsTranslationEntity" />.
/// Defines the table structure, constraints, and relationships for lyrics translations.
/// </summary>
public class LyricsTranslationConfiguration : IEntityTypeConfiguration<LyricsTranslationEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<LyricsTranslationEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.LyricsId).IsRequired();

        builder.Property(x => x.Language).IsRequired();

        builder.Property(x => x.Text).IsRequired();

        builder.Property(x => x.Source).HasConversion<string>().IsRequired();

        // One translation row per (LyricsId, Language) — corrections update this row's text
        // via an accepted revision, they never create a second row for the same language.
        builder.HasIndex(x => new { x.LyricsId, x.Language }).IsUnique();

        builder.HasOne<LyricsEntity>().WithMany().HasForeignKey(x => x.LyricsId).OnDelete(DeleteBehavior.Cascade);
    }
}
