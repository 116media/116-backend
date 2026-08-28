# 05 — Core & Mailer Modules

Scope: `src/Modules/Core/Core` (file management / Cloudinary) and `src/Modules/Mailer/Mailer`
(+ `Mailer.Contracts`), plus every place they are consumed.

Mailer is the stronger of the two — a genuinely well-built email outbox with a clean
`IEmailSender` port and a correctly-shaped contracts project. Core is the weaker: a "file
module" that has absorbed avatar, thumbnail, colour and slug concerns, wraps Cloudinary with
no resilience, and orders its replace/upload flows so a failure destroys the old asset before
the new one is confirmed. The single most severe finding is a **critical SSRF** reachable from
an anonymous endpoint through Core's downloader.

---

## 5.1 Unauthenticated SSRF: an attacker-supplied URL is fetched by the server through Core's `FileService`

**Severity: Critical** · related to [07 S1](07-identity-and-security.md) (the same endpoint's
account-takeover flaw).

**Where:** `PublicSocialLoginEndpointV1.cs:22` — the anonymous request record carries
`string? AvatarUrl`. `PublicSocialLoginAuthFactory.cs:44` → `UpdateAvatarUrlFromSourceAsync`
→ `FileRepository.cs:140` `fileService.DownloadFileAsync(avatarUrl, ...)` →
`FileService.cs:172-213` (HEAD, ranged GET, streaming GET to the URI). The only guard
(`ValidationUtils.cs:15`) checks the scheme is http/https. `FileService.cs:138` reflects the
failure text back: `throw FileDownloadFailed(fileUrl, ex.Message)`.

**Problem/why.** `POST /api/v1/public/auth/social-login` with `AvatarUrl =
http://169.254.169.254/latest/meta-data/...` or `http://116_db:5432/` makes the API dial it
from inside the perimeter. The echoed exception turns it into an internal port/host scanner.
The URL is then persisted as `StorageUrl` and served as the user's avatar (arbitrary
third-party content under a user identity), with no redirect cap (default 50 hops).

**Solution.**
1. Add `UrlSafetyGuard` in Core: resolve the host with `Dns.GetHostAddressesAsync`, reject
   loopback/link-local/private/ULA/multicast and non-default ports, reject non-https outside
   Development.
2. Configure the `IFileService` HttpClient with `AllowAutoRedirect = false`, a 5s connect and
   10s total timeout; handle 3xx manually, re-running the guard per hop.
3. Call the guard from `FileService.ValidateFileUrl` so every `DownloadFileAsync` is covered.
4. Stop reflecting provider text — log `reason`, return a generic localized message.
5. Longer term: stop trusting `AvatarUrl` from the client at all — read the avatar from the
   verified provider token ([07 S1](07-identity-and-security.md)).

---

## 5.2 Every Cloudinary delete is hard-coded to `resource_type=image`, so videos and PDFs leak forever

**Severity: High**

**Where:** `FileService.DeleteFileAsync` → `CloudinaryService.DeleteImageAsync` →
`new DeletionParams(publicId)` leaving `ResourceType` at its `Image` default; the batch path
is explicitly `ResourceType = ResourceType.Image`. But assets are uploaded as `VideoUploadParams`
and `RawUploadParams` (PDF). All cleanups route through `DeleteFileAsync`. Failure is swallowed
(logged, returns `false`, discarded).

**Problem/why.** Destroying a video's public id with `resource_type=image` returns "not
found". Every short-video replacement/deletion permanently orphans up to 350 MB; every
rejected payment-proof PDF orphans up to 5 MB. Nothing surfaces — the row flips to
`is_deleted`, so the asset is unreachable from the DB and unfindable by key. Storage cost grows
monotonically with no audit trail.

**Solution.** Add `EnumStorageResourceType { Image, Video, Raw }` and a `StorageResourceType`
column on `FileEntity`, set at creation from the upload path (migration backfilled from
`MimeType`). Carry it on the delete/replace events. Change `DeleteAsync` to take the resource
type. Make `FileAssetCleanupHandler` log at Error on a `false` return so the leak is
observable. One-off reconciliation script over `is_deleted AND storage_key IS NOT NULL`.

---

## 5.3 Replace flows delete the old asset before the new upload succeeds

**Severity: High** · overlaps [02 §5](02-module-boundaries.md).

**Where:** `FileRepository.ReplaceImageFileAsync` (`:361`) calls `MarkReplacedByIdAsync`
(commits the soft-delete, raises `FileReplacedEvent` → the interceptor awaits the Cloudinary
delete inline) *before* `UploadAndStoreImageFileAsync`. Same in `ReplaceVideoFileAsync` and
`UpdateAvatarFromFileAsync`. Validation that can reject the new file runs after the delete.

**Problem/why.** Order is: commit old row deleted → delete old remote asset → upload new. If
the new upload fails (Cloudinary 5xx, size/extension rejection, cancellation, the 30 MB Kestrel
cap of §5.9), the caller gets a 4xx/5xx while the old avatar/cover/thumbnail is *already gone*.
`AdminUploadVideoThumbnailHandler` never reaches its commit, so `ThumbnailFileId` still points
at a now-deleted row with a dead asset.

**Solution.** Invert to upload-then-retire: upload first, only on success call
`MarkReplacedByIdAsync`. This needs distinct storage keys per version (change the `publicId` to
`{entityId}/{Guid}` and drop `Overwrite = true`) so the new upload doesn't overwrite the old
asset in place. Apply to the three replace methods + `UpdateAvatarFromUrlAsync`.

---

## 5.4 Upload and DB write are not atomic, `ICoreUnitOfWork` is bypassed, and cleanup doesn't close the gap

**Severity: High** · shared with [04 §7](04-content-infrastructure.md), [02 §5](02-module-boundaries.md).

**Where:** `FileRepository.cs:296-324` — upload, then create, then `SaveChangesAsync`, no
compensation. `FileRepository` contains 9 `SaveChangesAsync` calls; `ICoreUnitOfWork` has 0
consumers outside its own declaration. `AdminUploadVideoThumbnailHandler` commits `core.files`
then `content.videos` in separate transactions. `FileAssetCleanupHandler` only reacts to
replace/soft-delete events — i.e. rows that already exist.

**Problem/why.** Two orphan classes, neither covered: (a) upload succeeds, `SaveChanges` throws
(unique filename collision, drop) → Cloudinary asset with no row and no event; (b) the file
row commits, the Content commit fails → file row + asset referenced by nothing. The handler's
doc claims "a storage failure can only orphan a remote asset, never corrupt the row state" —
true for the retire path it covers, silent on the create path where the orphans are produced.

**Solution.** Wrap the upload in try/catch inside `FileRepository`: on any post-upload
exception, `DeleteFileAsync(uploadResult.PublicId, type)` and rethrow. Remove the 9
in-repository `SaveChangesAsync` and the interface member; callers commit through their own
UoW. Add an `AbandonedFileRowCleanupJob` (Core has no job today; the `AddScheduledJob`
mechanism exists) reaping unreferenced `core.files` rows older than 24h. See [02 §5](02-module-boundaries.md)
for the `ConfirmedAt` variant that ties this to the module boundary.

---

## 5.5 Core is not a file module: avatar, thumbnail, colour and slug concepts have all leaked in

**Severity: High** · this is the domain-modelling half of the Core boundary problem; see
[02 §1/§6](02-module-boundaries.md) for the project-reference half.

**Where:** `IFileRepository` declares 20 members; **6 are avatar-specific**
(`GetAvatarFileAsync`, `UploadAndStoreAvatarAsync`, `DownloadAndStoreAvatarFromUrlAsync`,
`UpdateAvatarFromUrlAsync`, `UpdateAvatarFromFileAsync`, `UpdateAvatarUrlFromSourceAsync(...,
string userId, bool isAvatarSourceManual, ...)` — Identity's `EnumAvatarSource` policy). Other
leaks: `folder: "avatars"` hard-coded in Core (all other folders are caller-passed);
`FileEntity.DominantColorHex`/`ForegroundColorHex` (frontend presentation state on a file
aggregate); `SlugHelper` in Core used only by two Content tag handlers;
`FileIsValidAvatarSpecification` (0 call sites); `FileConstants.MaxAvatarFileSizeBytes` used as
the limit for *every* image upload, so an article cover is validated against the avatar policy.

**Problem/why.** Core cannot be reasoned about, tested, or replaced independently — adding a
second avatar-like concept means a seventh method on a 20-method repository. Five of the 20 are
dead as a public contract (0 foreign call sites). And because avatar limits are the de facto
generic limits, a 3 MB article cover is rejected with an avatar-worded error.

**Solution.** Core keeps one job — store bytes, return a keyed row. Reduce `IFileRepository` to
~9 storage-neutral members taking a `StorageTarget` (public id, folder, resource type, upload
policy). Move the 6 avatar methods into an Identity `UserAvatarService`. Move `SlugHelper` to
Content (or `Shared`); delete the dead specification. Move the colour columns onto the Content
consumers that render them (or at minimum make extraction opt-in). Split `FileConstants` avatar
limits into per-target policies.

---

## 5.6 Core has no contracts project, so 116 files bind directly to its internals

**Severity: High** · same finding as [02 §1](02-module-boundaries.md), from the Core side.

**Where:** no `Core.Contracts.csproj`; `Identity.csproj:21`/`Content.csproj:21` reference
`Core.csproj`. 116 files outside Core carry `using _116.Core.*` (109 `Repositories`, 47
`Domain.Entities`, 6 `DTOs`, 2 `Services`, 2 `Helpers`). Content reaches past the repository
into `ICloudinaryService` (2 files) — it knows the storage vendor by name.

**Solution.** The sequenced extraction is in [02 §1](02-module-boundaries.md). The Core-specific
notes: step 2 defines `IAssetStorage` in `Core.Contracts` and makes `ICloudinaryService`
internal so Content stops naming the vendor; do not attempt the 87-file read-migration before
§5.5 has shrunk `IFileRepository` to 9 members, or you migrate onto an interface you are about
to cut. Realistically 4 PRs, ~130 file touches.

---

## 5.7 The Cloudinary integration has no timeout, no retry, no circuit breaker, no `IHttpClientFactory`

**Severity: High**

**Where:** `CloudinaryService` constructs the SDK by hand per scope (`new Cloudinary(account)`)
owning its own internal `HttpClient`; registered `AddScoped`, not `AddHttpClient`; `Api.Timeout`
never set. `CloudinaryService.cs:220` is sync-over-async on the PDF path (`Task.Run(() =>
_cloudinary.Upload(...))` — the token only cancels scheduling). `DestroyAsync`/
`DeleteResourcesAsync` drop the token. The `IFileService` client has no `ConfigureHttpClient`,
so its outbound calls inherit the 100s default. `FileService.cs:205` never disposes the
response. (Contrast Mailer, which sets a 10s timeout.)

**Problem/why.** A slow Cloudinary hangs a request thread indefinitely — no ceiling anywhere on
the upload path. `FileUpload` is a token-bucket policy, so a Cloudinary brownout drains the
thread pool and takes down unrelated endpoints. A transient 502 fails the whole operation with
no retry, and (§5.3) the old asset is already deleted. The undisposed `ResponseHeadersRead`
response holds the socket until GC — under §5.1's SSRF path, trivial connection-pool exhaustion.

**Solution.** Register the SDK's transport through `IHttpClientFactory` with a 30s timeout; add
`Microsoft.Extensions.Http.Resilience` `.AddStandardResilienceHandler()` on that client and the
`IFileService` client (retry 5xx/timeout, not 4xx). Set a 10s timeout on the `IFileService`
client. Replace `Task.Run(_cloudinary.Upload)` with `await _cloudinary.UploadAsync(...,
cancellationToken)`. Pass the token to the delete calls. `using` the request/response in
`FileService`.

---

## 5.8 Cloudinary and mail credentials are never validated at startup; `IsValid()` is dead code

**Severity: Medium** · overlaps [01 §1.10](01-composition-root-and-shared-kernel.md), [08 §10](08-cross-cutting.md).

**Where:** `CloudinaryExtensions.cs:18` coerces missing env vars to `""` and registers the
settings singleton anyway; `CloudinarySettings.IsValid()` has 0 production call sites. Mailer
defers credential checks to send time (`SmtpEmailSender`/`ResendEmailSender` throw on first
send). `SmtpEmailSender.cs:21` silently defaults a production relay to `localhost:1025`,
`SecureSocketOptions.None` unless `SMTP_USE_STARTTLS` parses true.

**Problem/why.** A typo'd `CLOUDINARY_API_SECRET` boots green and fails only on the first
upload, as a generic 502 attributed to Cloudinary. A missing `SMTP_HOST` in production quietly
tries `localhost:1025` **unencrypted** — every outbox row burns all 5 retries over ~15 hours
and lands in `Failed`; nobody completes signup and there is no alert. A missing
`SMTP_USE_STARTTLS` sends credentials in the clear.

**Solution.** Convert both to the options pattern with `.Validate(...).ValidateOnStart()` (this
finally uses `IsValid()`). Add a `MailerSettings` record requiring `UseStartTls == true` and a
non-localhost host outside Development; replace the raw `configuration[...]` reads. Add an alert
on `mailer.outbox_emails WHERE status = 'Failed'` — the outbox records everything needed, nothing
reads it.

---

## 5.9 The 350 MB video limit is unreachable — Kestrel caps the request at ~30 MB

**Severity: Medium** · same as [08 §4 (part)](08-cross-cutting.md).

**Where:** `FileConstants.MaxVideoFileSizeBytes = 350 MB`, enforced at `CloudinaryService.cs:427`.
No Kestrel/form limits configured anywhere (`grep MultipartBodyLengthLimit|MaxRequestBodySize`
→ 0), so the 30 MB Kestrel default applies. The interface doc already drifted to "100 MB limit".

**Problem/why.** Any short-video upload above ~28.6 MiB is killed by Kestrel with a bare 413
before Carter binds the form and before the localized error runs — the client gets an
untranslated framework response. The advertised 350 MB is a lie, and (§5.3) the old video is
already deleted by then.

**Solution.** Set `MaxRequestBodySize`/`MultipartBodyLengthLimit` to the real ceiling, scoped
per-endpoint (`RequestSizeLimitAttribute` on the short-video upload only, leaving the global
default low). At 350 MB, stream via `MultipartReader` into Cloudinary's chunked upload (or a
signed direct-to-Cloudinary upload) rather than buffering the whole body. Fix the 100 MB doc.

---

## 5.10 `FileEntity` has no invariants for the fields that matter and hard-codes presentation state

**Severity: Medium**

**Where:** `FileEntity.Create`'s guard covers 5 fields; `storageKey`, `dominantColorHex`,
`foregroundColorHex` are validated by nothing (`ColorContrastHelper.Normalize` exists and is
never called from the entity). Nothing enforces the `StorageUrl`/`StorageKey` relationship or
the documented "both colours set or neither". No storage-provider/resource-type discriminator.
`Create` takes `CoreI18n`, so the aggregate can't be constructed outside a DI scope.

**Problem/why.** The two invariants that would prevent real incidents are the two missing: a row
with a `StorageKey` must know its resource type (or cleanup no-ops — §5.2), and the colour pair
must be all-or-nothing (a partial write renders invisible text). Meanwhile the entity carries
WCAG-derived presentation data it has no business owning.

**Solution.** Extend the guard: `storageKey` non-empty when provided, colours valid and paired.
Add the `EnumStorageResourceType` from §5.2 and an `EnumStorageProvider` so "no key means
external" is a typed state, not a comment. Move the colour columns onto consumers (§5.5).
Replace the `CoreI18n` ctor parameter with a `Result<FileEntity>` return or a domain service so
the aggregate is constructible in isolation.

---

## 5.11 A user comment containing `{{anything}}` throws during email render and silently drops the notification

**Severity: Medium**

**Where:** `EmailTemplateRenderer.cs:55-66` HTML-encodes but `WebUtility.HtmlEncode` doesn't
touch `{`/`}`; `:72-82` then throws on any surviving `{{...}}`. User text reaches it —
`CommentReplyAddedNotificationsHandler.cs:98` passes `reply.Body`, `article.Title`,
`replierName`. `NotificationRenderer.cs:49` has the same hazard and doesn't encode at all. The
throw is swallowed by `DomainEventPublisher`. Substitution is also order-dependent.

**Problem/why.** Someone replies with `check {{this}} out`; `EnsureFullyResolved` matches
`{{this}}`, throws, the handler aborts before `notifier.NotifyAsync` — so the parent author
gets **neither** email nor in-app row. A trivially discoverable, user-triggered denial of
notifications with no error surface. The order-dependence is a lesser template injection (a
display name of `{{otpCode}}` can splice another token's value).

**Solution.** Do substitution in one pass over the *template* with a regex replace (injected
`{{...}}` in a value becomes inert by construction, ordering dependence gone). Run
`EnsureFullyResolved` against the template before substitution. Encode `{`/`}` in the HTML
branch. Apply to both renderers.

---

## 5.12 The outbox dispatcher holds a Postgres transaction and row locks across up to 20 network sends

**Severity: Medium**

**Where:** `OutboxEmailDispatcherJob.cs:45-68` opens a transaction, `ClaimDueBatchAsync` (`FOR
UPDATE SKIP LOCKED`), then `foreach` delivers all 20 (SMTP connect/auth/send or Resend POST),
then one `SaveChanges` + commit. `SmtpEmailSender` has no timeout; batch size 20, cron every
15s, `[DisallowConcurrentExecution]`.

**Problem/why.** Worst case the transaction stays open for 20 × (SMTP timeout) — unbounded on
SMTP. For that window one pooled connection is pinned, 20 rows locked, and Postgres cannot
vacuum past the snapshot. A slow provider stalls the whole queue. A crash mid-batch rolls back
every `MarkSent`, so already-delivered emails re-send on the next run with no provider-side
idempotency key.

**Solution.** Split claim from deliver: `ClaimDueBatchAsync` becomes an atomic `UPDATE … SET
status='Claimed' … RETURNING` committed immediately (add a `Claimed` state + a reaper for stuck
rows). Deliver outside any transaction, persisting each row's outcome individually. Give
`SmtpEmailSender` a 10s timeout and reuse one connection per batch. Send `email.Id` as
`Idempotency-Key` to Resend.

---

## 5.13 Enqueue is a second transaction, not a transactional outbox — the verification email can be lost

**Severity: Medium**

**Where:** `PublicSignUpAuthFactory.cs:70-83` commits signup, *then* `mailer.EnqueueAsync`;
`OutboxMailer.cs:47` commits on a separate context. All contexts share one database. The
request's `cancellationToken` is passed to `EnqueueAsync`. Same shape in the two newsletter
handlers.

**Problem/why.** The window between the two commits is unprotected. A crash, eviction, or
client abort there leaves the user + OTP rows written but **no outbox row** — nothing to retry,
nothing in `mailer.outbox_emails`, no reconciliation. The user is created, cannot verify, and
can only escape via `ResendOtp`. Affects all 30 `EnqueueAsync` sites including security alerts.

**Solution.** Cheap first fix: pass `CancellationToken.None` to `EnqueueAsync` (or drop the
parameter), eliminating the most frequent case (client disconnect) — the domain-event
interceptor already made exactly this call. Real fix: since all contexts target one database,
enlist `MailerDbContext` in the caller's transaction and enqueue *before* the business commit so
both land atomically (touches all 22 enqueuing files; sequence after §5.12). Add a
reconciliation job for unverified users with no outbox row.

---

## 5.14 Newsletter confirm and unsubscribe are `GET`s with side effects — link scanners trigger both

**Severity: Medium**

**Where:** `PublicConfirmNewsletterEndpointV1` and `...Unsubscribe...` are `MapGet` on
`{token}`, `.AllowAnonymous()`. The confirm GET flips state *and* enqueues an email. Tokens
travel in email HTML links and never expire.

**Problem/why.** Corporate mail security (Outlook Safe Links, Proofpoint) prefetches every URL.
Because both mutate on GET: double opt-in is defeated (a scanner marks the address subscribed
and fires the welcome email before the human clicks — a false GDPR/CAN-SPAM consent record),
and unsubscribes fire silently. HTTP requires GET to be *safe*, not merely idempotent — the doc
comment conflates the two.

**Solution.** Keep the GETs read-only (resolve the token, return state for the frontend page).
Add `POST /confirm` and `POST /unsubscribe` taking the token in the body as the only mutating
paths. Give the confirmation token a 48h expiry. Add `List-Unsubscribe-Post` (RFC 8058) so mail
clients use one-click POST.

---

## What is done well here

- **The Mailer outbox is genuinely well built** — `OutboxEmailEntity` is self-contained
  (subject + both bodies rendered at enqueue, so template changes never corrupt queued rows), a
  real transient/permanent split with a backoff schedule, `FOR UPDATE SKIP LOCKED` so replicas
  can't double-send, and the right `(Status, NextAttemptAt)` index. The user's request never
  waits on a provider. The single strongest piece of either module.
- **`IEmailSender` is a clean port with two real adapters** — `EmailMessage` omits the sender
  identity "so business code cannot spoof it", retry lives in the caller, `IsTransient`
  classifies 429/5xx correctly, and boot fails on an unknown `EMAIL_PROVIDER`.
- **`Mailer.Contracts` is the right shape** — 6 small types, no EF, no `IFormFile`. The model
  Core should copy.
- **`ColorContrastHelper`** is dependency-free, correct WCAG relative-luminance maths, and
  `ImageColorService` is properly best-effort (returns `null` rather than throwing into the
  upload pipeline).
- **`FileEntity`'s soft-delete modelling is right** — `Delete()`/`MarkReplaced()` both
  no-op-guard on `IsDeleted`, split events keep deletions distinguishable from replacements,
  and the partial unique index lets soft-deleted rows retain their filename.
- **`NewsletterSubscriberEntity` implements double opt-in properly** — 32-byte CSPRNG tokens,
  independent confirm/unsubscribe tokens, idempotent operations, and a subscribe handler that
  returns success on every path so the response can't enumerate subscribers.
- **Mailer's use-case slices match the house conventions exactly**; Core, by contrast, ships 0
  endpoints and 0 CQRS handlers — reinforcing that it is a library, not a module ([02 §12](02-module-boundaries.md)).
