# Spec 13 — Engagement Emails

## Goal

The first (deliberately small) engagement notification: tell a comment author
someone replied. Lower priority than specs 11–12 and scoped tightly, because
the platform has **no notification-preference system** — every engagement
email added before preferences exist is an email nobody can turn off.

## New template

| Template | Required tokens |
| --- | --- |
| `CommentReply` | `userName`, `replierName`, `articleTitle`, `replyExcerpt`, `articleUrl` |

`replyExcerpt` is the reply body truncated to 140 characters at a word
boundary, HTML-encoded by the renderer like every token.

## Hook

`PublicAddCommentReplyHandler` — the one consumer-side change:

- The handler **already injects `IUserLookupService`** and loads the parent
  comment; after the reply persists, resolve the parent author via
  `GetAuthorInfoByIdAsync(parent.UserId)` and enqueue `CommentReply`.
- Guards, in order:
  1. `parent.UserId == command.UserId` (self-reply) ⇒ skip
  2. author lookup returns null or a null `Email` (OAuth user) ⇒ skip, log `Debug`
  3. parent comment soft-deleted ⇒ skip
- Enqueue joins the handler's existing commit, standard pattern.
- `articleUrl` builds from `FRONTEND_BASE_URL` (spec 08) + the article slug the
  handler already has in scope (extend the load if it only has the id —
  record what was needed).
- Domain-events refactor note (docs/domain-events, spec 09): this hook moved
  behind `CommentReplyAddedEvent` — `CommentReplyAddedNotificationsHandler`
  now sends the email, writes the `CommentReply` in-app row, and owns the
  guards; `PublicAddCommentReplyHandler` no longer injects `IMailer`.

## Explicitly deferred (recorded so the scope is a decision, not an oversight)

| Candidate | Why deferred |
| --- | --- |
| Comment removed by moderator | `AdminDeleteArticleCommentHandler` soft-deletes with **no reason field** and no moderation/report pipeline exists; a reason-less "your comment was removed" email invites support load with nothing to say. Revisit when moderation grows a reason |
| Comment likes | Low-signal, high-volume — digest territory, and no digest system exists |
| Notification preferences | No preference entity/screen exists anywhere. Acceptable for exactly one low-volume transactional-ish notification; **a second engagement email must not ship before preferences do** — treat that as a hard gate |

## Testing

- Unit: handler asserts enqueue with correct tokens; self-reply skip;
  null-email skip; deleted-parent skip; excerpt truncation boundary cases.
- Integration: reply endpoint test asserts the outbox row targets the parent
  author with the excerpt; self-reply persists no row.

## Checklist

- [x] `CommentReply` template + resources appended
- [x] Hook with the three guards in `PublicAddCommentReplyHandler`
- [x] Excerpt truncation implemented and tested
- [x] Deferred list above left intact as the scope record
- [x] Unit + integration coverage green

## Implementation notes

- The hook resolves the parent author via the already-injected
  `IUserLookupService`; the excerpt truncates at 140 chars on a word
  boundary; the deferred list stands unchanged.
