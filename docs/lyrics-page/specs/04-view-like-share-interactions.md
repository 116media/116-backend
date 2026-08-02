# Spec 04 — View, Like & Share Interactions

`LyricsEntity` tracks zero interactions today. This spec adds the full system, copied 1:1 from
`ShortVideoEntity`'s existing like/share/view-event model (`Domain/Entities/ShortVideoLikeEntity.cs`,
`ShortVideoShareEntity.cs`, `ShortVideoViewEventEntity.cs`, and their handlers under
`Application/Interactions/UseCases/Public/Commands/`) with every `ShortVideo` renamed to `Lyrics`.

## `LyricsEntity` additions

```csharp
/// <summary>
/// Cached view count. Incremented by counted view events only.
/// </summary>
public int ViewCount { get; private set; }

/// <summary>
/// Cached like count.
/// </summary>
public int LikeCount { get; private set; }

/// <summary>
/// Cached share count.
/// </summary>
public int ShareCount { get; private set; }

/// <summary>
/// Increments the cached view count.
/// </summary>
public void IncrementViewCount() => ViewCount++;

/// <summary>
/// Increments the cached like count.
/// </summary>
public void IncrementLikeCount() => LikeCount++;

/// <summary>
/// Decrements the cached like count, floor at zero.
/// </summary>
public void DecrementLikeCount() => LikeCount = Math.Max(0, LikeCount - 1);

/// <summary>
/// Increments the cached share count.
/// </summary>
public void IncrementShareCount() => ShareCount++;
```

## New entities — direct copies of the `ShortVideo` equivalents

```csharp
/// <summary>
/// Records that a user has liked a lyrics page.
/// Created when a user likes; removed when a user unlikes. Never updated.
/// </summary>
public class LyricsLikeEntity : Aggregate<Guid>
{
    public Guid UserId { get; private set; }
    public Guid LyricsId { get; private set; }
    public LyricsEntity Lyrics { get; private set; } = null!;

    private LyricsLikeEntity() { }

    public static LyricsLikeEntity Create(Guid id, Guid userId, Guid lyricsId)
    {
        return new LyricsLikeEntity { Id = id, UserId = userId, LyricsId = lyricsId, CreatedAt = DateTime.UtcNow };
    }
}

/// <summary>
/// Records that a user (or anonymous visitor) shared a lyrics page.
/// UserId is nullable — anonymous social shares are tracked too.
/// </summary>
public class LyricsShareEntity : Aggregate<Guid>
{
    public Guid? UserId { get; private set; }
    public Guid LyricsId { get; private set; }
    public EnumShareChannel? ShareChannel { get; private set; }
    public LyricsEntity Lyrics { get; private set; } = null!;

    private LyricsShareEntity() { }

    public static LyricsShareEntity Create(Guid id, Guid? userId, Guid lyricsId, EnumShareChannel? shareChannel = null)
    {
        return new LyricsShareEntity
        {
            Id = id, UserId = userId, LyricsId = lyricsId, ShareChannel = shareChannel,
            CreatedAt = DateTime.UtcNow,
        };
    }
}

/// <summary>
/// Raw record of a single lyrics-page view event, kept separately from the cached
/// <c>ViewCount</c> so views can be deduplicated per identity and audited later.
/// Only events flagged <see cref="IsCounted" /> incremented the displayed count.
/// </summary>
public class LyricsViewEventEntity : Aggregate<Guid>
{
    public Guid LyricsId { get; private set; }
    public Guid? UserId { get; private set; }
    public string DedupKey { get; private set; } = string.Empty;
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public bool IsCounted { get; private set; }

    /// <summary>
    /// Total foreground dwell time on the lyrics body, in milliseconds, as reported by the
    /// client. Advisory input to the read-time counting rule — see spec 05.
    /// </summary>
    public int DwellMs { get; private set; }

    /// <summary>
    /// Maximum scroll coverage of the lyrics text (0.0–1.0), as reported by the client.
    /// Advisory input to the read-time counting rule — see spec 05.
    /// </summary>
    public double ScrollDepthRatio { get; private set; }

    public LyricsEntity Lyrics { get; private set; } = null!;

    private LyricsViewEventEntity() { }

    public static LyricsViewEventEntity Create(
        Guid id, Guid lyricsId, Guid? userId, string dedupKey, string? ipAddress, string? userAgent,
        bool isCounted, int dwellMs, double scrollDepthRatio)
    {
        return new LyricsViewEventEntity
        {
            Id = id, LyricsId = lyricsId, UserId = userId, DedupKey = dedupKey,
            IpAddress = ipAddress, UserAgent = userAgent, IsCounted = isCounted,
            DwellMs = dwellMs, ScrollDepthRatio = scrollDepthRatio, CreatedAt = DateTime.UtcNow,
        };
    }
}
```

`DwellMs`/`ScrollDepthRatio` are added here directly (rather than as a spec-05 follow-up
migration) since the entity is new in this spec — spec 05 only adds the *counting logic* that
reads them, not the columns themselves.

## Configurations

```csharp
public class LyricsLikeConfiguration : IEntityTypeConfiguration<LyricsLikeEntity>
{
    public void Configure(EntityTypeBuilder<LyricsLikeEntity> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.LyricsId).IsRequired();
        builder.HasIndex(x => new { x.UserId, x.LyricsId }).IsUnique();
        builder.HasOne(x => x.Lyrics).WithMany().HasForeignKey(x => x.LyricsId).OnDelete(DeleteBehavior.Cascade);
    }
}
```

`LyricsShareConfiguration`/`LyricsViewEventConfiguration` mirror
`ShortVideoShareConfiguration`/`ShortVideoViewEventConfiguration` 1:1 (no unique index on either —
repeat shares and repeat raw view events are both expected and valid; only likes are unique per
user).

## Errors

New `LyricsInteractionErrors` class — kept **separate** from `LyricsErrors`, mirroring how
`ShortVideoInteractionErrors` is a distinct class from `ShortVideoErrors` in this exact codebase,
not folded into it:

```csharp
/// <summary>
/// Lyrics interaction error factory providing simple, readable exception creation.
/// Covers likes and shares on lyrics pages.
/// </summary>
public class LyricsInteractionErrors(LyricsInteractionErrorMessage i18n)
{
    public LyricsInteractionErrorMessage Msg => i18n;

    /// <summary>
    /// Throws when a user attempts to like a lyrics page they have already liked.
    /// </summary>
    public ConflictException AlreadyLiked()
    {
        return new ConflictException(i18n.AlreadyLiked());
    }

    /// <summary>
    /// Throws when a like is not found for the given lyrics page and user.
    /// </summary>
    public BadRequestException LikeNotFound()
    {
        return new BadRequestException(i18n.LikeNotFound());
    }
}
```

`ContentI18n` gains a `LyricsInteraction` property, alongside the existing
`ArticleInteraction`/`ShortVideoInteraction` ones.

## Repository additions

`ILyricsRepository` gains the same five methods `IShortVideoRepository` already has:

```csharp
Task<bool> HasLikedAsync(Guid userId, Guid lyricsId, CancellationToken cancellationToken = default);
Task AddLikeAsync(LyricsLikeEntity like, CancellationToken cancellationToken = default);
Task RemoveLikeAsync(Guid userId, Guid lyricsId, CancellationToken cancellationToken = default);
Task AddShareAsync(LyricsShareEntity share, CancellationToken cancellationToken = default);
Task AddViewEventAsync(LyricsViewEventEntity viewEvent, CancellationToken cancellationToken = default);

/// <summary>
/// Checks whether a counted view event exists for the given dedup key since the given time.
/// </summary>
Task<bool> HasCountedViewSinceAsync(
    Guid lyricsId, string dedupKey, DateTime since, CancellationToken cancellationToken = default);
```

Implementations are copy-paste identical to `ShortVideoRepository`'s equivalents, swapping the
entity/`DbSet` names.

## Route constants

`InteractionsRouteConstants` gains:

```csharp
/// <summary>
/// The base endpoint path for lyrics interaction routes.
/// </summary>
public const string Lyrics = "lyrics";
```

## Use cases — four commands, each mirroring its `ShortVideo` counterpart exactly

| Command | Mirrors | Route |
| --- | --- | --- |
| `PublicLikeLyricsCommand`/`Handler` | `PublicLikeShortVideoHandler` | `POST /api/v1/public/lyrics/{id}/likes` |
| `PublicUnlikeLyricsCommand`/`Handler` | `PublicUnlikeShortVideoHandler` | `DELETE /api/v1/public/lyrics/{id}/likes` |
| `PublicShareLyricsCommand`/`Handler` | `PublicShareShortVideoHandler` | `POST /api/v1/public/lyrics/{id}/shares` |
| `PublicRecordLyricsViewCommand`/`Handler` | `PublicRecordShortVideoViewHandler` | `POST /api/v1/public/lyrics/{id}/views` |

```csharp
public class PublicLikeLyricsHandler(
    ILyricsRepository lyricsRepository, IContentUnitOfWork unitOfWork, ContentI18n i18n
) : ICommandHandler<PublicLikeLyricsCommand, PublicLikeLyricsResult>
{
    public async Task<PublicLikeLyricsResult> Handle(PublicLikeLyricsCommand command, CancellationToken ct)
    {
        LyricsEntity lyrics = await lyricsRepository.GetByIdOrThrowAsync(command.LyricsId, ct);

        bool alreadyLiked = await lyricsRepository.HasLikedAsync(command.UserId, command.LyricsId, ct);
        if (alreadyLiked)
        {
            throw i18n.LyricsInteraction.AlreadyLiked();
        }

        var like = LyricsLikeEntity.Create(Guid.NewGuid(), command.UserId, command.LyricsId);
        await lyricsRepository.AddLikeAsync(like, ct);

        lyrics.IncrementLikeCount();
        lyricsRepository.Update(lyrics);

        await unitOfWork.CommitAsync(ct);
        return new PublicLikeLyricsResult(IsSuccess: true);
    }
}
```

`PublicUnlikeLyricsHandler`, `PublicShareLyricsHandler` follow their `ShortVideo` counterparts with
the same one-to-one substitution. `PublicRecordLyricsViewCommand`/`Handler` are specced fully in
spec 05, which extends this same command with the two read-time fields rather than repeating the
whole dedup-window mechanics here.

Endpoints mirror `PublicLikeShortVideoEndpointV1` exactly: route group
`{Public}/{InteractionsRouteConstants.Lyrics}`, `WithAuthorization(UserRolePolicies.RequireVisitorOnly)`
for like/unlike (an identity is required to own a like), `AllowAnonymous()` for share/view (both
already support a nullable/anonymous actor), `RequireRateLimiting(RateLimitPolicies.ContentBrowsing)`
throughout.

## Migration

```bash
dotnet ef migrations add AddLyricsInteractions \
  --project src/Modules/Content/Content \
  --startup-project src/Api \
  --context ContentDbContext
```

## Task checklist

- [x] `LyricsEntity.ViewCount`/`LikeCount`/`ShareCount` + increment/decrement methods
- [x] `LyricsLikeEntity`, `LyricsShareEntity`, `LyricsViewEventEntity` + configurations
- [x] `LyricsInteractionErrors` (separate class) + `ContentI18n.LyricsInteraction`
- [x] `ILyricsRepository`/`LyricsRepository`: `HasLikedAsync`, `AddLikeAsync`, `RemoveLikeAsync`,
  `AddShareAsync`, `AddViewEventAsync`, `HasCountedViewSinceAsync`
- [x] `InteractionsRouteConstants.Lyrics`
- [x] `PublicLikeLyricsCommand`/`Handler`/`Validator`/`EndpointV1`
- [x] `PublicUnlikeLyricsCommand`/`Handler`/`EndpointV1`
- [x] `PublicShareLyricsCommand`/`Handler`/`EndpointV1`
- [x] `PublicRecordLyricsViewCommand`/`Handler`/`EndpointV1` — base shape here, read-time counting
  rule added in spec 05
- [x] `ViewCount`/`LikeCount`/`ShareCount`/`IsLiked` added to `LyricsSummaryDto` AND
  `LyricsDetailDto` (the flat `LyricsDto` this doc originally described no longer exists — Phase 1
  split it into the Summary/Detail pair). `IsLiked` resolved per-caller in the handler, `false` for
  anonymous — mirrors how `ArticleSummaryDto.IsLiked`/`ArticleDetailDto.IsLiked` are resolved
- [x] Migration `AddLyricsInteractions`
- [x] Integration tests: double-like conflicts, unlike-without-a-like rejects, share/view work
  anonymously, repeat views within the dedup window don't double-count (base mechanics only —
  read-time gating tested in spec 05); like/unlike require authentication

**Verification, 2026-07-31**: `dotnet build` clean; combined with specs 05/06 in the same phase —
376/376 unit, 177/177 integration, zero skips; full suite 6591/6594 unit (3 pre-existing unrelated
skips), 1606/1606 integration. Migration `AddLyricsInteractions` generated but **not applied** to
any database.
