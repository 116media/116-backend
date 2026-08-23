# Implementation Plan — Architecture Audit Remediation

This is the staged, checkbox-driven plan that turns the [architecture audit](../README.md) into
shippable work. **Rules of engagement:**

- Stages are **sequential** — finish and merge one before starting the next (later stages assume earlier
  fixes exist).
- Each stage is **one PR**. The PR title is given at the end of each stage.
- Each stage has its own spec file with the **full code** for every change, a checklist, and a
  verification step. A stage's spec is finalized when the previous stage's PR is merged (so the code
  matches the tree it lands on).
- Every task cites the audit finding it closes (e.g. `[01 §1.2]` = doc 01, section 1.2).
- The [coverage map](#coverage-map--every-finding--its-stage) at the bottom assigns **every**
  audit finding to a stage; a finding with no stage is a planning bug.

## Stage index

- [x] **Stage 1 — Critical security quick wins** → [`stage-01-critical-security-hardening.md`](stage-01-critical-security-hardening.md)
- [x] **Stage 2 — Social-login verification & SSRF** → [`stage-02-social-login-and-ssrf.md`](stage-02-social-login-and-ssrf.md)
- [x] **Stage 3 — Rate-limit partitioning & trusted proxies** → [`stage-03-rate-limits-and-proxies.md`](stage-03-rate-limits-and-proxies.md)
- [x] **Stage 4 — Session revocation, verified signup & account-status enforcement** → [`stage-04-session-revocation-and-account-status.md`](stage-04-session-revocation-and-account-status.md)
- [x] **Stage 5 — Password & OTP security** → [`stage-05-password-and-otp-security.md`](stage-05-password-and-otp-security.md)
- [x] **Stage 6 — Domain state-machine guards, order total & payment proof** → [`stage-06-publication-state-and-order-integrity.md`](stage-06-publication-state-and-order-integrity.md)
- [x] **Stage 7 — Invert localization-in-domain (DomainRuleException sweep)** → [`stage-07-domain-rule-exception-sweep.md`](stage-07-domain-rule-exception-sweep.md)
- [x] **Stage 8 — Atomic engagement counters & audit-trail integrity** → [`stage-08-atomic-engagement-counters.md`](stage-08-atomic-engagement-counters.md)
- [ ] **Stage 9 — Query performance (split queries, no-tracking, indexes, soft-delete, N+1)** → [`stage-09-query-performance.md`](stage-09-query-performance.md)
- [ ] **Stage 10 — Multi-instance readiness (distributed cache, clustered jobs, seeding, migrations)** → [`stage-10-multi-instance-readiness.md`](stage-10-multi-instance-readiness.md)
- [ ] **Stage 11 — CancellationToken, typed configuration & observability** → [`stage-11-cancellation-config-observability.md`](stage-11-cancellation-config-observability.md)
- [ ] **Stage 12 — Public/Admin DTO split & staff-data leak fixes** → [`stage-12-public-dto-split.md`](stage-12-public-dto-split.md)
- [ ] **Stage 13 — Domain-event durability (identity + outbox + transaction boundary)** → [`stage-13-domain-event-durability.md`](stage-13-domain-event-durability.md)
- [ ] **Stage 14 — Storage contracts, file pipeline, architecture tests & packaging** → [`stage-14-storage-contracts-and-packaging.md`](stage-14-storage-contracts-and-packaging.md)
- [ ] **Stage 15 — Domain model hardening (aggregate boundaries, guards, events)** → [`stage-15-domain-model-hardening.md`](stage-15-domain-model-hardening.md)
- [ ] **Stage 16 — API contract & authorization hygiene** → [`stage-16-api-contract-hygiene.md`](stage-16-api-contract-hygiene.md)
- [ ] **Stage 17 — Notifications, email & i18n overhaul** → [`stage-17-notifications-and-i18n.md`](stage-17-notifications-and-i18n.md)
- [ ] **Stage 18 — Project restructure (SharedKernel/BuildingBlocks, layer projects, entity/behavior split)** → [`stage-18-project-restructure.md`](stage-18-project-restructure.md)
- [ ] **Stage 19 — Documentation restructure** → [`stage-19-documentation-restructure.md`](stage-19-documentation-restructure.md)

Stages 15–17 were added after Stage 8 shipped: the original plan left ~35 findings unassigned
(the domain-model structure findings, the API-surface hygiene, and doc 14's notification model).
The restructure and documentation stages stay last — every semantic change lands before the
whole-repo file move, and the docs describe what actually shipped.

---

## Stage summaries (objective · tasks · PR title)

### Stage 1 — Critical security quick wins

Small, isolated, high-urgency fixes with no cross-module surgery. Full code in the stage spec.

- [x] Stop `LoggingDecorator` serializing command payloads (credentials/OTP/tokens) `[01 §1.2 / 08 §2]`
- [x] Sanitize unhandled-exception responses in `DefaultExceptionHandler` (env-gated) + add an
      `OperationCanceledException` strategy `[08 §3]`
- [x] Enforce `PaginatedRequest` page-size clamp in the constructor `[06 §2 / 08 §6]`
- **PR:** `fix(security): stop logging credentials, sanitize errors, clamp page size`

### Stage 2 — Social-login verification & SSRF

- [x] `IExternalTokenVerifier` + Google/Facebook verifiers; change `PublicSocialLoginRequest` to
      `(Provider, IdToken)`; reject unverified email `[07 S1]`
- [x] Add `ProviderSubjectId` to `UserEntity` + unique `(AuthProvider, ProviderSubjectId)` index +
      migration; match subject-id first `[07 S1]`
- [x] `UrlSafetyGuard` (block loopback/private/link-local, non-default ports) wired into
      `FileService.ValidateFileUrl`; disable auto-redirect; stop echoing provider errors `[05 §1]`
- **PR:** `fix(auth): verify social-login provider tokens and block avatar-url SSRF`

### Stage 3 — Rate-limit partitioning & trusted proxies

- [x] Populate `ForwardedHeadersOptions.KnownNetworks` from config; `ForwardLimit = 1` `[08 §20]`
- [x] Partition the 3 rate-limit builders per authenticated subject → client IP; add per-account
      partition for `Authentication`/`Otp`/`PasswordManagement` `[01 §1.1 / 07 S6 / 08 §1]`
- [x] Fail CORS closed when origins empty outside Development; move `UseCors` above the exception handler
      `[08 §8]`
- **PR:** `fix(security): partition rate limits per caller and trust only known proxies`

### Stage 4 — Session revocation, verified signup & account-status enforcement

- [x] `UserTokenStateEntity` markers (`sstamp`/`tver`) + `ISessionRevocationCache` denylist, enforced
      once per request in `OnTokenValidated` `[07 S2]`
- [x] Stop issuing tokens at signup; return "verify email" result `[07 S8]`
- [x] Fold `is_active`/`is_verified` claim checks into `RequireVisitorOnly` (no per-request DB)
      `[06 §5 / 07 S8]`
- [x] Refresh re-checks account state; add `AbsoluteExpiresAt` cap `[07 S11]`
- **PR:** `feat(auth): token invalidation, session revocation and verified signup`

### Stage 5 — Password & OTP security

- [x] PBKDF2 iterations 25k→600k with `v2:` prefix + lazy re-hash; keyed OTP hashing `[07 S10]`
- [x] Consume the OTP on password reset (expiry left at 60; consumption closes it) `[07 S4]`
- [x] Per-account OTP attempt lockout + resend cap `[07 S5]`
- [x] Constant-time login (dummy verify on unknown account); remove `.Produces(404)`; neutralize the
      admin forgot/resend role oracle `[07 S7]`
- **PR:** `fix(auth): strengthen password hashing, OTP consumption and account enumeration`

### Stage 6 — Domain state-machine guards, order total & payment proof

- [x] `ContentPublicationState` transition table; route `Publish/Submit/Approve/Reject/Archive` through
      it; delete the 18 duplicated handler guards `[03 §3]`
- [x] Recalculate the order total when an item is added; make `RecalculateTotalFromItems` private;
      tighten submit guard to `All` `[03 §4]`
- [x] `ContentPaymentEntity.Verify` requires proof; `AttachProof` guards status `[03 §5]`
- [x] `DbUpdateException` → 409 strategy (fixes the like/unlike race 500) `[06 §15]`
- **PR:** `fix(content): guard publication state, order totals and payments`

### Stage 7 — Invert localization-in-domain (full sweep)

- [x] Drop the `errors` parameter from every remaining domain signature, replacing each guard with a
      coded rule exception (`ContentRuleException` et al.) and per-aggregate `IRuleProblemCatalog`
      maps `[03 §6 / 08 §9]`
- [x] Delete the `using _116.*.Application.Shared.Errors` imports from all domain files; move the
      Mailer contract enums into their proper layers
- **PR:** `refactor(domain): remove i18n from the domain via coded exceptions`

### Stage 8 — Atomic engagement counters & audit-trail integrity

- [x] Replace load-mutate-save counters with `ExecuteUpdateAsync` deltas across the 5 engagement
      handlers; clamp in SQL `[04 §1]` (also fixes the audit-trail overwrite `[04 §15]`)
- **PR:** `fix(content): make engagement counters atomic and stop clobbering the audit trail`

### Stage 9 — Query performance

- [ ] `.AsSplitQuery()` on the multi-collection includes; `ExistsAsync` on the interaction paths
      `[04 §2]`
- [ ] Make the 19 tracker-dependent writes explicit, then default `NoTracking` + `.AsTracking()`
      on the write paths `[04 §5]`
- [ ] Global soft-delete query filters + `IgnoreQueryFilters` on the tombstone paths `[04 §4]`
- [ ] `AddContentReadIndexes` migration (CONCURRENTLY) + the two query rewrites `[04 §6]`
- [ ] Batch the N+1 mapper/file lookups `[04 §13 / 06 §4 / 02 §8]`
- [ ] Identity sweep: the 2 discarded role existence loads + the bulk-permission N+1
- **PR:** `perf(content): split queries, no-tracking reads, read indexes and batch lookups`

### Stage 10 — Multi-instance readiness

- [ ] Redis distributed cache + version-key invalidation `[04 §8]`
- [ ] Quartz clustering / advisory-lock jobs `[04 §8]`
- [ ] Idempotent, advisory-locked seeders (fixes the missing `Lyrics` content type); remove or wire
      the dead `IDataSeeder` infra `[04 §9 / 01 §1.13]`
- [ ] Move migrations out of the request pipeline; `EnableMigrations=false` in prod `[04 §10 / 01 §1.12]`
- [ ] EF resilience: `EnableRetryOnFailure` + `CommandTimeout` + pool size `[04 §14]`
- [ ] Fix the Dockerfile's missing Mailer projects `[01 §1.15]`
- **PR:** `fix(infra): make the app safe to run on more than one instance`

### Stage 11 — CancellationToken, typed configuration & observability

- [ ] Thread `CancellationToken` through every endpoint→handler→repository; drop the `= default` on
      `IDispatcher.Send` `[01 §1.5 / 06 §1]`
- [ ] Typed `IOptions` with `ValidateOnStart` for DB/JWT/Cloudinary/CORS/SMTP/Resend
      `[08 §10 / 01 §1.10 / 05 §8]`
- [ ] Health checks + OpenTelemetry + correlation middleware; fix Seq/env labels `[08 §12]`
- [ ] Kestrel/form body limits so the 350 MB upload works; gate Swagger; security headers + HSTS
      `[05 §9 / 08 §4 / 08 §7 / 01 §1.11]`
- [ ] Versioning static-field fix; account-status check cached per request `[01 §1.14 / 07 S12]`
- **PR:** `feat(platform): cancellation tokens, validated config, health checks and headers`

### Stage 12 — Public/Admin DTO split & staff-data leak fixes

- [ ] `Public*Dto` records (no `AuditableDto`) + mappers for the leaking public endpoints `[06 §6]`
- [ ] Drop `Email` from `AuthorInfo`/content `AuthorDto`; mail handlers fetch it separately `[07 S9]`
- **PR:** `fix(api): stop leaking audit, commercial and staff data on public endpoints`

### Stage 13 — Domain-event durability

- [ ] Stable `IDomainEvent.EventId`/`OccurredOn`; `DomainEvent` base stamped in `AddDomainEvent`
      `[01 §1.8]`
- [ ] Per-module domain-event outbox written in `SaveChanges`; clustered replay job; idempotent
      consumers `[01 §1.7 / 02 §13]`
- [ ] `ExecuteInTransactionAsync` unit-of-work; collapse the multi-commit handlers; atomic
      cross-module file+content writes `[04 §7 / 06 §7 / 02 §5]`
- [ ] Mailer outbox: claim-then-send, transactional enqueue `[05 §12 / 05 §13]`
- **PR:** `feat(platform): durable domain events with a per-module outbox and one-commit handlers`

### Stage 14 — Storage contracts, file pipeline, architecture tests & packaging

- [ ] Central Package Management + `Directory.Build.props` `[study 04]`
- [ ] `Core.Contracts` (`IFileStore`/`FileRef`); split the 17-method `IFileRepository`; migrate
      Identity then Content off `Core.csproj`; fix the Api/Core registration mismatch
      `[02 §1 / 02 §4 / 02 §6 / 02 §12 / 05 §6]`
- [ ] Evict avatar/thumbnail/colour/slug from Core; collapse the duplicated `FileEntity → FileDto`
      mapping `[05 §5 / 02 §9]`
- [ ] File pipeline: swap-then-delete, correct `resource_type`, Polly-wrapped Cloudinary, atomic
      upload+write, `FileEntity` invariants `[05 §2 / 05 §3 / 05 §4 / 05 §7 / 05 §10]`
- [ ] `tests/Architecture` NetArchTest rules (boundaries + layers) `[02 §3 / study 02]`
- **PR:** `refactor(core): storage contracts, hardened file pipeline, architecture tests and CPM`

### Stage 15 — Domain model hardening

- [ ] Cross-aggregate navigations → IDs, per aggregate `[03 §2]` (unblocks builder reflection
      `[03 §9]`)
- [ ] Strongly-typed IDs: decide and execute (or record the rejection) `[03 §8]`
- [ ] Review-workflow status guards `[03 §7]`; the missing domain events `[03 §10]`; derive
      `HasLyrics` `[03 §11]`
- [ ] Stop `Update()` clobbering every column `[04 §3]`; per-aggregate repositories `[04 §11]`;
      specification-layer decision `[04 §12 / 06 §14]`; drop unused `IMapper` injections `[06 §16]`
- [ ] Standing rule for anemic entities / state pattern `[03 §12 / 03 §13]`
- **PR:** `refactor(domain): aggregate boundaries, review guards, events and repository shape`

### Stage 16 — API contract & authorization hygiene

- [ ] Page/cap the 22 unbounded list endpoints + the unbounded reads `[06 §3 / 04 §16]`
- [ ] Route relocations + scope group helper `[06 §10 / 08 §14]`; rate-limit policy remap `[06 §11]`
- [ ] Unify per-resource authorization; decide S3 (enforce or cut the permission model)
      `[06 §12 / 07 S3]`
- [ ] Validators + `ProducesValidationProblem`; RFC-7807 validation errors and problem completion
      `[06 §13 / 08 §5 / 08 §11]`
- [ ] Envelope/DELETE semantics; versioning decision `[08 §19 / 08 §13]`; Admin/Public dedup where
      touched `[06 §8]`
- [ ] Dispatcher dispatch cache; per-module `TypeAdapterConfig` `[01 §1.4 / 01 §1.6]`
- **PR:** `fix(api): pagination caps, route/rate-limit/authz hygiene and RFC 7807 completion`

### Stage 17 — Notifications, email & i18n overhaul

- [ ] Message-class model per doc 14; retire the cross-module template enum `[doc 14 / 02 §11]`
- [ ] Recipient-culture rendering `[08 §17]`; template-engine escaping for user content `[05 §11]`
- [ ] POST-backed newsletter confirm/unsubscribe `[05 §14]`
- [ ] Compile-time resource keys; interpolated strings into resources; language-set decision
      `[08 §15 / 08 §18 / 08 §16]`
- [ ] Split the `ContentI18n` god facade along the per-aggregate catalogs `[06 §9]`
- **PR:** `refactor(mailer): message-class notifications, recipient-culture rendering and i18n hygiene`

### Stage 18 — Project restructure

- [ ] Split `Shared` into `SharedKernel` + `BuildingBlocks.{Domain,Application,Infrastructure,Presentation}`
      `[01 §1.9 / 02 §10 / study 08 / 09]`
- [ ] Split each module into `Domain/Application/Infrastructure` (+ `Api`) projects; complete the
      Core→Storage rename `[study 02 / 11 / 13]`
- [ ] Entity/behavior partial split (`Entities/` + `Behaviors/`) `[study 10]`
- [ ] Demote junction/child entities from aggregate roots `[03 §1]`
- [ ] `internal` by default; architecture-test allowlist → 0 `[02 §7]`
- **PR:** `refactor(structure): shared kernel split, layer projects and the storage rename`

### Stage 19 — Documentation restructure

- [ ] `docs/README.md` entry point; truthful `CLAUDE.md`/`ARCHITECTURE.md`; status reconciliation;
      one testing rulebook; conventions `[09 §9.1–9.14 / 02 §2]`
- **PR:** `docs: single entry point, truthful guides and one testing rulebook`

---

## Coverage map — every finding → its stage

`✔ n` = shipped in stage n. Plain numbers = planned stage.

| Doc | Finding → stage |
| --- | --- |
| 01 | 1.1 ✔3 · 1.2 ✔1 · 1.3 ✔1 · 1.4 →16 · 1.5 →11 · 1.6 →16 · 1.7 →13 · 1.8 →13 · 1.9 →18 · 1.10 →11 · 1.11 ✔3(CORS)+→11(rest) · 1.12 →10 · 1.13 →10 · 1.14 →11 · 1.15 →10 |
| 02 | 2.1 →14 · 2.2 →19 · 2.3 →14 · 2.4 →14 · 2.5 →13 · 2.6 →14 · 2.7 →18 · 2.8 →9 · 2.9 →14 · 2.10 →18 · 2.11 →17 · 2.12 →14 · 2.13 →13 |
| 03 | 3.1 →18 · 3.2 →15 · 3.3 ✔6 · 3.4 ✔6 · 3.5 ✔6 · 3.6 ✔7 · 3.7 →15 · 3.8 →15 · 3.9 →15 · 3.10 →15 · 3.11 →15 · 3.12 →15 · 3.13 →15 |
| 04 | 4.1 ✔8 · 4.2 →9 · 4.3 →15 · 4.4 →9 · 4.5 →9 · 4.6 →9 · 4.7 →13 · 4.8 →10 · 4.9 →10 · 4.10 →10 · 4.11 →15 · 4.12 →15 · 4.13 →9 · 4.14 →10 · 4.15 ✔8 · 4.16 →16 |
| 05 | 5.1 ✔2 · 5.2 →14 · 5.3 →14 · 5.4 →14 · 5.5 →14 · 5.6 →14 · 5.7 →14 · 5.8 →11 · 5.9 →11 · 5.10 →14 · 5.11 →17 · 5.12 →13 · 5.13 →13 · 5.14 →17 |
| 06 | 6.1 →11 · 6.2 ✔1 · 6.3 →16 · 6.4 →9 · 6.5 ✔4 · 6.6 →12 · 6.7 →13 · 6.8 →16 · 6.9 →17 · 6.10 →16 · 6.11 →16 · 6.12 →16 · 6.13 →16 · 6.14 →15 · 6.15 ✔6 · 6.16 →15 |
| 07 | S1 ✔2 · S2 ✔4 · S3 →16 · S4 ✔5 · S5 ✔5 · S6 ✔3 · S7 ✔5 · S8 ✔4 · S9 →12 · S10 ✔5 · S11 ✔4 · S12 →11 |
| 08 | 8.1 ✔3 · 8.2 ✔1 · 8.3 ✔1 · 8.4 →11 · 8.5 →16 · 8.6 ✔1 · 8.7 →11 · 8.8 ✔3 · 8.9 ✔7 · 8.10 →11 · 8.11 →16 · 8.12 →11 · 8.13 →16 · 8.14 →16 · 8.15 →17 · 8.16 →17 · 8.17 →17 · 8.18 →17 · 8.19 →16 · 8.20 ✔3 |
| 09 | 9.1–9.14 →19 |
| 10–14 | doc 10 verdict →18 · doc 11 →18/14 · doc 12 →18 · doc 13 →14/18 · doc 14 →17 |

---

## Notes

- Stages 1–8 (**shipped**) were the security/correctness core.
- Stages 9–13 are **platform correctness** — each independently valuable, in dependency order
  (9 query semantics → 10 infra → 11 chores → 12 contracts → 13 durability; 13 needs 10's
  clustered jobs).
- Stage 14 must precede 18: CPM and architecture tests make the project explosion safe.
- Stages 15–17 are the **semantic** debt; deliberately before 18 so behaviour changes never hide
  inside the whole-repo file move.
- Stage 18 is the move; Stage 19 documents what shipped. Doc updates still happen continuously —
  19 is the restructure, not the only writing.
