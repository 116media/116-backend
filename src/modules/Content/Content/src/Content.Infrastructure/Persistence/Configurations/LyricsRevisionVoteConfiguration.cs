using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _116.Content.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the <see cref="LyricsRevisionVoteEntity" />.
/// Defines the table structure, constraints, and relationships for lyrics-revision votes.
/// </summary>
public class LyricsRevisionVoteConfiguration : IEntityTypeConfiguration<LyricsRevisionVoteEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<LyricsRevisionVoteEntity> builder)
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
            .HasOne<LyricsRevisionEntity>()
            .WithMany()
            .HasForeignKey(x => x.RevisionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
