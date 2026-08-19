# Spec 03 — In-App Notifications

## Goal

A user-facing notification feed fed **exclusively by domain-event handlers**
— no business handler ever writes a notification row directly. This is the
second delivery channel that makes most events multi-consumer from day one.

## Scope rule

Recipients are **platform users only**. Commerce customers never log in —
their events remain email-only. Every handler applies the same null-recipient
guards as the email side (OAuth users without email still get in-app rows;
the guards are per-channel).

## NotificationEntity

Lives in the Mailer module (the cross-cutting notification module; renaming
the module to `Notifier` is explicitly out of scope), table
`mailer.notifications`:

| Column | Type | Notes |
| --- | --- | --- |
| `id` | uuid PK | |
| `user_id` | uuid | bare Guid, Identity-owned; no FK by design (house pattern) |
| `type` | text | `EnumNotificationType`, string-converted |
| `title` | text (200) | rendered at write time, localized |
| `body` | text (500) | rendered at write time, localized |
| `link_path` | text (300) null | relative frontend path (`/articles/slug`), never absolute |
| `read_at` | timestamptz null | null = unread |
| + auditable fields | | `created_at` orders the feed |

Indexes: `(user_id, read_at)` for the unread count,
`(user_id, created_at desc)` for the feed page.

Domain methods: `Create(...)` factory, `MarkRead(now)` (idempotent no-op when
already read).

## INotifier contract (Mailer.Contracts)

```csharp
public interface INotifier
{
    Task NotifyAsync(
        Guid userId,
        EnumNotificationType type,
        IReadOnlyDictionary<string, string> tokens,
        string culture,
        CancellationToken cancellationToken
    );
}
```

- Renders `title`/`body` from a `NotificationMessage` localizer facade with
  neutral/en/fr `.resx` — the exact machinery `EmailTemplateMessage` uses,
  including the unresolved-placeholder failure rule.
- `link_path` comes from a token by convention (`linkPath`, optional).
- Persists immediately in the Mailer context (same posture as `IMailer`);
  called from event handlers only.

## EnumNotificationType (append-only)

v1 catalog — matching the "in-app: yes" rows of specs 04 and 09:

`PasswordChanged`, `PasswordResetCompleted`, `LocalPasswordAdded`,
`EmailChanged`, `SignedOutAllDevices`, `AccountForceLoggedOut`,
`RoleChanged`, `CommentReply`.

Reserved for the community wave (spec 09): `SubmissionDecided`,
`RevisionDecided`, `ArtistVerified`.

## Endpoints

All authenticated, ownership enforced by `user_id` filter from the JWT; use
case slices follow house conventions (Command/Query + Handler + Validator +
MetaField + `V1` endpoint).

| Method | Route | Rate limit | Behavior |
| --- | --- | --- | --- |
| `GET` | `/api/v1/public/notifications` | `ContentBrowsing` | Own feed, newest first, paginated; optional `unreadOnly` filter |
| `GET` | `/api/v1/public/notifications/unread-count` | `ContentBrowsing` | `{ count }` for the badge |
| `PATCH` | `/api/v1/public/notifications/{id}/read` | `UserProfile` | Idempotent; 404 for another user's row (no existence leak) |
| `PATCH` | `/api/v1/public/notifications/read-all` | `UserProfile` | Bulk `read_at = now` where null |

## Retention

None in v1. A cleanup job (read + older than 90 days) is noted for later —
same time-threshold polling category as the existing cleanup jobs.

## Testing

- Unit: entity transitions (idempotent `MarkRead`), renderer resources for
  every type × {en, fr}, notifier persistence, validator rules.
- Integration: feed pagination and ownership isolation over real HTTP
  (user A never sees user B's rows), unread count, single and bulk read
  idempotency; handler-driven creation is covered by the event specs'
  end-to-end tests.

## Checklist

- [x] Entity + configuration + migration (`AddNotifications`)
- [x] `INotifier` + `NotificationMessage` + neutral/en/fr resources
- [x] Four endpoints with MetaFields, validators, rate limits
- [x] v1 type catalog rendered in both cultures
- [x] Unit + integration coverage green

## Implementation notes

- The enum ships all eleven members (eight active plus the three reserved
  community-wave members), but resources exist only for the eight active
  types. `NotificationMessage` throws on a missing resource instead of
  returning the raw key name — a small addition over the
  `EmailTemplateMessage` machinery so a reserved member can never render as
  `SubmissionDecidedTitle`; spec 09 adds the copy when those flows land.
- `NotificationRenderer` mirrors `EmailTemplateRenderer` (culture switch,
  `{{token}}` substitution, unresolved-placeholder failure) but inserts token
  values raw: the feed renders plain text, not HTML, so there is nothing to
  encode.
- The `linkPath` token is lifted into the row's `link_path` column by
  `Notifier` rather than substituted into the copy; supplying it is optional
  and omitting it stores null.
- Read-all is implemented as load-unread → `MarkRead(now)` per row → one
  commit, not a single SQL `UPDATE`. Per-user unread volume is small, and
  going through the tracked entities keeps the audit interceptor and the
  domain transition in the loop. The endpoint returns the number of rows
  transitioned by the call (zero on repeat).
- Mark-read commits only when the row actually transitions; a re-mark
  responds 200 with the original `read_at` untouched. Unknown ids and other
  users' rows share one 404 via a user-scoped repository lookup, so row
  existence never leaks.
- Endpoints authorize with `UserRolePolicies.RequireVisitorOnly` plus the
  JWT-scoped `user_id` filter, matching every other authenticated
  `public/*` endpoint.
- The only user-supplied inputs are the route id and the pagination query;
  the id gets the `PublicMarkNotificationReadValidator`, while `pageIndex` /
  `pageSize` are clamped at the endpoint like the existing paged listings, so
  no pagination validator exists to fire.
- No domain event is raised in this phase — `INotifier` consumers are the
  event handlers of specs 04–09 — so spec 01's end-to-end dispatch proof
  remains open and lands with the first migrated event.
