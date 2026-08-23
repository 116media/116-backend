# Stage 17 — Notifications, email & i18n overhaul

Closes **[doc 14]** (the message-class model), **[02 §11]**, **[05 §11]**, **[05 §14]**,
**[08 §15]**, **[08 §16]**, **[08 §17]**, **[08 §18]**, **[06 §9]**.

Verified in the current tree:

- `IMailer.SendAsync(...)` takes `string culture` documented as "the two-letter **request**
  culture" — every email renders in the *acting caller's* language. A French admin rejecting an
  English artist's submission sends that artist a French rejection `[08 §17]`.
- `EmailTemplateRenderer.AssertFullySubstituted` throws when any `{{placeholder}}` survives
  rendering. Token values are substituted *into* the body first — so a user comment containing
  literal `{{anything}}` survives as text, matches `PlaceholderRegex`
  (`\{\{[a-zA-Z0-9]+\}\}`), and the render throws: notification silently dropped `[05 §11]`.
- `PublicConfirmNewsletterEndpointV1` and `PublicUnsubscribeNewsletterEndpointV1` are `MapGet`
  with side effects — link scanners confirm and unsubscribe people `[05 §14]`.
- `ContentI18n` carries 46 injected members and is constructed on 232 files `[06 §9]`.

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
| D6 | Language set | finish the second language, or cut to what's real | **Decide with product, execute here.** "Three languages" is `en` + `fr` + an English copy; the dead second-language middleware config goes either way `[08 §16]`. The spec carries both branches costed; the checklist pins the chosen one at finalization. |
| D7 | `ContentI18n` | split the facade, or per-aggregate injection | **Per-aggregate injection.** Stage 7 already created per-aggregate catalogs; handlers take the one or two `*ErrorMessage` classes they use, `ContentI18n` shrinks to the shared plumbing and is deleted when its member count hits zero `[06 §9]`. Mechanical, compiler-led, spread over the stage's touched files first. |

---

## Checklist

- [ ] 17.1 — `Message` model: class, audience, channels; registry replaces `EnumEmailTemplate`
- [ ] 17.2 — `PreferredLocale` on `UserEntity` (+ migration, unapplied); per-recipient rendering; `culture` removed from the contracts
- [ ] 17.3 — Two-phase render; `{{x}}` in user content sends literally
- [ ] 17.4 — Confirm/unsubscribe: GET page + POST action + `List-Unsubscribe-Post`
- [ ] 17.5 — Resource completeness test; interpolated strings into `.resx`
- [ ] 17.6 — Language-set decision executed; dead middleware config removed
- [ ] 17.7 — `ContentI18n` split; handlers take their own catalogs
- [ ] 17.8 — Verify (build 0/0, csharpier, unit, integration)

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

The client-facing English interpolations (`[08 §18]`, enumerated at finalization by grepping
interpolated throws outside the resource path) move into the `.resx` files this test then
guards.

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

---

## Tests

- **Unit:** two-phase renderer — user token containing `{{x}}` renders literally; missing
  declared token throws `TemplateTokenMissing`; HTML encoding applied to token values. Message
  records: class/recipients/tokens per event. Dispatcher: `Transactional` ignores preferences,
  `Notification` respects the channel toggle.
- **Integration:** comment containing `{{anything}}` over HTTP → outbox email row exists and
  renders (the `[05 §11]` regression — fails today). Reply to a French user from an English
  session → the outbox row's rendered body is French (`[08 §17]` regression — fails today).
  GET on the unsubscribe link changes nothing; POST unsubscribes.
- Resource completeness theory per culture.

---

## Rollout

`PreferredLocale` migration generated, unapplied; existing rows backfill to the platform
default. The `List-Unsubscribe` headers and the GET→page change are deliverability-positive and
scanner-safe immediately. Coordinate D6's language decision before the resource sweep, not
after.

---

## Verification

1. Build/format/unit/integration green.
2. `grep -rn "EnumEmailTemplate" src/` → empty.
3. `grep -rn "string culture" src/Modules/Mailer/Mailer.Contracts` → empty.
4. `grep -rn "ContentI18n" src/Modules/Content --include="*Handler.cs"` → empty.
5. The two regression tests (literal `{{x}}` mail sent; recipient-culture render) green.

---

**PR:** `refactor(mailer): message-class notifications, recipient-culture rendering and i18n hygiene`
