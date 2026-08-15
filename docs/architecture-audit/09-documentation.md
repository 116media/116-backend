# 09 — Documentation

Scope: 307 markdown files under `docs/` plus the root `CLAUDE.md`.

The problem here is not that documentation is missing — there is a great deal of it. It is that
the documentation cannot be trusted: the canonical guide is untracked, its structural facts are
false, the copy-paste templates don't compile, no doc is bootable, and shipped feature specs are
still presented as pending work with code snippets that no longer match the real signatures — which
matters acutely because this repo's specs are written to be *executed* by an agent.

The through-line: **there is no status signal and no entry point**, so a reader cannot tell durable
reference from a finished implementation plan from an unbuilt design, and picks whichever stale
worldview they land on first.

---

## 9.1 The canonical codebase guide is gitignored and untracked

**Severity: Critical**

**Where:** `.gitignore:71` lists `CLAUDE.md`; `git ls-files CLAUDE.md` is empty. The 926-line file
that 10 other docs defer to has no version history, never appears in a PR diff, and does not exist in
a fresh clone.

**Problem/why.** The one document every other doc points at is never reviewed and gone on clone. Every
drift below accumulated precisely because nothing in CI or review can see this file change; a new
engineer cloning the repo gets 307 files and no guide.

**Solution.** Remove line 71, commit `CLAUDE.md`, add it to the PR checklist as a must-update-when
`src/Modules/*`, `Program.cs`, `RateLimitPolicies.cs`, or `.env.template` change. If the intent was to
keep agent instructions out of the repo, split: a tracked `docs/codebase-guide.md` plus a small
untracked agent preamble.

---

## 9.2 `CLAUDE.md`'s structural facts are false: tree, schemas, migration paths, key files, policy symbols

**Severity: Critical**

**Where:** the "Project Structure" tree lists only Core and Identity (omitting Content — 49 entities,
221 endpoints — and Mailer). The schema list says `core`, `authentication`; the real schemas are
`core`, `identity`, `content`, `mailer` (no `authentication`). Migration commands reference
`src/Modules/Identity/Infrastructure` (real: `.../Identity/Identity/`) — both commands fail. "Key
Files" paths (`Shared/Abstractions/Dispatcher.cs`, `Shared/Common/BaseModule.cs`) don't exist. The
endpoint sample uses `UserRolePolicies.AdminOnly` — 0 occurrences; the real constant is
`RequireAdminOnly`, which no endpoint uses. Visitor permissions "28" vs 29. Rate-limit table lists 9
of 10 policies.

**Problem/why.** The tree omits 73% of the codebase, so a reader believes Content doesn't exist. The
only documented migration commands both fail. The `authentication` schema name gets typed into a
`HasDefaultSchema` and silently creates a second empty schema. `UserRolePolicies.AdminOnly` is copied
into a new endpoint and doesn't compile; the "fix" to `RequireAdminOnly` picks the one policy no
endpoint uses, producing an endpoint no admin can reach.

**Solution.** Regenerate the structure section from `find src -name '*.csproj'` in a CI-checked script.
Replace the schema list with a table generated from the four `SchemaName` constants. Fix the "Key
Files" paths and migration commands (one parameterized block + a table of the four `(project,
DbContext)` pairs). Repoint the RBAC section at the real policy file and state which policies are
actually used.

---

## 9.3 `CLAUDE.md`'s copy-paste endpoint and CQRS templates do not compile

**Severity: High**

**Where:** the "Creating New Endpoints" snippet hardcodes `/api/v1/...` (real endpoints use
`MapApiVersionGroup(1)` — 294 sites vs the snippet's `WithApiVersionSet` which has 1), calls
`.WithRateLimiting(...)` (not a real method; the real one is `RequireRateLimiting`, 293 sites), and
dispatches via `DispatchAsync` (only `Send` exists). It also omits the mandatory `MetaField` (293
files).

**Problem/why.** This is the one snippet a new engineer or agent copies verbatim to add an endpoint,
and every line fails — two symbols don't exist, the route shape bypasses the version group (making the
endpoint unreachable at `/api/v1`), and the rate-limit call is a no-op name.

**Solution.** Replace it with a verbatim, annotated copy of a real `*EndpointV1.cs`. Add a CI grep that
fails on `WithRateLimiting|DispatchAsync|WithApiVersionSet|UserRolePolicies\.AdminOnly` in the doc.

---

## 9.4 Configuration is documented wrong; the app cannot be booted from any doc in the repo

**Severity: Critical**

**Where:** 7 of 14 env var names in `CLAUDE.md` don't exist (`DB_HOST` → `POSTGRES_HOST`;
`JWT_REFRESH_TOKEN_EXPIRATION_IN_DAYS=30` → `JWT_REFRESH_TOKEN_EXPIRATION`, in **minutes** — wrong by
1440×). `POSTGRES_HOST` is missing from `.env.template` entirely, so `cp .env.template .env` yields
`Host=;...` and every DbContext fails. 11 live vars are undocumented (`DEFAULT_USER_PASSWORD` hard-
throws, `EMAIL_PROVIDER` throws at boot, the SMTP/Resend/Odesli/Frontend vars). `appsettings.json`
advertises dead `ConnectionStrings:Redis`, a `Keycloak` block, and `Application = "EShop ASP.NET App"`.

**Problem/why.** Onboarding is impossible from the docs — a new engineer sets `DB_HOST`, gets a
connection failure, then finds the template is also missing the host, then email silently no-ops, then
provisions Redis for a service that doesn't use it.

**Solution.** Create `docs/configuration.md` as the single source: every variable with its reader
`file:line`, required/optional, default, unit, and failure-mode-when-absent — seeded from
`Environment.cs` and the sender services. Add `POSTGRES_HOST` and `RESEND_API_URL` to `.env.template`;
delete the Redis/Keycloak blocks. Add a CI test that every `GetEnvironmentVariable("X")` literal
appears in `.env.template`.

---

## 9.5 `ARCHITECTURE.md` describes a system that was never built

**Severity: Critical** · see [02 §2](02-module-boundaries.md) for the code side.

**Where:** "Communicates with other modules only through domain events" — reality is 276 cross-module
`using`s, 157 bypassing any contracts project (Content→Core 130, Identity→Core 27), and a Content
command writing directly into Core's schema. "Outbox pattern already in place" — the only outbox is
Mailer's *email-delivery* outbox; domain events are in-process, post-`SaveChanges`, best-effort, and a
failing handler is swallowed. "Content/ ← (planned)" — Content is the largest module. Mailer is absent
from the module list.

**Problem/why.** This is the document a tech lead reads to decide whether a module can be extracted. It
says the cost is near zero; the real cost is unwinding 157 direct references into Core, building an
actual transactional outbox, and deciding what happens to the ~90 endpoints resolving Identity's
`IClaimsProvider` synchronously. An architect trusting it plans a RabbitMQ migration assuming
durability that does not exist.

**Solution.** Rewrite the module list and sizes; replace the events-only claim with the real rule +ex­ceptions and a dependency-policy table; delete the outbox claim and add an honest "event delivery
today: at-most-once, in-process, best-effort" section citing the interceptor. Back it with the
architecture test from [02 §3](02-module-boundaries.md).

---

## 9.6 There is no entry point, and five documents each claim to be the one to read first

**Severity: High**

**Where:** no `docs/README.md`/`docs/00-index.md`; the root `README.md` never references `docs/`. Five
competing "read this first" banners (`CLAUDE.md`, `testing/README.md`, an integration-tests progress
doc, `claude/INSTRUCTION.md`, `testing-audit/00-overview.md`), two of which point at paths that no
longer exist. One quotes Content as 31 entities / 147 handlers (real: 49 / 245).

**Problem/why.** 307 files, no front door. `ls docs` returns 12 SCREAMING_CASE files and 21 folders
with no ordering or status; whichever "read first" a newcomer hits determines which stale worldview
they adopt.

**Solution.** Create `docs/README.md` as the only entry point — Start Here (≤5 links), Reference, Specs
(shipped → archive), Specs (in-flight, with owner+date). Link it from the root `README.md`. Delete the
four non-canonical banners and `docs/claude/`. Add a CI stale-link check.

---

## 9.7 Three overlapping testing doc trees (142 files) with eight direct contradictions and zero cross-links

**Severity: High**

**Where:** `docs/testing/` (85), `docs/how-to-tests/` (14), `docs/testing-audit/` (43), with 0
cross-references — `testing-audit/00-overview.md` states its reviewers were "not allowed to read
`docs/`". Eight verified contradictions where the code matches the audit (e.g. `how-to-tests` prescribes
`Randomizer.Seed = new Random(...)` and `Faker _faker = new()` — both deleted from all 73 sites and
named as the defect by the audit). `CLAUDE.md` names one canonical rulebook and never mentions
`testing-audit/`.

**Problem/why.** A reader lands on `how-to-tests/` (wrong on 6 of 8 subjects) and writes tests in the
exact style a 43-doc audit exists to eliminate.

**Solution.** Collapse to two trees: promote `testing-audit/standards/` + `testing/00-unit-vs-
integration-rules.md` into `docs/testing/` as the only normative set (reconciling the 8 conflicts in
the audit's favour); delete `how-to-tests/` after merging its still-true patterns; archive the audit
findings. Update `CLAUDE.md` to name the merged rulebook.

---

## 9.8 Finished work plans are still presented as pending, with 0% of their checkboxes ticked

**Severity: High**

**Where:** `TODO-ROADMAP-1-endpoint-route-params.md` is 100% shipped with 0/19 boxes checked (its
"BLOCKING" FormatException handler exists; all 15 GET endpoints it says are wrong now use `Guid`).
`EDITORIAL_VALIDATION_FIX.md` is done and now *harmful* — it prescribes hardcoded English against the
live i18n validators (99 `.resx`, 66 `IStringLocalizer`). `CONTENT_EF_PLAN.md` (1,709 lines) says
"Nothing is implemented yet" about a module with 49 entities. Repo-wide: 1,955 unchecked vs 1,137
checked boxes.

**Problem/why.** These read as work orders. An agent picking up `TODO-ROADMAP-1` will "implement" a
prerequisite that shipped; `EDITORIAL_VALIDATION_FIX.md` is a step-by-step guide to strip localization
out of 12 validators; `CONTENT_EF_PLAN.md` tells the reader the largest module doesn't exist.

**Solution.** Create `docs/archive/` and move each with a `> SHIPPED <date> — historical, do not
follow` banner. Extract the one durable rule from `EDITORIAL_VALIDATION_FIX.md` (the two-layer
validator pattern) into `docs/patterns/validation.md` written against the real i18n signature. Adopt a
rule: a spec folder is archived the day its index is fully checked, CI-enforced.

---

## 9.9 Checklists are unreliable in both directions, so no doc's status can be trusted

**Severity: High**

**Where:** `artists-page/specs` shows specs 01–10 unchecked while all are shipped (four migrations
exist). `lyrics-page/specs` are all checked but several are false — a "no claim-request entity in this
phase" item that *does* have an entity, and four checked-off commands whose real names carry
`Public`/`Admin` prefixes. No folder carries a status marker.

**Problem/why.** The checkbox is the only status signal and it's wrong both ways — unchecked-but-done
causes duplicate implementation, checked-but-false causes skipped verification of work that was built
differently than recorded. The correct reading of any checkbox is "unknown", so 3,092 checkboxes convey
nothing.

**Solution.** Replace checkboxes-as-status with front-matter (`status: shipped|in-flight|abandoned`,
`shipped_commit`, `verified_on`, `owner`). Backfill `status: shipped` for the ~12 verified-shipped
feature folders and move them to `docs/archive/`, replacing per-item checkboxes with a "Shipped as:"
table mapping each proposed name → the real `file:line`. CI: a `shipped` folder may not contain
`- [ ]`.

---

## 9.10 Spec code snippets contradict real signatures — and the specs are executable instructions

**Severity: Critical** (for the agent-driven workflow this repo uses)

**Where:** across `lyrics-page/specs` (all 14 marked complete, none archived), 13 verified
contradictions: `LyricsDto` (0 occurrences — split into `LyricsSummaryDto`/`LyricsDetailDto`);
`GetPublishedAsync` (real: `GetAllAsync` with 8 params); `GetDefaultLyricsCategoryIdAsync` returning
`Guid` (real: `GetDefaultLyricsCategoryAsync` returning `CategoryEntity?` — code following the spec
can't null-check); `SubmitLyricsCommand(...5 params)` where the real record inserts `string? Slug` at
position 5. The pattern (missing `Public`/`Admin` prefixes, added `CurrentUserId`/`Slug` params, DI
lists grown by 2–4) recurs across artists-page, domain-events, email-service, popular-articles,
article-comment-authors, article-interaction-state — ~68 spec files.

**Problem/why.** These are `public record` declarations and `await repo.Method(...)` call sites written
to be executed. An agent handed "implement spec 11" emits a wrong-named command, calls a nonexistent
repository method, and — case #8 — constructs a 5-arg record positionally against a 6-arg target that
**compiles** if `userId` binds to `string? Slug`, silently writing a GUID into the slug column. Because
the folder is marked 100% complete with no archive banner, nothing warns the reader these are
historical.

**Solution.** Immediately banner every shipped spec folder: `> ⚠ SHIPPED. Code snippets are
pre-implementation drafts and DO NOT match current signatures. Verify against src/ before use.`
Structurally: no spec may contain a code-signature fence once marked shipped — replace with a "Shipped
as" table (CI-enforced over fully-checked indexes). For in-flight specs, a nightly job extracts type
names from `csharp` fences and greps `src/`; a name that exists nowhere and isn't marked "to be
created" is a stale-spec alert.

---

## 9.11 Designs for unbuilt subsystems are formatted identically to documentation of shipped ones

**Severity: Medium**

**Where:** `ADS_MODULE.md` (296 lines) documents an `ads` schema with 8 tables — none exists.
`analytics/interactions.md` specifies endpoints — 0 analytics files exist. Neither carries a
"planned"/"proposed" marker; both sit beside `CONTENT_MODULE.md`, which *is* built. Inversely,
`ARCHITECTURE.md` marks Content "(planned)".

**Problem/why.** No way to tell a design from a description. A frontend engineer builds a dashboard
against `ads.campaigns` and `GET /admin/analytics/content`, neither of which will ever return anything.

**Solution.** Mandatory `status:` front-matter. Set the two to `status: proposed`, move to
`docs/proposals/`; set Content to shipped. Add the three-way split to `docs/README.md`: Reference (what
is) / Proposals (what might be) / Archive (what was).

---

## 9.12 The architecture's load-bearing rules are documented nowhere

**Severity: High**

**Where:** verified absent — a module dependency/allowed-reference policy (276 cross-module usings, 157
bypassing contracts, and no rule stated); any ADRs (nothing records why Carter, why a custom Dispatcher,
why post-`SaveChanges` events over an outbox); a domain-event catalogue (41 events, 33 handlers, 2
orphaned events silently dropped); an ERD/data model; an OpenAPI export (so no diffable API contract);
a definition of the `MetaField` convention (293 files, mentioned once); a runbook (Quartz jobs and the
email outbox run in production with no operational doc); an onboarding guide.

**Problem/why.** Each is a decision re-litigated per PR by whoever has the most context. Without a
dependency policy the Core coupling grows unchecked (already 157 sites for a module with one entity);
without an event catalogue the two orphaned session events stay orphaned; without ADRs the next
engineer proposes MediatR and nobody can say why it was rejected.

**Solution.** Add six files in value order: `module-boundaries.md` (the dependency matrix + the 157
violations as tracked debt, backed by a `NetArchTest`); `adr/` with ~6 backfilled decisions; a
generated `domain-events.md` table (name, module, raised-by, handled-by/UNHANDLED); `patterns/metafield.md`;
`data-model.md` (a mermaid ERD per schema); `runbook.md`. Commit an OpenAPI export per release tag.

---

## 9.13 No naming/location/index convention; three docs are textually corrupted; docs exist outside `docs/`

**Severity: Medium**

**Where:** four coexisting naming styles with no rule; index convention inconsistent (8 folders
`00-index.md`, 4 `README.md`, 9 with no index). `.sql` files sit in the markdown tree parallel to the EF
migrations that are the real source of truth. Three corruptions from accidental editor link-insertion
(`CLAUDE.md:595` `... instead of [.gitignore](.gitignore)opaque tokens`, and two more). A top-level
`articles/` directory duplicates `docs/` subjects. `docs/commits/` (9 files, 3,015 lines) is a one-off
git-history-rewrite worksheet checked in as documentation.

**Problem/why.** `ls docs` gives no signal about importance, status, or type; SCREAMING_CASE at the
root reads as canonical and is exactly the most-stale set. Nine folders offer no way in. The parallel
`.sql` files invite someone to run DDL the migrations already own.

**Solution.** One rule: `kebab-case.md`, ordered files `NN-` prefixed, every folder has `00-index.md`.
Split the root into `reference/`, `patterns/`, `proposals/`, `archive/`, `specs/`; root holds only
`README.md`. Delete the `.sql` files (or archive with a banner naming the superseding migration). Fix
the three corruptions; add markdown-lint + link-check to CI. Move/delete `articles/`. Delete
`docs/commits/`.

---

## 9.14 `README.md` contradicts `CLAUDE.md` on the branching model and links to nothing

**Severity: Low**

**Where:** `README.md` says `main` is the single source of truth; `CLAUDE.md` says `develop` is the
main branch and PR target (and the checkout + all CI badges are `develop`). `README.md` never
references `docs/`. `CLAUDE.md` spends 181 lines on Conventional Commits — more than architecture,
modules, database, and testing combined.

**Solution.** Reconcile in one `docs/reference/git-workflow.md` (`main` = release, `develop` = PR
target), linked from both. Add a "Documentation" section to `README.md`. Cut the commit section of
`CLAUDE.md` to ~30 lines; the reclaimed budget goes to the architecture sections fixed in §9.2.

---

## Inventory summary

Of 307 docs: **~30 (10%)** durable and current; **~145 (47%)** shipped specs not archived; **~120
(39%)** ephemeral (progress logs, TODOs, audit findings); **~2 (1%)** designs for unbuilt work; **~11
(4%)** dead (broken paths / one-off scripts). The durable-and-current set worth keeping as-is:
`factory-pattern.md`, `standalone-file-upload-pattern.md`, `testing/00-unit-vs-integration-rules.md`,
`testing-audit/standards/`. Everything else needs a status decision.
