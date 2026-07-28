# Spec 04 — Templates and Localization

## Goal

A template catalog that renders every product email (subject + HTML body +
text body) in the request culture, using the same `.resx` localization
machinery the error messages already use — no new templating engine, no Razor
compilation, no external dependency.

## Template catalog

`Domain/Enums/EnumEmailTemplate.cs` — one member per product email:

| Template | Trigger | Required tokens |
| --- | --- | --- |
| `EmailVerificationOtp` | Public signup (`EnumOtpPurpose.EmailVerification`) | `userName`, `otpCode`, `expiryMinutes` |
| `PasswordResetOtp` | Forgot password (`EnumOtpPurpose.PasswordReset`) | `userName`, `otpCode`, `expiryMinutes` |
| `Welcome` | First successful email verification | `userName` |
| `LoginAlert` | Successful login from a new device/IP | `userName`, `loginTime`, `ipAddress`, `deviceSummary` |
| `NewsletterConfirm` | Newsletter double opt-in (spec 07) | `confirmUrl` |
| `NewsletterWelcome` | Confirmed newsletter subscription | `unsubscribeUrl` |

The enum is append-only, like `EnumStreamingPlatform`.

`EnumOtpPurpose` also declares `TwoFactorAuthentication` and `AccountRecovery`,
but **no code path creates an OTP with either purpose today** — they exist only
as enum members. No template is defined for a flow that does not exist; when a
2FA or account-recovery flow ships, its template is appended here with it.

Specs 11–13 append further members for their consumer waves (eight security
templates, eight commerce templates, one engagement template) — each spec
carries its own template table; the rendering, key-naming and localization
rules in this spec apply to all of them unchanged.

## Rendering

`Application/Templates/EmailTemplateRenderer.cs` (registered behind
`IEmailTemplateRenderer`):

```csharp
public interface IEmailTemplateRenderer
{
    /// <summary>
    /// Produces the fully rendered subject, html and text parts for a template
    /// in the given culture. Throws when a required token is missing — a
    /// missing token is a programming error, never a runtime state.
    /// </summary>
    RenderedEmail Render(
        EnumEmailTemplate template,
        IReadOnlyDictionary<string, string> tokens,
        string culture
    );
}

public record RenderedEmail(string Subject, string HtmlBody, string TextBody);
```

- Token syntax in resources: `{{otpCode}}`. Rendering replaces every
  `{{token}}` occurrence and then **fails loudly** if any `{{…}}` placeholder
  survives (catches both missing tokens and typos in resources).
- Token values are HTML-encoded when substituted into the HTML part, raw in
  the text part.

## Resource layout

Follow the `XxxErrorMessage` + `.resx` triple exactly:

```text
Application/Templates/Messages/
├── EmailTemplateMessage.cs        # localizer facade, one method per template part
├── EmailTemplateMessage.resx      # neutral (en source of truth)
├── EmailTemplateMessage.en.resx
└── EmailTemplateMessage.fr.resx
```

Keys are `<Template>Subject`, `<Template>Html`, `<Template>Text` — e.g.
`PasswordResetOtpSubject`, `PasswordResetOtpHtml`, `PasswordResetOtpText`.

HTML bodies are stored as complete minimal documents (inline styles only, no
external assets, tables-free single column) — the norm for transactional email
clients. One shared layout string (`LayoutHtml` key with a `{{content}}`
token) wraps every body so branding changes are one-key edits.

## Culture resolution

- The caller passes the two-letter culture captured from the request
  (`CultureInfo.CurrentUICulture` at enqueue time).
- `fr` resolves the `.fr.resx` entries; anything else falls back to neutral —
  identical to error-message behavior, no new mechanism.

## Checklist

- [x] `EnumEmailTemplate` with the six members above
- [x] `EmailTemplateMessage` facade + neutral/en/fr resources for all keys
- [x] Renderer substitutes, HTML-encodes, and throws on unresolved placeholders
- [x] Shared layout wrapper applied to every HTML body
- [x] Unit tests: every template renders in both cultures with no leftover
      placeholders; missing-token throws (spec 09)

## Implementation notes

- `EnumEmailTemplate` lives in `Mailer.Contracts` (consumers reference it in
  `IMailer` calls), not `Domain/Enums` — 23 members: the six core templates
  plus specs 11-13's additions.
- `EmailTemplateMessage` exposes uniform `Subject/Html/Text(template)`
  accessors instead of one method per template — the catalog is uniform by
  construction and the renderer addresses it by name.
