# Spec 11 — Odesli Streaming-Link Resolution

**Enhancement, not a frontend gap.** Removes the per-platform manual entry from streaming-link
curation: the admin pastes **one** verified platform URL, the backend calls Odesli once, and every
other platform's deep link is filled automatically.

**Depends on nothing in specs 01–10.** It extends the streaming-link machinery the lyrics feature
shipped (spec 09 of the lyrics feature) and can land any time.

## The problem being removed

Today a fully-curated release means an admin hand-enters one deep link per platform per release —
four `PUT` calls against `UpsertAlbumStreamingLink` / `UpsertSingleStreamingLink`. The generated
search-URL fallback means nothing is ever *broken* without this work, but exact links are strictly
better and the manual cost per release is why most releases will never get them.

## How other sites solve it, and which way fits us

| Approach | Why / why not |
| --- | --- |
| **Odesli (song.link) API** — one platform URL in, all platforms out | ✅ Chosen. Matches on the catalog's ISRC linkage, not text, so a Kinshasa rapper with 200 streams resolves as reliably as a chart artist. Free tier, no key required at low volume. |
| Per-platform search APIs (Spotify, iTunes, Deezer) | ❌ Text matching on `artist + title` returns covers, karaoke and live versions — a wrong deep link asserts "this is the song" and lands on the wrong one, which is worse than our search fallback. |
| ISRC lookup | ❌ We are not the distributor, so we never hold ISRCs. Odesli uses them internally on our behalf. |

The human stays in the loop exactly once: **finding the correct source link**. That verification is
the part text search cannot do; everything after it is mechanical.

### Why resolution happens at admin time, never at read time

The public endpoints keep serving **stored rows only**. Calling Odesli when a user opens a
discography would add third-party latency to a public page, burn through the free tier
(~10 requests/minute unkeyed) on traffic we don't control, and couple our uptime to theirs. One
call per admin action is the entire outbound footprint.

## The Odesli API

```
GET https://api.song.link/v1-alpha.1/links?url=<encoded platform URL>&userCountry=CD
```

- **No authentication required.** An optional `key` query parameter raises the rate limit; we read
  it from the environment when present and omit it otherwise.
- Response (fields we consume):

```jsonc
{
  "linksByPlatform": {
    "spotify":      { "url": "https://open.spotify.com/album/…" },
    "appleMusic":   { "url": "https://music.apple.com/…" },
    "youtubeMusic": { "url": "https://music.youtube.com/…" },
    "tidal":        { "url": "https://listen.tidal.com/…" },
    "deezer":       { "url": "https://www.deezer.com/…" }
    // + platforms we do not model (amazonMusic, pandora, …) — skipped
  }
}
```

- Works for both **tracks and albums** — the entity type follows from the pasted URL.
- A platform missing from `linksByPlatform` means the release is not in that platform's catalog.
  We store nothing for it, and the generated search-URL fallback keeps covering that slot.

Mapping table, the only place Odesli's platform keys are known:

| Odesli key | `EnumStreamingPlatform` |
| --- | --- |
| `spotify` | `Spotify` |
| `appleMusic` | `AppleMusic` |
| `youtubeMusic` | `YoutubeMusic` |
| `tidal` | `Tidal` |
| `deezer` | `Deezer` *(new — below)* |

Unknown keys are **skipped silently** — Odesli adding a platform must never break resolution.

## `Deezer` joins `EnumStreamingPlatform`

Appended as the fifth member — never reordered; the stored value is the integer. Deezer is the
platform that matters most for francophone Africa, and this feature is the natural moment to add it
because resolution makes a fifth platform free instead of a fifth manual field.

Two knock-on updates ship with the member:

- **`StreamingLinkFactory`** gains a Deezer arm in the generated-search fallback
  (`https://www.deezer.com/search/<encoded artist + title>`), so uncurated releases get a Deezer
  search link like every other platform. Its "always exactly N platforms" tests move from 4 to 5.
- The frontend already skips platforms it does not recognise, so backend-first is safe — same
  contract as `EnumSocialPlatform` ([spec 02](02-artist-social-links.md)).

No migration: the enum is only ever a stored integer on existing tables.

## The port

`Application/Shared/Services/IStreamingLinkResolutionService.cs` — same shape as
`ITranslationService`: the application owns the port, infrastructure owns the provider.

```csharp
/// <summary>
/// Resolves one verified platform URL into deep links across every streaming platform,
/// via an external link-aggregation provider. Called once per admin resolve action —
/// never from a public read path.
/// </summary>
public interface IStreamingLinkResolutionService
{
    /// <summary>
    /// Returns the platform→URL pairs the provider could match for the given source URL.
    /// Platforms the release is not on are simply absent. Throws
    /// <see cref="StreamingLinkResolutionException" /> when the provider is unreachable,
    /// rate-limited, or does not recognise the URL.
    /// </summary>
    Task<IReadOnlyDictionary<EnumStreamingPlatform, string>> ResolveAsync(
        string sourceUrl,
        CancellationToken cancellationToken = default
    );
}
```

Returning a dictionary keyed by **our** enum (not Odesli's strings) keeps the provider vocabulary
out of the application layer — the mapping table lives entirely inside the implementation.

## The implementation

`Infrastructure/Services/OdesliStreamingLinkResolutionService.cs`, registered with
`services.AddHttpClient<IStreamingLinkResolutionService, OdesliStreamingLinkResolutionService>()` —
the exact `YoutubeThumbnailService` precedent, typed `HttpClient` and all.

Unlike the translation service there is **no placeholder**: Odesli is keyless and free, so the real
implementation ships immediately. The placeholder pattern existed because the LLM provider needed
credentials nobody had; that reason does not apply here.

Behaviour:

- Base URL and optional key from configuration: `ODESLI_API_URL`
  (default `https://api.song.link/v1-alpha.1`) and `ODESLI_API_KEY` (optional, appended as `key=`
  only when set). Follows the `.env` convention of every other external service.
- `userCountry=CD` on every request — link availability is region-dependent and our readers are
  the Congolese market.
- Timeout of 10 seconds on the typed client. An admin action may wait that long; it must not hang.
- Non-success status, malformed JSON, or a missing `linksByPlatform` → throw
  `StreamingLinkResolutionException` with the status detail. **429 specifically** is surfaced as
  the rate-limit message so the admin knows to wait a minute, not to retry immediately.
- Only `https` URLs are accepted from the response; anything else is skipped — the same trust
  posture as every other stored outbound URL.

## Errors

New three-layer error set, `StreamingLinkErrors` + `StreamingLinkErrorMessage` + three `.resx`
files, wired into `ContentI18n` like every other domain:

| Member | Exception | Message |
| --- | --- | --- |
| `ResolutionFailed()` | `BadGatewayException` | *The streaming-link provider could not be reached. Try again shortly.* |
| `ResolutionRateLimited()` | `RateLimitExceededException` | *The streaming-link provider is rate-limiting us. Wait a minute and retry.* |
| `UnresolvableSourceUrl` | *message only* — worded onto the validators' 400, no exception factory | *The provider does not recognise this URL. Paste a track or album link from a supported platform.* |
| `NothingResolved()` | `NotFoundException` | *The provider found no other platforms for this release.* |

`BadGatewayException` and `RateLimitExceededException` both already exist in
`Shared/Application/Exceptions` — verified, nothing new to invent.

`StreamingLinkResolutionException` lives in `Application/Shared/Exceptions` — the module's
exception folder, following the Identity module's `Application/Shared/Exceptions` precedent. It is
part of the port contract: implementations throw it, handlers translate it into the localized
errors above, and infrastructure never touches i18n.

## Admin surface

Two commands, mirroring the existing upsert pair one-for-one — albums and standalone singles are
already separate use cases and stay that way:

| Use case | Route |
| --- | --- |
| `AdminResolveAlbumStreamingLinks` | `POST /api/v1/admin/albums/{id}/streaming-links/resolve` |
| `AdminResolveSingleStreamingLinks` | `POST /api/v1/admin/lyrics/{id}/streaming-links/resolve` |

Request body: `{ "sourceUrl": "https://open.spotify.com/album/…" }`.

Handler flow (album variant; the single variant differs only in the parent lookup and the
existing `BelongsToAlbum` guard the manual upsert already applies):

1. `GetByIdOrThrowAsync` — 404 for an unknown album.
2. Validate `sourceUrl`: absolute, `https` — same rules as the social-link validator.
3. `ResolveAsync(sourceUrl)` — one outbound call.
4. If the result is empty, throw `NothingResolved()` — the admin learns nothing was stored rather
   than seeing a silent 200.
5. For each resolved platform: **upsert** through the existing repository methods — create the row
   or replace the URL, identical semantics to the manual endpoint. The pasted source URL's own
   platform is included, so pasting the Spotify link also stores the Spotify link.
6. One `CommitAsync` for the whole batch — the resolve is atomic; a half-stored platform set on
   failure would be worse than none.
7. Return what happened, so the admin UI can show it:

```csharp
public record AdminResolveAlbumStreamingLinksResult(
    IReadOnlyList<EnumStreamingPlatform> Resolved,
    IReadOnlyList<EnumStreamingPlatform> Unresolved   // modelled platforms Odesli had no link for
);
```

**Resolution never deletes.** A platform absent from the response leaves any existing curated row
untouched — the admin may have hand-entered a better link than Odesli knows about, and an outage
must not strip curation. Removal stays the job of the existing `Remove*StreamingLink` endpoints.

Both endpoints: admin-or-superadmin policy, `ProducesValidationProblem`, and — because each call
spends one unit of a shared external quota — `RateLimitPolicies.DataExport` (token bucket) rather
than `ContentBrowsing`, so a misbehaving dashboard cannot drain the Odesli free tier in a loop.

## What this spec deliberately does not do

- **No bulk backfill job.** Resolving the whole catalog in one sweep hits the unkeyed rate limit
  in seconds and turns a curation tool into a queue system. If bulk resolution is ever wanted, it
  is a background job with its own pacing — a separate spec.
- **No automatic resolution on album/lyrics create.** Creation has no source URL to resolve from;
  inventing one via text search reintroduces the wrong-song problem this design exists to avoid.
- **No storage of Odesli's own page URL** (`song.link/…`). We link readers to platforms, not to an
  intermediary's landing page.

## Testing

Per [`../../testing/00-unit-vs-integration-rules.md`](../../testing/00-unit-vs-integration-rules.md).
The external-service stub exception applies: Odesli is stubbed in integration tests exactly as
Cloudinary is — the one kind of mock allowed inside `tests/Integration/`.

- **Unit** — response mapping in `OdesliStreamingLinkResolutionService` (known platforms mapped,
  unknown keys skipped, non-https URLs skipped, missing `linksByPlatform` throws) against a fake
  `HttpMessageHandler`; both handlers with a mocked port (upserts existing rows instead of
  duplicating, empty result throws `NothingResolved`, provider exception becomes the localized
  error, absent platforms leave existing rows alone); validator rejects non-https and relative
  source URLs; `StreamingLinkFactory` emits five platforms including the Deezer search fallback.
- **Integration** — real HTTP against both resolve endpoints with the stubbed resolution service:
  resolved platforms persist as curated rows readable back through the public lyrics detail
  endpoint; a second resolve replaces URLs without duplicating rows; a pre-existing manual row for
  an unresolved platform survives; unknown album/lyrics id 404s; a stub that throws maps to the
  documented error status.

## Checklist

- [x] `EnumStreamingPlatform.Deezer` appended
- [x] `StreamingLinkFactory` Deezer search-fallback arm; platform-count tests updated to 5
- [x] `IStreamingLinkResolutionService` port in `Application/Shared/Services`
- [x] `StreamingLinkResolutionException` in `Application/Shared/Exceptions`
- [x] `OdesliStreamingLinkResolutionService` via typed `HttpClient`, config-driven base URL and optional key, `userCountry=CD`, 10s timeout
- [x] Response mapping: known keys → enum, unknown keys skipped, non-https skipped
- [x] `StreamingLinkErrors` + `StreamingLinkErrorMessage` + three `.resx` files, wired into `ContentI18n`
- [x] `AdminResolveAlbumStreamingLinks` — command, handler, validator, meta field, `EndpointV1`
- [x] `AdminResolveSingleStreamingLinks` — command, handler, validator, meta field, `EndpointV1`, keeping the `BelongsToAlbum` guard
- [x] Batch upsert is atomic — one commit; resolution never deletes existing rows
- [x] Result reports `Resolved` and `Unresolved` platform lists
- [x] `AddHttpClient` registration in `ContentModule`
- [x] `.env.template` gains `ODESLI_API_URL` and `ODESLI_API_KEY` (optional)
- [x] Unit tests per the testing section
- [x] Integration tests per the testing section, Odesli stubbed
- [x] `dotnet build` and both test suites clean
