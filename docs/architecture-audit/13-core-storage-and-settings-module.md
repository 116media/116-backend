# 13 — "Core" Is Really Storage; the Settings Module You Actually Want

Scope: the `Core` module (`src/Modules/Core/Core`) and the **Settings/Preferences** bounded
context that was intended but never built.

Two facts drive this document:

1. **What `Core` contains today is 100% file/media storage** — a single `FileEntity`, Cloudinary
   services, image-colour extraction, slug helper. There is no settings, preference, or
   notification-setting concept anywhere in `Core` (or anywhere in the codebase — verified by
   grep). Calling a file-storage module "Core" is what invited the 116-file leak
   ([02 §12](02-module-boundaries.md), [05 §5](05-core-and-mailer.md)): every module reaches into
   it *because it is named like a shared kernel*.

2. **The module intended — system settings + user preferences (notification settings, locale,
   "everything the user needs outside of what the application is really about")** — does not exist.
   The concerns that would live there are currently absent or scattered: there is no per-user
   language preference at all ([08 §17](08-cross-cutting.md)), Mailer sends every notification
   unconditionally with no per-user opt-out, and the tunables that should be settings are hardcoded
   constants (`FileConstants`, rate-limit numbers).

The fix is two moves: **rename the storage concern to what it is**, and **build the Settings
module** in the space that frees up.

---

## Move 1 — rename `Core` → `Storage` (or `Media`/`Files`)

`Core` is a real bounded context: *store bytes, return a keyed row*. It is just misnamed. Rename it
to `Storage` and its module name stops advertising "everything depends on me".

This composes with the already-recommended work:
- The `Core.Contracts` → `Storage.Contracts` extraction with an `IFileStore` returning `FileRef`
  ([02 §1](02-module-boundaries.md)).
- Evicting the avatar/thumbnail/colour/slug leaks so it is a pure storage port
  ([05 §5](05-core-and-mailer.md)).
- Its `FileConstants` moving into `Storage/Domain/Constants/` ([12](12-shared-kernel-and-buildingblocks.md)).

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
  - **`PreferredLanguage` / locale** — closes the gap in [08 §17](08-cross-cutting.md): recipients
    get *their* language, not the acting caller's.
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

- **Mailer consults preferences before sending.** `Notifier`/`OutboxMailer` call
  `IUserPreferencesProvider.IsChannelEnabledAsync(userId, notificationType, channel, ct)` and skip
  the send when disabled. This finally makes the 12-member `EnumNotificationType` opt-out-able.
- **Localization uses the preference.** The 18 `EmailCulture.Current()` sites
  ([08 §17](08-cross-cutting.md)) become
  `preferences.PreferredLanguage ?? EmailCulture.Current()`.
- **Identity stays lean.** Preferences are keyed by `UserId` but do **not** live on `UserEntity`
  (which is already a god aggregate — [07 A2](07-identity-and-security.md)). On signup, Identity
  raises `UserCreatedEvent`; a Settings handler creates a default `UserPreferences` row (or Settings
  lazily creates one on first read). No FK, no bloating the user aggregate.
- **Everyone reads system settings** through a cached `ISystemSettingsProvider`. The cache **must**
  be distributed (Redis) with versioned invalidation — an in-memory cache breaks on multi-instance
  ([04 §8](04-content-infrastructure.md)), and settings are read on nearly every request.

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
- It fills two real gaps the audit found (no per-user language [08 §17](08-cross-cutting.md); no
  notification opt-out) with a purpose-built home instead of bolting them onto `UserEntity` or
  Mailer.
- It keeps policy (Settings: *may we notify this user?*) separate from mechanism (Mailer: *render
  and deliver*) — the two were about to blur, and this draws the line before they do.

## Rollout

1. **Rename `Core` → `Storage`** as part of the `Core.Contracts`/`IFileStore` extraction
   ([02 §1](02-module-boundaries.md)) and the leak-eviction ([05 §5](05-core-and-mailer.md)). Retire
   the name "Core".
2. **Stand up `Settings`** with `SystemSetting` first (unblocks moving `FileConstants`/rate-limit
   numbers out of hardcode) — smallest, no cross-module wiring.
3. **Add `UserPreferences`** + `Settings.Contracts`; wire `PreferredLanguage` into the localization
   sites ([08 §17](08-cross-cutting.md)) and the notification toggles into Mailer's send path.
4. Default-preferences creation on `UserCreatedEvent`; distributed cache for the settings provider.
