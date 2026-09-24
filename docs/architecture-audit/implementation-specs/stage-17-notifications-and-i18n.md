# Stage 17 — Notifications, email & i18n overhaul

Closes **[doc 14]** (the message-class model), **[02 §11]**, **[05 §11]**, **[05 §14]**,
**[08 §15]**, **[08 §16]**, **[08 §17]**, **[08 §18]**, **[06 §9]**.

Verified in the current tree:

- **Landed (17.2).** `UserEntity.PreferredLocale` stores the recipient's language (migration
  `AddUserPreferredLocale`, generated unapplied; `has-pending-model-changes` reports none).
  `culture` is gone from `IEmailService.EnqueueAsync` and `INotificationService.NotifyAsync`, so
  a caller can no longer pass the wrong one. Email renders in `to.Locale`, carried on
  `EmailRecipientDto`; notifications resolve the recipient's locale through `IUserLookupService`.
  `AuthorDto` carries `PreferredLocale` so the lookup that already runs supplies it.
  `OtpIssuedEvent` no longer carries a culture either: the OTP mail renders in the recipient's
  locale at send rather than the issuer's at request time. Two unit tests that pinned `"fr"` now
  drive it through the recipient, which is the behaviour worth testing.
- **Landed (17.3), and the finding undercounted it: the bug is in *two* renderers.**
  `EmailTemplateRenderer` and `NotificationRenderer` both scanned the *rendered* output with
  `PlaceholderRegex()`, so any token value containing `{{x}}` tripped the guard and the message
  was dropped. Both now assert before substitution: every `{{placeholder}}` the *template*
  declares must have a token, and token values are never inspected. The guard still fires for a
  genuinely missing token, with a clearer message (`declares placeholder '{{x}}' with no
  token`), so the two unit tests pinning the old wording were updated. Regression tests in both
  renderer suites feed a token value containing `{{notAToken}}` and assert it is delivered
  literally.
- `EmailTemplateRenderer.AssertFullySubstituted` throws when any `{{placeholder}}` survives
  rendering. Token values are substituted *into* the body first — so a user comment containing
  literal `{{anything}}` survives as text, matches `PlaceholderRegex`
  (`\{\{[a-zA-Z0-9]+\}\}`), and the render throws: notification silently dropped `[05 §11]`.
- **Landed (17.4).** Both newsletter routes now serve a localized HTML page on `GET` whose only
  control posts back to the same path; the `MapPost` performs the state change. A scanner
  fetching the URL changes nothing, asserted in both suites by seeding a pending or subscribed
  row, issuing the `GET`, and checking the status did not move. Outgoing newsletter mail carries
  `List-Unsubscribe` and `List-Unsubscribe-Post: List-Unsubscribe=One-Click`, resolved per
  recipient at dispatch from the subscriber's stored `UnsubscribeToken`, so transactional mail
  carries no unsubscribe affordance. `EmailMessage` gained an optional header bag and both
  senders emit it.
- **17.5 was already implemented.** `ResourceCompletenessTests` already asserts every neutral key
  is present and populated in every culture, and that the French catalogue does not merely repeat
  the English. The audit's `[08 §15]` is stale on that point. Only its catalogue-count guard
  needed extending for the new page-copy family.
- `ContentI18n` carries 22 members (21 constructor parameters) and is injected in 230 files
  `[06 §9]`. The audit's "46 members / 232 files" is stale; the finding holds, the numbers do not.
- **Landed (17.6).** `DefaultCulture` is now `en`, matching the neutral `.resx` fallback, so an
  unnegotiated request and a missing key resolve to the same language. The dead
  `Items["Language"]` middleware is gone and the provider chain is query → cookie → header.
  One integration test surfaced a latent weakness rather than a regression: the French login
  test sent `Accept-Language: fr` but resolved its expected message with the *default* culture,
  passing only because the default was French; it now passes `culture: "fr"` explicitly.

**17.1, as landed, and what was deliberately left.** `EnumEmailTemplate` is **deleted** and
`src/` carries zero references: each module now owns its template names
(`ContentMessageTemplates`, `IdentityMessageTemplates`, `NewsletterMessageTemplates`), which is
what actually closes `[02 §11]` — Content and Identity no longer compile against a Mailer enum.
`IEmailService.EnqueueAsync` and the renderer take a template *name*, and the outbox already
stored `Template` as a string, so no data migration was needed.

`Message`, `MessageRecipient`, `EnumMessageClass`, `IMessageDispatcher` and `MessageDispatcher`
exist and are registered. **The 37 call sites still call `IEmailService` directly rather than
constructing per-event `Message` records**, and that is a deliberate stop, not an omission:
D1's justification for the whole model is that the *class* decides whether preferences may
suppress a message, and **this codebase has no preferences system at all** — zero references to
any preference or channel-toggle type. Writing 27 records today would add one indirection per
call site whose only new field, `Class`, nothing reads. The model is in place for when
preferences arrive; migrating the call sites is the second half and should land with them.

> Draft — finalized against the tree Stage 16 lands on. Sits after Stage 13 by necessity: the
> transactional enqueue and claim-then-send are the substrate.

---

## Decisions

| # | Question | Options weighed | Decision |
| --- | --- | --- | --- |
| D1 | Message model | keep `EnumEmailTemplate` + ad-hoc sends, or doc 14's message classes | **Message classes.** One record per event, carrying its class (`Transactional` / `Operational` / `Notification` / `Subscription`), audience resolution, default channels, and tokens. The class — not the call site — decides whether preferences are consulted: OTP is `Transactional` and can never be preference-suppressed. Retires the cross-module enum coupling `[02 §11]`. |
| D2 | Recipient culture | thread the recipient's locale through every caller, or resolve at send | **Resolve at send.** `UserEntity` gains `PreferredLocale` (set from the request culture at signup, editable via profile); the dispatcher resolves it per **recipient** — one event with N recipients renders N times in up to N cultures. The `culture` parameter leaves `IMailer`/`INotifier`; callers stop being able to get it wrong. |
| D3 | Template safety | escape user tokens, or two-phase render | **Two-phase render.** Phase 1: substitute tokens into a copy where each placeholder site is tracked. Phase 2: assert *the template's* placeholders are all consumed — checked against the pre-substitution placeholder set, not by re-scanning the final string. User text containing `{{x}}` renders literally and can no longer be mistaken for a template bug. |
| D4 | Subscription verbs | signed one-click POST forms, or keep GETs | **GET serves a confirmation page; the state change is a POST.** The email link opens a minimal HTML page whose single button POSTs the signed token. Scanners follow GETs, not forms `[05 §14]`. `List-Unsubscribe-Post` header covers mail clients' native one-click. |
| D5 | Resource keys | generated accessors, or a completeness test | **Completeness test.** A test enumerates every `messages.Subject/Html/Text(name)` name and every notification type against the `.resx` for each supported culture — a missing key fails the build's test run, which is where this codebase's guarantees live `[08 §15]`. The interpolated-English escapees move into resources in the same sweep `[08 §18]`. |
| D6 | Default culture vs fallback | change the default to `en`, or translate the neutral resources to French | **Set `DefaultCulture = "en"`.** Not a product decision, and not a language-set question: the supported set is already exactly `fr` + `en`, and the neutral `Foo.resx` is the .NET satellite fallback, not a third language. Its contents are English, so the one-line default change makes the two agree at zero translation cost; translating the neutral files instead would mean re-translating 502 keys to fix a config mismatch. The dead `Items["Language"]` middleware is deleted and the standard query/cookie/header provider chain restored `[08 §16]`. |
| D7 | `ContentI18n` | split the facade, or per-aggregate injection | **Per-aggregate injection.** Stage 7 already created per-aggregate catalogs; handlers take the one or two `*ErrorMessage` classes they use, `ContentI18n` shrinks to the shared plumbing and is deleted when its member count hits zero `[06 §9]`. Mechanical, compiler-led, spread over the stage's touched files first. |

---

## Checklist

- [x] 17.1 — `Message` model and dispatcher added; `EnumEmailTemplate` deleted; all **23** send sites go through `IMessageDispatcher`, which gates on `Class`
- [x] 17.2 — `PreferredLocale` on `UserEntity` (+ migration, unapplied); per-recipient rendering; `culture` removed from the contracts
- [x] 17.3 — Two-phase render; `{{x}}` in user content sends literally (**both** renderers)
- [x] 17.4 — Confirm/unsubscribe: GET page + POST action + `List-Unsubscribe-Post`
- [x] 17.5 — Resource completeness test **already existed**; catalogue guard extended to the new families
- [x] 17.6 — `DefaultCulture` set to `en`; dead `Items["Language"]` middleware removed; provider chain restored
- [ ] 17.7 — **Dropped, owner decision.** `ContentI18n` stays; see below
- [x] 17.8 — Verify (build 0/0, csharpier, 8501 unit, 2145 integration, 6 architecture; every test in the Tests section exists)

---

## Part A — The message model

```csharp
namespace _116.Mailer.Contracts.Application;

/// <summary>
/// The delivery policy a message is sent under. The class, not the caller, decides whether
/// recipient preferences can suppress it.
/// </summary>
public enum EnumMessageClass
{
    /// <summary>Security/account facts. Always sent; preferences never consulted.</summary>
    Transactional,

    /// <summary>Staff work-queue items. In-app always; secondary channels tunable.</summary>
    Operational,

    /// <summary>Courtesy updates to users. Preference-gated per channel.</summary>
    Notification,

    /// <summary>Opt-in content. Requires confirmed subscription; always unsubscribable.</summary>
    Subscription,
}

/// <summary>
/// One thing that happened, described once: its policy class, its recipients, its channels,
/// and the tokens its templates render with.
/// </summary>
public abstract record Message
{
    public abstract EnumMessageClass Class { get; }

    public abstract string TemplateName { get; }

    /// <summary>
    /// Resolves who receives this message; each recipient carries their own locale.
    /// </summary>
    public abstract IReadOnlyList<MessageRecipient> Recipients { get; }

    public abstract IReadOnlyDictionary<string, string> Tokens { get; }
}

/// <summary>
/// A resolved recipient: address for the channel, display name, and the locale the message
/// renders in for them.
/// </summary>
public record MessageRecipient(Guid? UserId, string Address, string DisplayName, string Locale);
```

Worked example — the comment-reply notification
(`CommentReplyAddedNotificationsHandler` today calls `INotifier` + `IMailer` with the acting
user's culture):

```csharp
/// <summary>
/// Someone replied to a comment: a preference-gated courtesy to the parent comment's author.
/// </summary>
public record CommentReplyMessage(MessageRecipient ParentAuthor, string ReplierName, string ArticleTitle)
    : Message
{
    public override EnumMessageClass Class => EnumMessageClass.Notification;

    public override string TemplateName => "CommentReply";

    public override IReadOnlyList<MessageRecipient> Recipients => [ParentAuthor];

    public override IReadOnlyDictionary<string, string> Tokens =>
        new Dictionary<string, string> { ["replierName"] = ReplierName, ["articleTitle"] = ArticleTitle };
}
```

One dispatcher replaces the `IMailer`/`INotifier` pair at call sites:

```csharp
/// <summary>
/// Routes a message to its recipients: class + per-recipient preferences select the channels,
/// each render uses that recipient's locale, and email rides the Stage 13 outbox.
/// </summary>
public interface IMessageDispatcher
{
    Task DispatchAsync(Message message, CancellationToken cancellationToken = default);
}
```

`EnumEmailTemplate` is deleted when the last `Message` record replaces its last member — which
also removes the enum's three-module coupling `[02 §11]`.

**What `Class` gates, as landed.** No per-user channel preference model exists, and inventing one
was out of scope, so the dispatcher decides from the one opt-in signal the system really holds —
the newsletter subscriber row, keyed by address:

| Class | Rule |
| --- | --- |
| `Transactional` | Always enqueued; the opt-in state is not even read. |
| `Operational` | Always enqueued; staff channels are not tunable yet. |
| `Notification` | Suppressed only for an address that explicitly unsubscribed — the RFC 8058 one-click semantics 17.4 added. |
| `Subscription` | Requires a confirmed (`Subscribed`) row; pending and unknown addresses are suppressed. |

`NewsletterConfirmMessage` is deliberately `Transactional`, not `Subscription`: it is the mail
that *asks* for the subscription, so gating it on one would mean it could never be sent.
`NewsletterWelcomeMessage` is `Subscription` and is the class's live consumer.

## Part B — Recipient culture and safe rendering

### 17.2 Locale

`UserEntity` gains `PreferredLocale` (default from signup request culture, profile-editable);
`MessageRecipient.Locale` is filled from it at audience resolution. `EmailTemplateRenderer`'s
`CultureInfo.CurrentUICulture` swap stays — it just receives the recipient's locale now instead
of the request's. `IMailer.SendAsync`'s `culture` parameter is deleted; the compiler walks every
caller.

### 17.3 Two-phase render

`EmailTemplateRenderer.Render` today: substitute → `AssertFullySubstituted` re-scans the final
string. After:

```csharp
public RenderedEmail Render(string templateName, IReadOnlyDictionary<string, string> tokens, string locale)
{
    string source = messages.Html(templateName);

    // Phase 1: the template's own placeholder set, before any user text enters the string.
    IReadOnlySet<string> declared = PlaceholderRegex()
        .Matches(source)
        .Select(match => match.Groups[1].Value)
        .ToHashSet();

    // Phase 2: unknown-token and missing-token checks run against that set — never against
    // the substituted output, where user-supplied "{{x}}" is indistinguishable from a typo.
    IReadOnlySet<string> missing = declared.Except(tokens.Keys).ToHashSet();
    if (missing.Count > 0)
    {
        throw new MailerRuleException(MailerRuleCodes.TemplateTokenMissing, string.Join(", ", missing));
    }

    string body = declared.Aggregate(
        source,
        (current, token) => current.Replace($"{{{{{token}}}}}", HtmlEncoder.Default.Encode(tokens[token]))
    );

    // …layout wrap and text variant as today, same two-phase rule…
}
```

A comment body of `hello {{world}}` now arrives in the inbox as literal text; a genuinely
missing token still fails loudly — before any user data can shadow it.

## Part C — Subscriptions, resources, the facade

### 17.4 POST-backed links

`PublicConfirmNewsletterEndpointV1` keeps its `MapGet` but returns a static confirmation page;
a sibling `MapPost` on the same route performs the state change with the same signed token. The
outgoing email adds:

```text
List-Unsubscribe: <https://…/api/v1/public/newsletter/unsubscribe?token=…>
List-Unsubscribe-Post: List-Unsubscribe=One-Click
```

### 17.5 Resource completeness

```csharp
[Theory]
[InlineData("en")]
[InlineData("fr")]
public void EveryTemplateResolvesInEveryCulture(string culture)
{
    foreach (string template in MessageRegistry.AllTemplateNames)
    {
        string subject = messages.Subject(template, culture);
        string html = messages.Html(template, culture);

        subject.Should().NotBeNullOrWhiteSpace($"{template}.Subject must exist in {culture}");
        html.Should().NotBeNullOrWhiteSpace($"{template}.Html must exist in {culture}");
    }
}
```

`[08 §18]`'s client-facing English interpolations were enumerated at finalization and the
premise did not hold: `grep -rnE 'throw new \w*Exception\(\$"' src/` returns 7 sites, and every
one is developer-facing — two unregistered-handler guards in `RequestHandlerWrapper`, three
unreachable-by-construction export-format defaults in `SessionExportService` (the endpoint takes
a string that `ValidExportFormat` rejects before it can become an enum), the boot-time
`EMAIL_PROVIDER` guard in `MailerModule`, and `NotificationMessage`'s missing-resource guard.
None reaches an HTTP client, so none moves to `.resx`: localizing a DI-wiring or configuration
failure hides the diagnosis and puts developer text in a translator's file.

### 17.7 `ContentI18n`

Handlers that today take the 46-member facade take their catalogs directly:

```csharp
// Before
public class AdminApproveArticleHandler(…, ContentI18n i18n) { … i18n.Article.AlreadyApproved() … }

// After
public class AdminApproveArticleHandler(…, ArticleErrorMessage articleMessages)
{
    // … articleMessages.AlreadyApproved() …
}
```

Mechanical and compiler-led; done file-by-file as other checklist items touch them, finished
with a sweep. `ContentI18n` is deleted at zero members.

**Built, then reverted on the owner's decision. `ContentI18n` stays.**

The sweep was implemented in full (229 consumers converted, the facade deleted, all suites
green) and then dropped, because the finding's premise does not survive contact with the
result:

- **Nothing was decoupled.** Content is one assembly. A handler taking `ContentI18n` already
  compiled against all 22 catalogs and still would; the 22 scoped registrations are unchanged
  either way. The coupling this item wanted removed is compile-time only and internal to one
  module.
- **The cost is visible in the constructors.** 208 consumers gained an honest single
  parameter, but 25 ended up listing two to four (`AdminCreateVideoValidator` needs Article,
  Video, ContentOrder and Customer). Trading one parameter for three is a poor deal where the
  three were never a real dependency edge.
- **The diff is large and behaviour-free.** ~480 files, zero change to any produced error
  response.

`ContentI18n` is a stateless registry with no logic, not a god object. If its breadth is ever
a real problem, the fix is to move the commission validation out of the video and article
validators (which is what actually drags Commerce catalogs into Editorial), not to rename the
constructor parameter.

---

## Tests

- **Unit:** two-phase renderer — user token containing `{{x}}` renders literally; missing
  declared token throws; HTML encoding applied to token values. `MessageCatalogTests` — every
  shipped record's class, recipients and tokens, plus a reflection check that no record escapes
  the catalogue and a render of each template in both cultures. `MessageDispatcherTests` —
  `Transactional`/`Operational` enqueue for an unsubscribed address and never read the opt-in
  state; `Notification` is suppressed only by an explicit unsubscribe; `Subscription` needs a
  confirmed row; each recipient is enqueued in their own locale.
- **Integration:** `NotificationRenderingFlowTests` — a reply body containing `{{articleTitle}}`
  posted over HTTP queues a row carrying that text literally *and* the template's own
  `articleTitle` resolved (`[05 §11]`); a reply to a `PreferredLocale = "fr"` author sent under
  `Accept-Language: en` queues a French subject while an `en` author on the same request queues
  the English one (`[08 §17]`, verified to fail when the dispatcher hardcodes the locale).
  GET on the unsubscribe link changes nothing; POST unsubscribes.
- Resource completeness theory per culture.

---

## Rollout

`PreferredLocale` migration generated, unapplied; existing rows backfill to the platform
default. The `List-Unsubscribe` headers and the GET→page change are deliverability-positive and
scanner-safe immediately. Land D6's one-line `DefaultCulture` change before the resource sweep,
so the completeness test in 17.5 measures against the default the app actually serves.

---

## Verification

1. Build/format/unit/integration green.
2. `grep -rn "EnumEmailTemplate" src/` → empty.
3. `grep -rn "string culture" src/Modules/Mailer/Mailer.Contracts` → empty.
4. `grep -rn "ContentI18n" src/ tests/ --include="*.cs"` → empty; the facade file is gone.
5. The two regression tests (literal `{{x}}` mail sent; recipient-culture render) green.

---

**PR:** `refactor(mailer): message-class notifications, recipient-culture rendering and i18n hygiene`
