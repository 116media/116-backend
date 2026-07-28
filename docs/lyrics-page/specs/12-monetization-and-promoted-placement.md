# Spec 12 — Monetization: Advertising, Streaming Affiliate & Promoted Placement

Scoped down deliberately to three streams — advertising, streaming-affiliate revenue, and
label/artist-paid promoted placement. **Premium subscriptions, a per-artist revenue-share ledger,
and data/API licensing are out of scope**: subscription billing and per-view creator payouts assume
payment/banking infrastructure (recurring card billing, cross-border payouts) that isn't reliably
available across this platform's actual markets in Africa, so building that data model now would be
speculative. If that changes later, it's a new spec, not a revival of this one.

Depends on spec 01's `CategoryId`/`CustomerId`/`OrderItemId`/`CreateFree`/`CreatePaid` additions to
`LyricsEntity` — this spec only adds the promotion fields and wires the existing Commerce
verification flow to them.

## 1. Advertising — no backend work

The existing ad-serving infrastructure (already used on articles/videos) extends to `/lyrics` and
`/lyrics/{slug}` as ordinary display placements. Nothing new to design or build server-side beyond
what already serves ads elsewhere — the only work is frontend layout (reserving ad slots in the
page composition, [../../../frontend/docs/lyrics-page/06-musixmatch-scale-expansion.md](../../../frontend/docs/lyrics-page/06-musixmatch-scale-expansion.md)).

## 2. Streaming affiliate revenue — already fully specced (spec 09)

The "Go to album" modal's platform links are already built in
[09-streaming-links-and-album-tracks.md](09-streaming-links-and-album-tracks.md) — `ResolveStreamingLinks`
returns a curated-or-generated URL for each of the four platforms. Turning those into affiliate
links is a **URL-parameter change at the point they're constructed**, not a new endpoint or table:

```csharp
private static string GenerateSearchUrl(EnumStreamingPlatform platform, string query) => platform switch
{
    EnumStreamingPlatform.Spotify => $"https://open.spotify.com/search/{query}",
    // + an affiliate/referral query param once each platform's program is confirmed active,
    // e.g. "&aff_id={AffiliateConfig.SpotifyPartnerId}" — added here, nowhere else.
    ...
};
```

A curated `StreamingLinkEntity.Url` (spec 09) can already contain a full affiliate URL as-entered
by an admin — no schema change needed there either, since it's stored as a plain string. **Before
enabling this**: confirm each platform's affiliate/referral program is still active and available
in this platform's markets — this is a legal/business verification step, not a technical one, and
is not gated on anything in this codebase.

## 3. Label & artist licensing / promoted placement — reuses the existing Commerce module

This is **not new commerce infrastructure** — `ContentOrderEntity` → `ContentOrderItemEntity` →
`ContentItemTierEntity` → `ContentPaymentEntity`, `PromotionLevelEntity`, `CustomerEntity`, and
`AdminVerifyPaymentFactory`'s stamping logic (`Application/Commerce/UseCases/Admin/Commands/VerifyPayment/AdminVerifyPaymentFactory.cs`)
already do this exact job for articles and videos today. Lyrics get the identical treatment — same
category model (any number of lyrics categories, each independently free or paid via
`CategoryEntity.IsFree`), same `CreateFree`/`CreatePaid` split (spec 01), same verification handler.

### `EnumCoreContentType` gains `Lyrics`

```csharp
public enum EnumCoreContentType
{
    Article,
    Video,
    Short,
    Custom,
    Lyrics,
}
```

`ContentTypeSeeder` gains `nameof(EnumCoreContentType.Lyrics)` in its seeded list, exactly like
`Article`/`Video`/`Short` — no special-cased seeding logic. Once seeded, an admin creates lyrics
categories under it through the **same category CRUD** articles/videos already use — no new
admin endpoint. There is no fixed number or fixed set of lyrics categories: an admin might create
a free "Standard Lyrics" category (the default, used for the overwhelming majority of songs —
admin-entered, community-submitted, verified-artist self-uploads) and one or more paid categories
("Promoted Lyrics", "Sponsored Placement", etc.) for commercial products, exactly the same
free/paid mix articles have ("Chronique Sale" free vs. "Artist Profile" paid).

One of the seeded free categories needs to be resolvable programmatically — community submissions
(spec 11) and a verified artist's own self-upload never ask anyone to pick a category, so the
approval/upload handler needs a default to assign automatically:

```csharp
/// <summary>
/// Resolves the id of the default free category new, uncommissioned lyrics pages are
/// assigned to (community-submitted, verified-artist self-uploads). Configured via the
/// seeded "Standard Lyrics" category's well-known id, not a magic lookup by name.
/// </summary>
Task<Guid> GetDefaultLyricsCategoryIdAsync(CancellationToken cancellationToken = default);
```

Added to the existing `ICategoryRepository` — a thin wrapper returning a configured id (an
`appsettings` value or a `ContentConstants`-style constant holding the seed guid), not a
name-based lookup (matching this codebase's general avoidance of matching business-meaningful rows
by display name rather than id).

### Two ways a lyrics page ends up paid — both already covered by spec 01

1. **Commissioned as paid from the start** — a label pays to have a brand-new song entered
   specifically as sponsored content. `AdminCreateLyricsHandler` calls `LyricsEntity.CreatePaid(...)`
   (spec 01 §0), exactly like `AdminCreateArticleHandler` does for a paid article — `CustomerId`/
   `OrderItemId` are set at creation, and the record follows `Draft → PendingPayment → …` from day
   one.
2. **An existing free lyrics page retroactively promoted** — a label wants to boost a song that's
   already published and free. `ArticleEntity.Update(...)` already accepts `customerId`/
   `orderItemId` parameters for exactly this case (an existing free article can be converted to a
   paid/promoted one via an edit + a new order) — `LyricsEntity.Update(...)` (the existing
   content-editing method) gains the identical two nullable parameters:

```csharp
/// <summary>
/// Updates the lyrics content and commerce fields in a single call. Fields intentionally
/// excluded: Status (dedicated transition methods), AuthorId (immutable), interaction
/// counters (event-driven). Passing customerId/orderItemId on a previously-free lyrics
/// page retroactively links it to a new commerce order — the same mechanism
/// ArticleEntity.Update already supports for converting free content to paid.
/// </summary>
public void Update(
    Guid categoryId, string songTitle, string artistName, string lyricsText, string language,
    Guid? videoId, Guid? customerId, Guid? orderItemId, LyricsErrors errors)
{
    ValidateRequiredFields(songTitle, artistName, lyricsText, errors);
    CategoryId = categoryId;
    SongTitle = songTitle;
    ArtistName = artistName;
    LyricsText = lyricsText;
    Language = language;
    VideoId = videoId;
    CustomerId = customerId;
    OrderItemId = orderItemId;
}
```

Both paths converge on the same downstream mechanism: whatever `OrderItemId` ends up set on a
`LyricsEntity`, `AdminVerifyPaymentFactory` finds it via `GetByOrderItemIdAsync` at verification
time — no separate "link an existing song" endpoint needed, since `Update()` already does that job
generically (it's how every other field on an existing lyrics page gets changed too).

### `LyricsEntity` additions — the promotion fields specific to this spec

`CategoryId`/`CustomerId`/`OrderItemId` are already added in spec 01; this spec adds the promotion
flag pair, mirroring `ArticleEntity`/`VideoEntity` exactly:

```csharp
/// <summary>
/// Whether this song has an active paid "Top Lyrics" promoted placement.
/// </summary>
public bool IsPromoted { get; private set; }

/// <summary>
/// When the paid promotion expires. Null if never promoted.
/// </summary>
public DateTimeOffset? PromotedUntil { get; private set; }

/// <summary>
/// Activates promoted placement until the given date. Called by
/// AdminVerifyPaymentFactory only, mirroring ArticleEntity.StampPromotion exactly.
/// </summary>
public void StampPromotion(Guid promotionLevelId, DateTimeOffset until)
{
    IsPromoted = true;
    PromotedUntil = until;
}

/// <summary>
/// Force-removes an active promotion. SuperAdmin only, mirrors ArticleEntity.ForceUnpromote
/// (audit fields UnpromotedAt/UnpromotedBy/UnpromotedReason copied verbatim, omitted here
/// for brevity).
/// </summary>
public void ForceUnpromote(string unpromotedBy, string reason)
{
    if (!IsPromoted)
    {
        throw new BadRequestException("Lyrics page is not currently promoted.");
    }

    IsPromoted = false;
    PromotedUntil = null;
}
```

`PromotionLevelId` itself is **not** stored on `LyricsEntity` — `ArticleEntity`/`VideoEntity` don't
store it either; it lives on `ContentOrderItemEntity.PromotionLevelId` and is only consulted at
verification time, exactly as today.

### Repository & verification wiring

```csharp
// ILyricsRepository — one new method, same shape as IArticleRepository's equivalent
Task<LyricsEntity?> GetByOrderItemIdAsync(Guid orderItemId, CancellationToken cancellationToken = default);
```

```csharp
// AdminVerifyPaymentFactory.VerifyAsync — one more lookup added to the existing loop,
// no change to the existing article/video branches
LyricsEntity? lyrics = article is null && video is null
    ? await lyricsRepository.GetByOrderItemIdAsync(orderItemId: item.Id, cancellationToken: cancellationToken)
    : null;

if (item.PromotionLevelId.HasValue)
{
    PromotionLevelEntity promoLevel = await lookupRepository.GetPromotionLevelByIdOrThrowAsync(
        id: item.PromotionLevelId.Value, cancellationToken: cancellationToken);

    DateTimeOffset promotedUntil = DateTimeOffset.UtcNow.AddDays(promoLevel.DurationDays);
    article?.StampPromotion(promotionLevelId: promoLevel.Id, until: promotedUntil);
    video?.StampPromotion(promotionLevelId: promoLevel.Id, until: promotedUntil);
    lyrics?.StampPromotion(promotionLevelId: promoLevel.Id, until: promotedUntil);
}

lyrics?.MarkPendingReview();

if (lyrics is not null)
{
    lyricsRepository.Update(lyrics: lyrics);
}
```

`lyrics?.MarkPendingReview()` mirrors `article?.MarkPendingReview()`/`video?.MarkPendingReview()` in
the existing handler exactly — a newly-commissioned paid lyrics page moves out of
`PendingPayment` into editorial review the same way a paid article does once its payment is
verified. For a *retroactively* promoted, already-published lyrics page, this call is a no-op
(`MarkPendingReview()` only transitions from `PendingReview`/`Draft`-adjacent states — see spec 01;
a `Published` record calling it simply returns `false` and changes nothing, same as the existing
article/video behavior when payment verification runs against already-published content that's
merely being re-promoted).

### Frontend: never blended into organic ranking

Already established in spec 13 — `LyricsEntity.IsPromoted` never appears in the "Top Lyrics" sort
switch. A promoted record renders in its own visually distinct, clearly-labeled slot the frontend
composes separately.

## Migration

```bash
dotnet ef migrations add AddLyricsPromotedPlacement \
  --project src/Modules/Content/Content \
  --startup-project src/Api \
  --context ContentDbContext
```

## Task checklist

- [x] `EnumCoreContentType.Lyrics` + `ContentTypeSeeder` updated
- [x] Admin can create lyrics categories (free or paid) through the existing category CRUD —
  no new endpoint needed; confirmed the existing admin category form accepts the new
  `Lyrics` content type
- [x] Seed a designated default free lyrics category, resolved via
  `ICategoryRepository.GetDefaultLyricsCategoryAsync()` — shipped as a boolean marker
  (`CategoryEntity.IsDefaultForLyrics`, with `MarkAsDefaultForLyrics()`/`UnmarkAsDefaultForLyrics()`)
  following the exact existing `IsGossip`/`GetGossipCategoryAsync` precedent, NOT the hardcoded
  config-id lookup this doc originally sketched (consumed by spec 11's community-submission
  approval and verified-artist self-upload paths, once that spec lands)
- [x] `LyricsEntity.Update(...)` already had `customerId`/`orderItemId` parameters going back to
  Phase 1 (spec 01) — this checklist item was already satisfied before this phase started, no
  change needed here
- [x] `LyricsEntity`: `IsPromoted`, `PromotedUntil`, `StampPromotion`, `ForceUnpromote` (plus the
  `UnpromotedAt`/`UnpromotedBy`/`UnpromotedReason` audit trio, included for real rather than
  omitted "for brevity" as this doc's own snippet did — full parity with `ArticleEntity`)
- [x] `ILyricsRepository.GetByOrderItemIdAsync`
- [x] `AdminVerifyPaymentFactory.VerifyAsync` extended with the lyrics branch, including the
  `MarkPendingReview()` call (no change to its existing article/video branches)
- [x] `POST /api/v1/admin/lyrics/{id}/unpromote` (SuperAdmin, mirrors the existing
  article/video force-unpromote endpoints)
- [x] Migration `AddLyricsPromotedPlacement`
- [x] Integration tests: creating a lyrics page via `CreatePaid` and verifying its order's payment
  stamps `IsPromoted`/`PromotedUntil` and calls `MarkPendingReview()`; updating an existing
  `Published`, free lyrics page with a new `customerId`/`orderItemId` then verifying that order's
  payment stamps promotion without disturbing its `Published` status; an unlinked lyrics record
  (`OrderItemId` null) is untouched by verification; force-unpromote clears the flag; `IsPromoted`
  never appears in the "Top Lyrics" sort query (spec 13's guard test covers this)
- [ ] Confirm each streaming platform's affiliate program status (spec 09's `GenerateSearchUrl`)
  before adding any affiliate query parameter — a business/legal check, not a code checklist item,
  intentionally left unchecked pending that confirmation. Remaining work once confirmed: an
  `AffiliateConfig`-style constants class holding each platform's real partner/affiliate id (none
  exist yet, since there's nothing to configure until this is confirmed), and one query-param
  line per platform branch in `StreamingLinkFactory.GenerateSearchUrl`
  (`src/Modules/Content/Content/Application/Editorial/Factories/StreamingLinkFactory.cs`) appending it — a
  few lines of code, not a redesign.

**Bug caught and fixed during verification (affected Article/Video too, not just Lyrics)**:
`MarkPendingReview()` only guarded against being already `PendingReview` — it had no guard against
`Published`. The retroactive-promotion integration test above caught it directly: promoting an
already-`Published` free lyrics page silently regressed its status back to `PendingReview`,
un-publishing a live page. Fixed by also treating `Published` as a no-op state in
`LyricsEntity.MarkPendingReview()` — and, since `ArticleEntity`/`VideoEntity` shared the exact same
latent bug (confirmed by reading their real code, not assumed), fixed both there too with matching
regression tests, out of an abundance of correctness even though this phase's scope was Lyrics only.

**Verification, 2026-07-31**: `dotnet build` clean; combined with specs 09/13 in the same phase —
634/635 unit (1 pre-existing unrelated skip), 265/265 integration, zero failures; full suite
6527/6530 unit, 1567/1567 integration (includes the Article/Video `MarkPendingReview` regression
tests).
