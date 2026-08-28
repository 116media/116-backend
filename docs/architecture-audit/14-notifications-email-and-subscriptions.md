# 14 — Notifications, Email & Subscriptions: the message-class model

Scope: the `Mailer` module (`src/Modules/Mailer/Mailer`) and how the system tells users things.

**The question that prompted this:** a notification and an email are not the same thing — a
notification can be delivered in-app *or* by email; some messages the user must never be able to
deny (OTP verification); some require explicit opt-in (newsletter). Is this configured right?

**Answer: no.** "Mailer" is named after a *transport* (email) but is used as the home for a
*concept* (notification), a *second transport* (in-app), and a *subscription sub-domain*
(newsletter) — with no model of the four things that actually matter: **the event, its message
class, its audience, and its channels.**

---

## The evidence

1. **The same event is modelled twice.** Six members appear in **both** `EnumEmailTemplate` (27)
   and `EnumNotificationType` (11): `PasswordChanged`, `PasswordResetCompleted`,
   `LocalPasswordAdded`, `SignedOutAllDevices`, `AccountForceLoggedOut`, `RoleChanged`. So "your
   password changed" is not one notification that fans out to channels — it is two disconnected
   enum members, and the caller manually fires `IMailer.EnqueueAsync(...)` **and**
   `INotifier.NotifyAsync(...)` for the same fact. The ports are typed **by transport**
   (`IMailer` = email, `INotifier` = in-app), not by concept.

2. **No message-class or channel concept exists.** `grep Category|Mandatory|Transactional|Channel|
   MessageClass|CanOptOut` → 0 hits. `EmailVerificationOtp` (undeniable), `NewsletterWelcome`
   (opt-in marketing), and `RoleChanged` (preference-gated) sit flat in one enum with identical
   handling. Nothing can express "this class ignores preferences" vs "this one is opt-out" vs
   "this one needs a subscription".

---

## The four axes that should be modelled

| Axis | Question it answers | Values |
|---|---|---|
| **Event** | What happened? | `PasswordChanged`, `CommentReply`, `EpisodePublished`, `ContentSubmittedForReview`, `OtpRequested`, … (one per real fact) |
| **Message class** | Under what policy may we send it? | Transactional (mandatory) · Operational (staff) · Notification (preference-gated) · Subscription (opt-in) |
| **Audience** | Who receives it, and how is the set resolved? | Direct user · Broadcast (all users) · Role-derived (e.g. reviewers/admins) · Opt-in list (confirmed subscribers) |
| **Channel** | Through which transport? | InApp · Email · Push · SMS |

An event has **one class**, a way to **resolve its audience**, and a set of **default channels**;
the class + the recipient's preferences + subscription/follow state decide which channels fire.

### The four message classes

**Transactional (mandatory) — the user cannot deny these.**
- OTP verification, password-reset code, password-changed / email-changed / security alerts, payment
  receipts, account-force-logout.
- Audience: **direct user**. **Always sent. Preferences are not consulted. No unsubscribe.** This is
  why OTP must never be gated by the Settings preferences — a preference toggle over OTP would lock
  users out.
- Deliverability note: a *transactional* email stream, kept separate from marketing so a marketing
  unsubscribe never suppresses a security email.

**Operational (staff / editorial) — the work queue; in-app-first.**
- Content submitted for review, content approved/rejected/published (lifecycle), payment awaiting
  verification, artist-claim awaiting decision, abandoned-draft alerts.
- **In-app is the primary channel** — this is the admin content-review / content-lifecycle feed. The
  recipient can tune *secondary* channels (email digest on/off), but the in-app work-queue entry
  always exists, because it is their job, not a courtesy. Effectively mandatory-for-role on the in-app
  channel, preference-tunable on the rest.
- **Two audience shapes inside this class:**
  - **Role-derived — the review queue.** "Content awaiting review", "payment to verify" → whoever
    can act on it, resolved by role/permission. Fan-out to a group.
  - **Direct staff member — the author feedback loop.** "*Your* article was rejected / approved /
    published" → the **specific admin who created that content item**, resolved from the item's
    author/`CreatedBy`, not a role. This is the case where Admin A authors an article, Admin B rejects
    it, and A must be told — the reviewer and the author are different people, and the notification is
    user-particular.
- This is what "in-app notifications, specially for admins" maps to — both the shared queue and the
  personal "what happened to my content" feed.

**Notification (preference-gated) — default on, opt-out per type × channel.**
- Comment reply, submission/revision decided (to the proposer), artist verified, role changed (the
  informational half), and **"a new episode was published" — broadcast to every user** (there is no
  follow/subscription relationship in this application; a publish notifies the whole user base).
- Audience: **direct user** (comment reply) or **broadcast / all users** (episode-published). Each
  recipient can still toggle the type × channel via the **Settings** module
  ([13](13-core-storage-and-settings-module.md)) — the broadcast reaches everyone who has not opted
  out. Default channels: in-app on, email on (say), push on.

**Subscription (opt-in) — default off, explicit consent required.**
- Newsletter, digests, marketing.
- Audience: **relationship-derived** (confirmed subscribers). Require **double opt-in** (the codebase
  already does this for newsletter — `NewsletterSubscriberEntity` with confirm/unsubscribe tokens),
  an unsubscribe link, and RFC 8058 `List-Unsubscribe` headers ([05 §14](05-core-and-mailer.md)).
- Sent only to confirmed subscribers; unsubscribing suppresses this class only, never transactional.

### The channel model

A notification event is dispatched to channels, each an **adapter** behind a common port:
- **In-app** — persists a `NotificationEntity` row (already exists), surfaced by the notifications
  endpoints.
- **Email** — enqueues an outbox row (already exists), delivered by the dispatcher job.
- **Push / SMS** — future adapters, same port.

One event → many channels. "Your comment got a reply" writes an in-app row **and** (if the user's
email channel for that type is on) an email — from **one** dispatch call, not two hand-wired ones.

---

## Two worked examples

### A. "A new episode was published" → broadcast to all users

- **Class:** Notification (preference-gated). **Audience:** **broadcast — every user.** There is no
  follow/subscription in this application (verified — the only opt-in is the newsletter); a publish
  notifies the whole user base, minus anyone who has opted out of "new episode" notifications in
  Settings.
- **Flow:** `EpisodePublishedEvent` → audience = all users → each recipient's channels =
  default channels ∩ their Settings preference for this type → fan out.
- **Delivery nuance — broadcast is fan-out-heavy, so don't do it naively:**
  - **In-app:** write **one broadcast/announcement row**, not one row per user, with per-user
    read-state. Millions of `NotificationEntity` inserts per publish is the wrong shape; a single
    announcement the feed joins against is the right one.
  - **Email / push:** fan out over the user base in a **background job** (batched), honouring each
    user's preference — never inline in the publish request. This reuses the outbox/dispatcher
    machinery Mailer already has ([05 §12](05-core-and-mailer.md)).
- **No new relationship model is needed** — the audience is simply "all active users", a query
  Identity can expose. This is the second general audience shape (broadcast), distinct from a single
  user and from the confirmed opt-in list a newsletter uses.

### B. Admin content review & lifecycle → in-app work queue *and* author feedback

- **Class:** Operational (staff), in-app-first. Two audience shapes, both in this one flow:
  - **The review queue (role-derived).** `ContentSubmittedForReviewEvent` → resolve recipients by
    role/permission (everyone who can review this content type) → write an in-app work-queue entry for
    each; email is a secondary, tunable digest.
  - **The author feedback (direct staff member).** `ArticleRejectedEvent` / `ArticleApprovedEvent` /
    `ArticlePublishedEvent` → resolve the **one** recipient = the content item's author (`CreatedBy`)
    → in-app entry (+ optional email). **Example: Admin A creates the article, Admin B rejects it →
    the event carries the rejection reason and A is notified.** The actor who rejected (B) and the
    recipient (A) are different, and the recipient is a specific user, not a role.
- **Prerequisites that do not exist today:** there are **no admin-targeted notifications** — the
  current decided-notifications only fire for community *submissions* and go to the external proposer,
  never to a staff author. This needs: (a) role/permission recipient resolution for the queue (Identity
  knows roles; expose "users in role R" via `Identity.Contracts`); (b) author resolution for the
  feedback — read the content item's `CreatedBy` (already stamped by the audit interceptor); and (c)
  the editorial lifecycle events raised with the decider and reason on them (some exist — `Content*`
  state-change events from [03 §3/§10](03-content-domain.md) — the reject/approve/publish ones for
  admin-created content need adding).
- The in-app channel is the **primary** transport here by design: the admin editorial dashboard reads
  the in-app feed both as the shared review queue and as each admin's personal "what happened to my
  content" list, so an Operational notification must always produce an in-app entry even if every
  other channel is silenced.

---

## What "Mailer" should become

Same lesson as [13](13-core-storage-and-settings-module.md)'s Core→Storage: name the module for the
**concept**, not the transport.

- **Rename `Mailer` → `Notifications`** — the bounded context "informing users". It owns:
  - the **notification catalogue** (event → class → default channels), replacing the two overlapping
    enums with one concept;
  - the **dispatch/orchestration** that applies the class policy, reads Settings preferences (for
    the Notification class) and subscription state (for the Subscription class), and fans out to
    channels;
  - the **channel adapters** — in-app (its `NotificationEntity` store) and email (its outbox +
    provider adapters). Email is now *one channel*, not the module's identity.
- **`INotifier`/`IMailer` collapse into one `INotificationDispatcher`** in `Notifications.Contracts`:
  `DispatchAsync(NotificationEvent evt, CancellationToken)`, where the event carries (or the catalogue
  resolves) its **audience** — a direct user id, a relationship key (e.g. "followers of artist X"), or
  a role. Callers stop firing two ports for one event and stop hand-resolving recipients. A thin
  transactional-only entry (`SendTransactionalAsync(userId, …)`) exists for the mandatory class so
  OTP/security can't accidentally be preference-gated.
- **Newsletter/subscriptions** is a distinct sub-domain (marketing consent, different rules). Keep it
  as a slice inside `Notifications` for now; it is a candidate to split into its own `Subscriptions`
  module later if marketing grows — the same "extract when it earns it" logic as Commerce in
  [10](10-content-module-sizing.md).

### Who decides vs who delivers

```text
Domain event (e.g. EpisodePublishedEvent)
        │
        ▼
Notifications.Dispatch(evt)
        │   1. catalogue lookup: message CLASS + default channels + how to resolve AUDIENCE
        │   2. resolve AUDIENCE → recipient set:
        │        direct user   → [that user]
        │        broadcast     → [all active users]           (episode-published — example A)
        │        role-derived  → [users in role R]            (via Identity.Contracts — example B)
        │        opt-in list   → [confirmed subscribers]      (newsletter)
        │   3. per recipient, pick CHANNELS by class:
        │        Transactional → all default channels, ignore preferences        (OTP, security)
        │        Operational   → in-app ALWAYS + secondary channels per prefs     (admin review queue)
        │        Notification  → default channels ∩ Settings preferences          (reply, new episode)
        │        Subscription  → only if a confirmed subscription exists           (newsletter)
        ▼
   channel adapters:  [In-App store]  [Email outbox]  [Push]  …
```

- **Notifications** owns the *policy* (class → channels, audience resolution, honour prefs/consent).
- **Settings** owns the *preference data* the Notification/Operational classes read
  ([13](13-core-storage-and-settings-module.md)) — and is **never consulted for the Transactional
  class**, nor allowed to silence an Operational in-app work-queue entry.
- **Identity** resolves role-derived ("users in role R") and broadcast ("all active users")
  audiences via `Identity.Contracts`; the opt-in list is the newsletter subscriber table.
- **Channel adapters** own the *mechanism* (render + persist/deliver + retry).

---

## What this fixes

- **The duplicated enums** — one notification catalogue instead of `EnumEmailTemplate` (27) +
  `EnumNotificationType` (11) with 6 events defined in both.
- **OTP stays undeniable** — the Transactional class structurally bypasses preferences, so no toggle
  can suppress a verification or security email.
- **Notifications become opt-out-able** — the missing per-type × channel preference finally has a
  home and a code path ([13](13-core-storage-and-settings-module.md)).
- **Newsletter stays correctly opt-in** — the Subscription class keeps the double-opt-in + unsubscribe
  rules ([05 §14](05-core-and-mailer.md)), separated from transactional so a marketing unsubscribe
  can't kill a security email.
- **Recipient language** — dispatch reads the recipient's `PreferredLanguage` from Settings, closing
  [08 §17](08-cross-cutting.md) (emails currently render in the *caller's* culture) for every class.
- **Admins get a real work queue** — the Operational class gives content review / lifecycle its own
  in-app feed that can't be silenced, instead of nothing (no admin-targeted notifications exist
  today).
- **Publish-to-everyone becomes a first-class broadcast** — "new episode published" rides the same
  dispatch, with one in-app announcement row (not one per user) and a batched email/push fan-out —
  no per-user materialisation and no follow model needed.

---

## Rollout

1. **Add the message-class + audience axes first, non-breaking:** annotate each event with a class
   (Transactional / Operational / Notification / Subscription) and an audience-resolution strategy —
   a lookup table, no behaviour change yet. This alone lets the send path branch correctly and makes
   OTP structurally mandatory.
2. **Introduce `INotificationDispatcher`** and route new events through it; migrate the ~6
   double-fired events to a single dispatch, deleting the duplicate enum members.
3. **Gate the Notification class on Settings preferences**, the Subscription class on subscription
   state, and make the Operational class always write the in-app entry (role-resolved via
   `Identity.Contracts`); leave Transactional ungated.
4. **Wire the new audiences as they land:** role-derived (admins) and broadcast (all users) as soon
   as `Identity.Contracts` exposes "users in role R" and "all active users" — the first unlocks the
   admin content-review / lifecycle feed, the second unlocks "new episode published" (one in-app
   announcement row + a batched email/push fan-out). Both ride the dispatch from step 2 with no new
   relationship model.
5. **Rename `Mailer` → `Notifications`** (module + `Mailer.Contracts` → `Notifications.Contracts`),
   with email as an internal channel adapter. Do this with the [11](11-project-structure-and-packages.md)
   layer split so it lands once.
6. Add push/SMS later as new adapters behind the existing channel port — no dispatch changes.

Net: **notification is the concept; email/in-app/push are channels; the message class decides the
deliverability policy; and the audience decides who receives it** — four axes, modelled explicitly,
instead of two transport-named ports with overlapping flat enums.
