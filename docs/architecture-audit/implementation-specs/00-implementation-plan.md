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

## Stage index

- [x] **Stage 1 — Critical security quick wins** → [`stage-01-critical-security-hardening.md`](stage-01-critical-security-hardening.md)
- [x] **Stage 2 — Social-login verification & SSRF** → [`stage-02-social-login-and-ssrf.md`](stage-02-social-login-and-ssrf.md)
- [x] **Stage 3 — Rate-limit partitioning & trusted proxies** → [`stage-03-rate-limits-and-proxies.md`](stage-03-rate-limits-and-proxies.md)
- [ ] **Stage 4 — Session revocation, verified signup & account-status enforcement** → [`stage-04-session-revocation-and-account-status.md`](stage-04-session-revocation-and-account-status.md)
- [ ] **Stage 5 — Password & OTP hardening**
- [ ] **Stage 6 — Domain state-machine guards, order total & payment proof**
- [ ] **Stage 7 — Atomic engagement counters & audit-trail integrity**
- [ ] **Stage 8 — Query performance (split queries, no-tracking, indexes, soft-delete, N+1)**
- [ ] **Stage 9 — Multi-instance readiness (distributed cache, clustered jobs, seeding, migrations)**
- [ ] **Stage 10 — CancellationToken, typed configuration & observability**
- [ ] **Stage 11 — Public/Admin DTO split & staff-data leak fixes**
- [ ] **Stage 12 — Domain-event durability (identity + outbox + transaction boundary)**
- [ ] **Stage 13 — Core→Storage contracts, architecture tests & packaging (CPM)**
- [ ] **Stage 14 — Invert localization-in-domain (DomainException)**
- [ ] **Stage 15 — Project restructure (SharedKernel/BuildingBlocks, layer projects, entity/behavior split)**
- [ ] **Stage 16 — Documentation restructure**

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
      partition for `Authentication`/`Otp`/`PasswordManagement` `[01 §1.1 / 07 S6]`
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

### Stage 5 — Password & OTP hardening
- [ ] PBKDF2 iterations 25k→600k with `v2:` prefix + lazy re-hash; separate cheap OTP hasher `[07 S10]`
- [ ] Consume the OTP on password reset; cut expiry to 10 min `[07 S4]`
- [ ] Per-account OTP attempt lockout + resend cap `[07 S5]`
- [ ] Constant-time login (dummy verify on unknown account); remove `.Produces(404)`; neutralize the
      admin forgot/resend role oracle `[07 S7]`
- **PR:** `fix(auth): strengthen password hashing, OTP consumption and account enumeration`

### Stage 6 — Domain state-machine guards, order total & payment proof
- [ ] `ContentPublicationState` transition table; route `Publish/Submit/Approve/Reject/Archive` through
      it; delete the 18 duplicated handler guards `[03 §3]`
- [ ] Recalculate the order total when an item is added; make `RecalculateTotalFromItems` private;
      tighten submit guard to `All` `[03 §4]`
- [ ] `ContentPaymentEntity.Verify` requires proof; `AttachProof` guards status `[03 §5]`
- [ ] `DbUpdateException` → 409 strategy (fixes the like/unlike race 500) `[06 §15]`
- **PR:** `fix(content): guard publication state, order totals and payment verification`

### Stage 7 — Atomic engagement counters & audit-trail integrity
- [ ] Replace load-mutate-save counters with `ExecuteUpdateAsync` deltas across the 5 engagement
      handlers; clamp in SQL `[04 §1]` (also fixes the audit-trail overwrite `[04 §15]`)
- **PR:** `fix(content): make engagement counters atomic and stop clobbering the audit trail`

### Stage 8 — Query performance
- [ ] `.AsSplitQuery()` on the multi-collection includes; `Exists` check on the interaction paths
      `[04 §2]`
- [ ] Default `NoTracking`; `.AsTracking()` on the ~25 write paths `[04 §5]`
- [ ] Global soft-delete query filters + `IgnoreQueryFilters` on the tombstone paths `[04 §4]`
- [ ] `AddContentReadIndexes` migration (CONCURRENTLY) + the two query rewrites `[04 §6]`
- [ ] Batch the N+1 mapper/file lookups `[04 §13 / 06 §4]`
- **PR:** `perf(content): split queries, no-tracking reads, read indexes and batch lookups`

### Stage 9 — Multi-instance readiness
- [ ] Redis distributed cache + version-key invalidation `[04 §8]`
- [ ] Quartz clustering / advisory-lock jobs `[04 §8]`
- [ ] Idempotent, advisory-locked seeders (fixes the missing `Lyrics` content type) `[04 §9]`
- [ ] Move migrations out of the request pipeline; `EnableMigrations=false` in prod `[04 §10 / 01 §1.12]`
- [ ] EF resilience: `EnableRetryOnFailure` + `CommandTimeout` + pool size `[04 §14]`
- **PR:** `fix(infra): make the app safe to run on more than one instance`

### Stage 10 — CancellationToken, typed configuration & observability
- [ ] Thread `CancellationToken` through all 293 endpoints; drop the `= default` on `IDispatcher.Send`
      `[06 §1]`
- [ ] Typed `IOptions` with `ValidateOnStart` for DB/JWT/Cloudinary/CORS/SMTP/Resend `[08 §10 / 01 §1.10]`
- [ ] Health checks + OpenTelemetry + correlation middleware; fix Seq/env labels `[08 §12 / §11]`
- [ ] Kestrel/form body limits for uploads; gate Swagger + security headers + HSTS `[08 §4 / §7]`
- **PR:** `feat(platform): cancellation tokens, validated config, health checks and headers`

### Stage 11 — Public/Admin DTO split & staff-data leak fixes
- [ ] `Public*Dto` records (no `AuditableDto`) + mappers for the 27 leaking public endpoints `[06 §6]`
- [ ] Drop `Email` from `AuthorInfo`/content `AuthorDto`; mail handlers fetch it separately `[07 S9]`
- **PR:** `fix(content): stop leaking admin/commercial fields on public endpoints`

### Stage 12 — Domain-event durability
- [ ] Stable `IDomainEvent.EventId`/`OccurredOn`; `DomainEvent` base stamped in `AddDomainEvent`
      `[01 §1.8]`
- [ ] Per-module domain-event outbox written in `SaveChanges`; replay job `[01 §1.7]`
- [ ] `ExecuteInTransactionAsync` unit-of-work; collapse the multi-commit handlers `[04 §7]`
- **PR:** `feat(platform): durable domain events via transactional outbox`

### Stage 13 — Core→Storage contracts, architecture tests & packaging
- [ ] `Central Package Management` + `Directory.Build.props` `[study 04]`
- [ ] `Core.Contracts` (`IFileStore`/`FileRef`); migrate Identity then Content off `Core.csproj`
      `[02 §1 / 05 §6]`; rename Core→Storage `[13]`
- [ ] Evict avatar/thumbnail/colour/slug from Core `[05 §5]`
- [ ] `tests/Architecture` NetArchTest rules (boundaries + layers) `[02 §3 / study 02]`
- **PR:** `refactor(core): extract Storage contracts and enforce module boundaries`

### Stage 14 — Invert localization-in-domain
- [ ] `DomainException` (code-only) + strategy; drop the `errors` parameter from the ~50 domain
      signatures; re-key `.resx` by code `[03 §6 / 08 §9]`
- **PR:** `refactor(domain): remove i18n from the domain via coded domain exceptions`

### Stage 15 — Project restructure
- [ ] Split `Shared` into `SharedKernel` + `BuildingBlocks.{Domain,Application,Infrastructure,Presentation}`
      `[study 08 / 09]`
- [ ] Split each module into `Domain/Application/Infrastructure` (+ `Api`) projects `[study 02 / 11]`
- [ ] Entity/behavior partial split (`Entities/` + `Behaviors/`) `[study 10]`
- [ ] Demote junction/child entities from aggregate roots `[03 §1]`
- **PR:** `refactor(structure): SharedKernel/BuildingBlocks and per-module layer projects`

### Stage 16 — Documentation restructure
- [ ] `docs/README.md` entry point; front-matter status; archive shipped specs; banner stale executable
      specs; commit `CLAUDE.md`; fix config/onboarding docs `[09 documentation]`
- **PR:** `docs: restructure documentation with an entry point and accurate status`

---

## Notes

- Stages 1–5 are **security/correctness** and should ship first regardless of the restructure appetite.
- Stages 6–12 are **correctness & platform** — each is independently valuable.
- Stages 13–15 are the **structural** work; they are large and optional relative to 1–12. Do them only
  with the appetite the [module-restructure-study](../module-restructure-study/README.md) verdict
  assumes.
- Stage 16 runs **continuously** — update docs as each stage lands, not only at the end.
