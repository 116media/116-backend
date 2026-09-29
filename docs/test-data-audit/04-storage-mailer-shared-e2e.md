# Storage, Mailer, shared and end-to-end suites

Coverage: Mailer read essentially end to end (both suites plus all of `Mailer.TestData`); Storage 17 of
40 files read, the other 23 confirmed by name-grep to construct no file data; `Shared.Unit.Tests`
excluded structurally; `EndToEnd.Tests` is the thinnest at 3 of 32 files read in full.

## Mailer — the factories to add

`Mailer.TestData` was created with three builders and no factories. Counts below were verified by
reading every call site. All three builders already default `_id = Guid.NewGuid()`, so every site's
`.WithId(Guid.NewGuid())` is redundant and the factory bodies drop it with identical behaviour.

### `NewsletterSubscriberFactory.Create(string email)` — 11 sites, 8 files

`new NewsletterSubscriberBuilder().WithEmail(email).Build()` — a pending-confirmation subscriber.

| Site |
| --- |
| `Mailer.Unit.Tests/Application/Newsletter/AdminGetNewsletterSubscribersHandlerTests.cs:27` |
| `Mailer.Unit.Tests/Application/Newsletter/PublicConfirmNewsletterHandlerTests.cs:54` |
| `Mailer.Unit.Tests/Application/Shared/Mappers/NewsletterSubscriberMapperTests.cs:20-23` (inside local `CreateSubscriber`) |
| `Mailer.Unit.Tests/Infrastructure/Repositories/NewsletterRepositoryTests.cs:38` (inside local `Seed`), `:48` |
| `Mailer.Unit.Tests/Infrastructure/Persistence/MailerUnitOfWorkTests.cs:42` |
| `Mailer.Unit.Tests/Infrastructure/Services/EmailDispatcherTests.cs:57` (inside local `Pending()`) |
| `Mailer.Integration.Tests/.../GetNewsletterSubscribers/V1/AdminGetNewsletterSubscribersEndpointV1Tests.cs:39-42` |
| `Mailer.Integration.Tests/.../ConfirmNewsletter/V1/PublicConfirmNewsletterEndpointV1Tests.cs:24-27, 57-60, 97-100` |

### `NewsletterSubscriberFactory.CreateConfirmed(string email)` — 8 sites, 6 files

`PublicConfirmNewsletterHandlerTests.cs:87-91`, `PublicSubscribeNewsletterHandlerTests.cs:89-93`,
`PublicUnsubscribeNewsletterHandlerTests.cs:49-53`, `AdminGetNewsletterSubscribersEndpointV1Tests.cs:43-47`,
`PublicSubscribeNewsletterEndpointV1Tests.cs:43-47`, `PublicUnsubscribeNewsletterEndpointV1Tests.cs:24-28, 59-63, 95-99`.
Each is the identical 4-call chain; `At()` is never set at any site, so `_now` stays `DateTime.UtcNow`
either way.

### `NewsletterSubscriberFactory.CreateUnsubscribed(string email)` — 4 sites

`PublicConfirmNewsletterHandlerTests.cs:112-116`, `PublicSubscribeNewsletterHandlerTests.cs:58-62`,
`PublicUnsubscribeNewsletterHandlerTests.cs:74-78`, `PublicSubscribeNewsletterEndpointV1Tests.cs:67-71`.

### `NotificationFactory.CreatePasswordChanged(Guid userId)` — 6 sites, and the strongest single finding

**Six files hold a byte-identical private `static NotificationEntity CreateNotification(Guid userId)`**
(md5-verified identical):

- `Mailer.Unit.Tests/Application/Notifications/PublicGetNotificationsHandlerTests.cs:21-30`
- `.../PublicMarkAllNotificationsReadHandlerTests.cs:23-32`
- `.../PublicMarkNotificationReadHandlerTests.cs:29-38`
- `Mailer.Integration.Tests/.../MarkAllNotificationsRead/V1/PublicMarkAllNotificationsReadEndpointV1Tests.cs:16-25`
- `.../MarkNotificationRead/V1/PublicMarkNotificationReadEndpointV1Tests.cs:19-28`
- `.../GetUnreadNotificationCount/V1/PublicGetUnreadNotificationCountEndpointV1Tests.cs:16-25`

Two more can adopt it: `NotificationRepositoryTests.cs:40-46, 61-67` use `.WithContent("title","body")`
and assert only ids, `UserId`, `ReadAt` and counts; and
`PublicGetNotificationsEndpointV1Tests.cs:17-26` parameterises the title and asserts on it, so it needs
a `CreatePasswordChanged(Guid userId, string title)` overload — with which the factory reaches 9 of 9.

### `OutboxEmailFactory` — do **not** add in-module

The three in-module sites meet the count but share no chain: `OutboxEmailDispatcherJobTests.cs:66-72`
(template `"NewsletterWelcome"`, no recipient name), unit `OutboxEmailRepositoryTests.cs:34-40`
(same template, name `"Fan"`), integration `OutboxEmailRepositoryTests.cs:111-117` (template `"Welcome"`,
random address, `At(dueAt)`). Only `WithContent("subject","<p>body</p>","body")` is verbatim shared; a
factory covering all three would take four parameters and name nothing.

The real case is in end-to-end (below), and it is blocked on a missing project reference.

## End-to-end

**`tests/EndToEnd.Tests/EndToEnd.Tests.csproj` references Content, Identity and Storage `.TestData` but
not `Mailer.TestData`.** Confirmed. That blocks the one genuine outbox factory case:
`Workflows/EmailDeliveryFlowTests.cs:284-293, 321-330, 358-367, 400-409` each call
`OutboxEmailEntity.Enqueue(...)` inline, varying only address, subject and once the instant →
`OutboxEmailFactory.CreatePendingDueAt(Guid id, string recipientAddress, string subject, DateTime dueAt)`.

The suite is also internally split on request builders: 11 workflow files use them
(`TokenInvalidationFlowTests`, `AccountLockoutFlowTests`, `ExternalAccountLinkingFlowTests`,
`OrderTotalIntegrityFlowTests`, …) while these sites new the record where a builder exists:

| File | Sites | Builder |
| --- | --- | --- |
| `Workflows/AuthenticationFlowTests.cs` | `:39, 70, 111, 152, 208` | `PublicSignUpRequestBuilder` |
| | `:86, 124, 166, 227` | `PublicLoginRequestBuilder` |
| | `:179` | `PublicSignOutRequestBuilder` |
| `Workflows/EmailDeliveryFlowTests.cs` | `:49, 223` | `PublicSignUpRequestBuilder` |
| | `:78, 162, 258` | `PublicVerifyOtpRequestBuilder` |
| | `:154, 269` | `PublicForgotPasswordRequestBuilder` |
| | `:168` | `PublicResetPasswordRequestBuilder` |
| | `:195` | `PublicResendOtpRequestBuilder` |
| `Workflows/OrderLifecycleTests.cs` | `:116, 140, 151` | `AdminCreateOrderRequestBuilder` |
| `Workflows/InteractionFlowTests.cs` | `:179` | `PublicAddArticleCommentRequestBuilder` |
| | `:204` | `PublicRateVideoRequestBuilder` (its default star rating is already the literal used) |
| `Workflows/IdentitySecurityEventFlowTests.cs` | `:138` | `AdminAssignRoleToUserRequestBuilder` |

The only non-identical detail is the signup username, which changes from `m{guid}[..10]` to the
builder's `u{guid}[..10]`; same validator, and the tests that assert on it bind the value first.
Sites excluded because no builder exists: `PublicAddCommentReplyRequest`,
`AdminApproveLyricsSubmissionRequest`, `AdminRejectLyricsSubmissionRequest`, `AdminSetLyricsTagsRequest`,
`AdminDecideLyricsRevisionRequest`, `AdminDecideTranslationRevisionRequest`,
`AdminVerifyArtistOwnerRequest`, `AdminResolveSingleStreamingLinksRequest`.

## Shared

`Shared.Integration.Tests/Infrastructure/Persistence/CrossContextTransactionTests.cs:40` and `:69` new
`TagEntity.Create(...)` one line after using `FileFactory.CreateImage()`. `TagFactory.Create()` exists,
`TagBuilder` already generates a unique capped name and slug, the test asserts only that `tag.Id` round
trips, and the project already references `Content.TestData`. No new factory needed.

`Mailer.Unit.Tests/Infrastructure/Persistence/MailerUnitOfWorkTests.cs:96` news
`NewsletterSubscriberEntity.Subscribe(...)` inline while `:42` of the same file uses the builder.

## Storage

Nothing to adopt: **`new FileBuilder()` appears at zero call sites outside `FileFactory` itself**, so
every Storage test already goes through a factory and no unused member can have a hand-rolled twin. Five
are dead — see [11-dead-factories.md](11-dead-factories.md).

Six more `FileFactory` members have exactly one calling file (`Create(string)`, `CreateWithFileName`,
`CreateDeletedWithId` — all three only from `FileIdentificationSpecificationsTests.cs`;
`CreateWithMimeType` 2 files; `CreateVideo` 1 test file plus `FileReferenceDtoFactory`;
`FileReferenceDtoFactory.CreateWithColors` 1 file). By the stated rule they belong at the call site as
builder chains, but inlining them would introduce the first-ever direct `new FileBuilder()` in a test.
Flagged, not recommended.
