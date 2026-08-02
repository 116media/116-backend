using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _116.Content.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the <see cref="LyricsTranslationVoteEntity" />.
/// Defines the table structure, constraints, and relationships for translation-revision votes.
/// </summary>
public class LyricsTranslationVoteConfiguration : IEntityTypeConfiguration<LyricsTranslationVoteEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<LyricsTranslationVoteEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.RevisionId).IsRequired();

        builder.Property(x => x.UserId).IsRequired();

        builder.Property(x => x.Vote).HasConversion<string>().IsRequired();

        builder.Property(x => x.Comment).IsRequired(false);

        // The actual one-vote-per-user-per-revision enforcement — a real DB unique constraint,
        // not just an application-level check.
        builder.HasIndex(x => new { x.RevisionId, x.UserId }).IsUnique();

        builder
            .HasOne<LyricsTranslationRevisionEntity>()
            .WithMany()
            .HasForeignKey(x => x.RevisionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
