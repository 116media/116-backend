# Spec 07 — Newsletter Subscriptions

## Goal

Public newsletter signup with double opt-in and one-click unsubscribe, plus a
minimal admin view — the platform's first marketing-email surface, built on
the same outbox/provider pipeline as the transactional mail.

## NewsletterSubscriberEntity

`Domain/Entities/NewsletterSubscriberEntity.cs` → `mailer.newsletter_subscribers`:

| Column | Type | Notes |
| --- | --- | --- |
| `id` | uuid PK | |
| `email` | citext/text | unique index (lowercased before store) |
| `status` | text | `EnumNewsletterStatus`: `PendingConfirmation`, `Subscribed`, `Unsubscribed` |
| `confirmation_token` | text | random 32-byte url-safe token, unique index |
| `unsubscribe_token` | text | independent token, unique index — lives forever |
| `confirmed_at` / `unsubscribed_at` | timestamptz null | |
| + auditable fields | | |

Domain methods: `Subscribe(email)` factory (status `PendingConfirmation`,
both tokens generated), `Confirm(now)`, `Unsubscribe(now)`, and
`ReissueConfirmation()` for re-subscribing after unsubscribe. Guards:
`Confirm` on a non-pending row and `Unsubscribe` on an already-unsubscribed
row are idempotent no-ops (safe link re-clicks), never throws.

## Why double opt-in

- Anyone can type any email into a public form; sending newsletters to a
  never-confirmed address is abuse-by-proxy and torches sender reputation.
- Only `Subscribed` rows ever receive newsletter content; `PendingConfirmation`
  rows only ever receive the single confirmation email.

## Endpoints

| Method | Route | Auth | Rate limit | Behavior |
| --- | --- | --- | --- | --- |
| `POST` | `/api/v1/public/newsletter/subscriptions` | anonymous | `Otp` (strict sliding window — same abuse profile) | Create or re-issue; **always 202** with a neutral body (no enumeration of existing subscribers); enqueues `NewsletterConfirm` |
| `GET` | `/api/v1/public/newsletter/confirm/{token}` | anonymous | `ContentBrowsing` | Confirms; enqueues `NewsletterWelcome` (carrying the unsubscribe link); unknown token ⇒ 404 problem |
| `GET` | `/api/v1/public/newsletter/unsubscribe/{token}` | anonymous | `ContentBrowsing` | One-click unsubscribe; idempotent 200; unknown token ⇒ 404 |
| `GET` | `/api/v1/admin/newsletter/subscribers` | admin | `AdminMetrics` | Paginated list, filterable by status |

- Confirm/unsubscribe are `GET` because they are clicked from email clients;
  they mutate state idempotently, which is the accepted trade-off for email
  links (same as every mainstream ESP).
- `confirmUrl`/`unsubscribeUrl` tokens are built from a configured public base
  URL (`FRONTEND_BASE_URL`, spec 08) so the links land on frontend pages that
  call these endpoints — the API never renders HTML pages.
- Use-case slices follow house conventions: `PublicSubscribeNewsletterCommand`
  + Handler + Validator + MetaField + `V1` endpoint, etc., with errors via a
  `NewsletterErrors`/`NewsletterErrorMessage` three-layer set (neutral/en/fr).

## Sending an actual newsletter issue

Deliberately **out of scope** for this feature: composing/campaigning is a
product of its own. The subscriber table plus the provider abstraction is the
contract future campaign work builds on. Record any campaign design in a new
spec, not here.

## Checklist

- [x] Entity + enum + configuration + `AddNewsletterSubscribers` migration
- [x] Subscribe/confirm/unsubscribe/admin-list slices with validators and
      MetaFields
- [x] Neutral 202 on subscribe — no subscriber enumeration
- [x] Idempotent confirm/unsubscribe token handling
- [x] `NewsletterConfirm` and `NewsletterWelcome` templates wired (spec 04)
- [x] Unit + integration coverage per spec 09 (including the double-subscribe
      and re-subscribe-after-unsubscribe paths)

## Implementation notes

- Subscribe answers 202 with a neutral body on every path, as specced; the
  confirm/unsubscribe links point at `FRONTEND_BASE_URL` routes via
  `NewsletterLinkBuilder`.
- No goodbye email on unsubscribe — emailing someone who just opted out
  defeats the point.
