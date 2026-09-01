using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _116.Shared.Infrastructure.Outbox;

/// <summary>
/// Entity Framework Core configuration for <see cref="ProcessedDomainEventEntity" />. The
/// composite key is what makes a replayed handler a no-op.
/// </summary>
public class ProcessedDomainEventConfiguration : IEntityTypeConfiguration<ProcessedDomainEventEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ProcessedDomainEventEntity> builder)
    {
        builder.ToTable("processed_domain_events");

        builder.HasKey(x => new { x.EventId, x.HandlerName });

        builder.Property(x => x.HandlerName).HasMaxLength(200).IsRequired();

        builder.Property(x => x.ProcessedAt).IsRequired();
    }
}
