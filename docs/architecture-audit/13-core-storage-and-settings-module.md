# 13 — "Core" Is Really Storage; the Settings Module You Actually Want

Scope: the `Core` module (`src/Modules/Core/Core`) and the **Settings/Preferences** bounded
context that was intended but never built.

Two facts drive this document:

1. **What `Core` contains today is 100% file/media storage** — a single `FileEntity`, Cloudinary
   services and image-colour extraction. Measured: one owned table, `core.files`; the other two
   (`domain_event_outbox`, `processed_domain_events`) are the shared outbox plumbing every module
   carries in its own schema. There is no settings, preference, or
   notification-setting concept anywhere in `Core` (or anywhere in the codebase — verified by
   grep). Calling a file-storage module "Core" is what invited the 116-file leak
   ([02 §12](02-module-boundaries.md), [05 §5](05-core-and-mailer.md)): every module reaches into
   it *because it is named like a shared kernel*.

2. **The module intended — system settings + user preferences (notification settings, locale,
   "everything the user needs outside of what the application is really about")** — does not exist.
   The tunables that should be settings are still hardcoded constants (`FileConstants`, rate-limit
   numbers).

   Two of the gaps this document was written against have since closed in Stage 17, which changes
   what Settings has to build rather than whether it should exist:
   - **Per-user locale now exists.** `UserEntity.PreferredLocale` is stored, settable through
     both update-own-profile endpoints, validated against `UserConstants.SupportedLocales`, and
     every email renders per recipient. [08 §17](08-cross-cutting.md) is closed.
   - **A coarse email opt-out now exists.** `EnumEmailClass` gates delivery: transactional and
     operational mail always sends, `Notification` mail stops at an explicit unsubscribe, and
     `Subscription` mail requires a confirmed subscription. What is still missing is the *granular*
     preference — per `EnumNotificationType` × channel.

The fix is two moves: **rename the storage concern to what it is**, and **build the Settings
module** in the space that frees up.

---

## Move 1 — rename `Core` → `Storage` (or `Media`/`Files`)

`Core` is a real bounded context: *store bytes, return a keyed row*. It is just misnamed. Rename it
to `Storage` and its module name stops advertising "everything depends on me".

This composes with the already-recommended work:
- The `Core.Contracts` → `Storage.Contracts` extraction, shipped in Stage 14 as
  `IFileStorageService` + `FileReferenceDto` ([02 §1](02-module-boundaries.md)).
- Its `FileConstants` moving into `Storage/Domain/Constants/` ([12](12-shared-kernel-and-buildingblocks.md)).

**The leak eviction is already done — three of the four, and the fourth is a deliberate keep.**
Stage 14 moved the avatar workflow to Identity (`AvatarService`) and `SlugHelper` to Content;
neither is referenced from `Core` any more, and no thumbnail concern was ever there.
**`IImageColorService` stays**, per Stage 14's amended D3: the colours are not a render-time
concern, they are derived metadata written once at upload onto `core.files`'
`DominantColorHex`/`ForegroundColorHex` and read by consumers off `FileReferenceDto`. Its only
caller is `FileUploadService`, inside the module. Moving it would force `UploadAsync` to accept
colours from outside, leaking a presentation concern *into* the cross-module contract to satisfy a
rule about keeping it out. A store that returns bytes plus their derived metadata is still a store.

### What the rename actually costs

| Concern | Consequence |
| --- | --- |
| `core.files` | A migration doing `ALTER SCHEMA core RENAME TO storage`, carrying `__EFMigrationsHistory` with it. The only step that is not reversible by a revert. |
| `Core.Contracts` | Project/namespace rename to `Storage.Contracts`; two consumers (Identity, Content). Compiler-led. |
| `CoreConstants`, `CoreDbContext`, `CoreI18n`, `CoreRuleCodes`, the outbox job, ~45 files | Mechanical renames. **Measured in stage 18.6: 24 identifiers** — the `Core*` names the module declares, plus its test classes and the four test aliases into its namespace. Not renamed: EF Core's own `CoreEventId`, Identity's `CoreRoleCannotBeDeleted` / `CoreRoleName` / `CoreUserRole`, and the `Migrations/` folder. The six `core.file.*` rule codes become `storage.file.*` (no client matches on rule codes). |
| Colour extraction, slug, avatar, thumbnail | **Nothing to do.** Already resolved above. |

### Will `Storage` grow?

Not much, and that is the point. "Store bytes, return a keyed row" is a narrow context; the
plausible additions are derivatives/variants, signed URLs, a scan status and a retention sweep.
Anything that makes it grow *faster* than that — a slug helper, an avatar policy, a thumbnail size
table — is the symptom the rename exists to make visible.

### How it talks to other modules after the rename

Exactly as it does now, because the seam is already correct and already enforced: consumers
reference `Storage.Contracts` only, and `ModuleBoundaryTests` fails the build when a module reaches
past a Contracts project into another module's internals. Nothing about the mechanism changes. What
changes is the *pressure* — a module named `Core` invites couplings by its name, which is what
produced the 116-file leak; a module named `Storage` makes the next misplacement look as wrong as
it is.

**Mailer and notifications do not move here.** Storage stores bytes; notification *dispatch* stays
in Mailer and notification *preferences* go to Settings (below). The rename is not an invitation to
refill the module under a new name.

**Do not reuse the name "Core" for the Settings module.** "Core" as a module name is an anti-pattern
— every module believes it is core, so the name draws couplings. Retire it. Name the new module for
what it holds: `Settings` (or `Preferences`).

---

## Move 2 — the Settings module

### Bounded context

**Configuration & Preferences** — everything that *tunes how the system behaves*, for the platform
and for each user, and is deliberately **not** part of the publishing domain. Two aggregates, one
`settings` schema, one `SettingsDbContext`.

### What belongs

**A. System settings** — global, admin-managed, read-heavy/write-rare.
- `SystemSettingEntity` — one typed key/value per setting, audited.
- Examples: `maintenance_mode`, `default_locale`, feature flags, the upload size limits that are
  hardcoded in `FileConstants` today, tunable rate-limit numbers, support/contact address, default
  page size.
- Admin CRUD; consumed by every module through a cached `ISystemSettingsProvider`.

**B. User preferences** — per-user, keyed by the Identity `UserId` as an **FK-free `Guid`**
(the codebase's cross-module rule — no cross-schema FK).
- `UserPreferencesEntity` — one row per user, holding:
  - **Locale** — no longer a gap to close but a field to *relocate*. Stage 17 shipped
    `UserEntity.PreferredLocale` with its endpoints and validation, so Settings inherits a working
    feature: move the column, keep the contract, leave the behaviour alone. Do this only when
    Settings exists for other reasons; moving it earlier is churn for no gain.
  - `Timezone`, `Theme` (and whatever else is UI/UX, not domain).
  - **Notification preferences** — per `EnumNotificationType` × channel (email / in-app / push):
    an on/off, and optionally digest frequency and quiet hours. Modelled as child rows
    (`UserNotificationPreferenceEntity`) under the `UserPreferences` aggregate.

### What does NOT belong

- File/media storage → the renamed `Storage` module.
- Auth, roles, permissions, sessions, OTP → Identity.
- Notification **dispatch, rendering, and delivery** → the notifications module (today `Mailer`,
  which should be reshaped/renamed per [14](14-notifications-email-and-subscriptions.md)). Settings
  owns the *preference data* (which types × channels a user wants); the notifications module owns the
  *policy* (message class → which channels, honour prefs) and the *mechanism* (render + deliver).
  **Crucially, Settings is consulted only for preference-gated notifications — never for mandatory
  transactional messages like OTP** ([14](14-notifications-email-and-subscriptions.md)).
- Anything in the publishing domain → Content.

### Integration (via `Settings.Contracts`, never by reaching into entities)

- **The dispatcher consults preferences before sending.** `EmailDispatcher` already decides from
  the message's `EnumEmailClass`; Settings refines the `Notification` arm from "unless this address
  unsubscribed" to `IUserPreferencesProvider.IsChannelEnabledAsync(userId, notificationType,
  channel, ct)`. The class policy stays where it is — transactional mail must never consult
  preferences — so this is one branch changing, not a rewrite.
- **Localization already uses the preference.** `EmailCulture` is deleted and each recipient
  renders in their own `PreferredLocale`; Settings changes where that value is read from, not how
  it is used.
- **Identity stays lean.** Preferences are keyed by `UserId` but do **not** live on `UserEntity`
  (which is already a god aggregate — [07 A2](07-identity-and-security.md)). On signup, Identity
  raises `UserCreatedEvent`; a Settings handler creates a default `UserPreferences` row (or Settings
  lazily creates one on first read). No FK, no bloating the user aggregate.
- **Everyone reads system settings** through a cached `ISystemSettingsProvider`. Settings are read
  on nearly every request and an in-memory-only cache breaks on multi-instance
  ([04 §8](04-content-infrastructure.md)). Use the mechanism Stage 10 establishes — `HybridCache`
  (L1 + L2 Redis) with `RemoveByTagAsync`, not a hand-built version key
  ([16 §16.4](16-caching-architecture.md)). System settings are reference data: one tag, evicted by
  the admin mutation that changed them.

### Contracts surface (`Settings.Contracts`, a leaf like `Identity.Contracts`)

```
ISystemSettingsProvider   — Task<T> GetAsync<T>(string key, T fallback, CancellationToken)
IUserPreferencesProvider  — Task<UserPreferencesDto> GetAsync(Guid userId, CancellationToken)
                            Task<bool> IsChannelEnabledAsync(Guid userId, EnumNotificationType, EnumNotificationChannel, CancellationToken)
EnumNotificationChannel   — Email | InApp | Push
UserPreferencesDto        — PreferredLanguage, Timezone, Theme, channel-map
```

`EnumNotificationType` already lives in `Mailer.Contracts`; Settings references it there (or it is
promoted to a shared location if both need it) rather than redefining it.

### Endpoints

- `GET/PUT /api/v1/me/preferences` — the authenticated user reads/updates their own preferences
  (language, timezone, theme, per-type notification toggles). Ownership from the principal, never
  the body ([07](07-identity-and-security.md)).
- `GET/PUT /api/v1/admin/settings` — SuperAdmin reads/updates system settings.

### Schema & module shape

- Schema `settings`; `SettingsDbContext`; aggregates `SystemSetting` and `UserPreferences`
  (with `UserNotificationPreference` children).
- Built to the adopted layout ([11](11-project-structure-and-packages.md)): `Settings.Domain` /
  `Settings.Application` / `Settings.Infrastructure` / `Settings.Contracts`, CQRS slices, error
  i18n, `MetaField`s — same conventions as every other module. Registered in `Program.cs` like the
  rest.

### One module or two?

System settings (ops concern, global singletons) and user preferences (per-user) are distinct, but
both are "configuration that isn't the core domain" and share the same consumers and the same
policy-vs-mechanism relationship with Mailer. Keep them as **one `Settings` module with two
aggregates** to avoid proliferating tiny modules; split later only if user preferences grow their
own rich lifecycle. If you prefer maximum separation now, `SystemSettings` and `Preferences` as two
modules is defensible — but one is the lower-ceremony start.

---

## Why this is the right DDD call

- It gives each concern its true name and boundary: **Storage** stores bytes, **Settings** holds
  configuration/preferences, **Mailer** delivers, **Identity** authenticates. No module is named
  "Core", so no module invites couplings by its name.
- It gives the preference data a home instead of leaving it on `UserEntity`. Stage 17 had to put
  `PreferredLocale` on the user aggregate because there was nowhere else; that is a stopgap on an
  aggregate the audit already calls too large ([07 A2](07-identity-and-security.md)), and Settings
  is where it belongs once it exists.
- It keeps policy (Settings: *may we notify this user?*) separate from mechanism (Mailer: *render
  and deliver*) — the two were about to blur, and this draws the line before they do.

## Rollout

1. **Rename `Core` → `Storage`.** The contract extraction and the leak eviction already shipped in
   Stage 14, so what is left is the rename itself: the schema migration, the Contracts project, and
   ~45 mechanical file renames. Retire the name "Core". Stage 18 carries this
   ([stage-18 18.5](implementation-specs/stage-18-project-restructure.md)) because it is file moves,
   and every other stage would conflict with it.
2. **Stand up `Settings`** with `SystemSetting` first (unblocks moving `FileConstants`/rate-limit
   numbers out of hardcode) — smallest, no cross-module wiring.
3. **Add `UserPreferences`** + `Settings.Contracts`; refine `EmailDispatcher`'s `Notification` arm
   from the address-level unsubscribe to per-type × channel toggles, and relocate `PreferredLocale`
   off `UserEntity` behind the existing contract.
4. Default-preferences creation on `UserCreatedEvent`; distributed cache for the settings provider.
