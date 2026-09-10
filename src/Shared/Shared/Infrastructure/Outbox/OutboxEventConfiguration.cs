using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _116.Shared.Infrastructure.Outbox;

/// <summary>
/// Entity Framework Core configuration for <see cref="OutboxEventEntity" />. Applied by every
/// module context, so each module's outbox lives in its own schema.
/// </summary>
public class OutboxEventConfiguration : IEntityTypeConfiguration<OutboxEventEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<OutboxEventEntity> builder)
    {
        builder.ToTable("domain_event_outbox");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EventType).HasMaxLength(500).IsRequired();

        builder.Property(x => x.Payload).IsRequired();

        builder.Property(x => x.OccurredOn).IsRequired();

        builder.Property(x => x.DispatchedAt).IsRequired(false);

        builder.Property(x => x.AttemptCount).HasDefaultValue(0).IsRequired();

        builder.Property(x => x.LastError).IsRequired(false);

        // The replay claim reads undispatched rows oldest first; the filter keeps the index to
        // the backlog rather than the whole event history.
        builder
            .HasIndex(x => x.OccurredOn)
            .HasFilter("dispatched_at IS NULL")
            .HasDatabaseName("ix_domain_event_outbox_pending");
    }
}
