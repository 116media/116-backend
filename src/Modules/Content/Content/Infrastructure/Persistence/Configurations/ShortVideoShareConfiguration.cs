using _116.Content.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _116.Content.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for <see cref="ShortVideoShareEntity" />.
/// UserId is nullable (anonymous shares allowed); no FK to identity schema by design.
/// </summary>
public class ShortVideoShareConfiguration : IEntityTypeConfiguration<ShortVideoShareEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ShortVideoShareEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId).IsRequired(false);

        builder.Property(x => x.ShortVideoId).IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired();

        builder
            .HasOne(x => x.ShortVideo)
            .WithMany()
            .HasForeignKey(x => x.ShortVideoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
