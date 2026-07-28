# Spec 03 — Provider Adapters and Selection

## Goal

Ship two concrete `IEmailSender` adapters — SMTP (covers Mailpit in dev and any
relay in prod) and Resend (first HTTP-API provider) — plus the startup switch
that picks one from configuration. Define the recipe every future provider
follows.

## Adapter 1 — SmtpEmailSender (MailKit)

`Infrastructure/Services/SmtpEmailSender.cs`, using **MailKit** (the
Microsoft-recommended replacement for the obsolete `System.Net.Mail.SmtpClient`;
pin the latest via `npm`-equivalent check: `dotnet add package MailKit` after
confirming the latest version on nuget.org).

- Reads `SMTP_HOST`, `SMTP_PORT`, `SMTP_USERNAME`, `SMTP_PASSWORD`,
  `SMTP_USE_STARTTLS` from `IConfiguration` (env-var style, like
  `OdesliStreamingLinkResolutionService` reads `ODESLI_API_URL`).
- Empty username/password ⇒ skip AUTH (Mailpit needs none).
- Builds a `MimeMessage` from `EmailMessage` (html + text as multipart/alternative),
  `From` from `EMAIL_FROM_ADDRESS`/`EMAIL_FROM_NAME`.
- Failure mapping: `SmtpCommandException` with a 5xx status ⇒
  `EmailDeliveryException(isTransient: false)`; socket/timeout/4xx ⇒ transient.

## Adapter 2 — ResendEmailSender (typed HTTP client)

`Infrastructure/Services/ResendEmailSender.cs`, following the
`YoutubeThumbnailService`/Odesli typed-`HttpClient` precedent — **no SDK
package**, one `POST https://api.resend.com/emails` with a JSON body
(`from`, `to`, `subject`, `html`, `text`) and a bearer key from
`RESEND_API_KEY`.

- Registered with `AddHttpClient<...>` and a 10-second timeout, like the
  Odesli client.
- Failure mapping: HTTP 429 and 5xx ⇒ transient; 4xx (invalid payload,
  unverified domain) ⇒ permanent.

## Selection at startup

**Where the value comes from:** `Program.cs` calls `Env.Load()` /
`Env.TraversePath().Load()` (DotNetEnv), which loads the repo's `.env` file
into process environment variables; in docker, compose injects the same
variables directly. Startup code reads them through the existing
`AppEnvironment` accessor class
(`src/Shared/Shared/Application/Configurations/Environment.cs`) — the same
mechanism as `AppEnvironment.Jwt()`, `AppEnvironment.Cloudinary()` and
`AppEnvironment.CorsAllowedOrigins()`. This spec adds an
`AppEnvironment.EmailProvider()` accessor rather than reading raw
`configuration["EMAIL_PROVIDER"]` in the module.

**No magic strings:** the provider names are constants in `MailerConstants`
(house rule — compare `RateLimitPolicies`, `RoleConstants`), so the accepted
values exist in exactly one place:

```csharp
public static class MailerConstants
{
    public static class EmailProviders
    {
        public const string Smtp = "smtp";
        public const string Resend = "resend";
    }
}
```

The switch in `MailerModule.RegisterModule`:

```csharp
string provider = AppEnvironment.EmailProvider() ?? MailerConstants.EmailProviders.Smtp;

switch (provider.ToLowerInvariant())
{
    case MailerConstants.EmailProviders.Smtp:
        services.AddSingleton<IEmailSender, SmtpEmailSender>();
        break;
    case MailerConstants.EmailProviders.Resend:
        services.AddHttpClient<IEmailSender, ResendEmailSender>(client =>
            client.Timeout = TimeSpan.FromSeconds(10));
        break;
    default:
        throw new InvalidOperationException($"Unknown EMAIL_PROVIDER '{provider}'.");
}
```

- Defaulting to SMTP is deliberate: a fresh dev checkout works against
  Mailpit with zero extra configuration (`SMTP_HOST`/`SMTP_PORT` also default
  to Mailpit's, spec 08).
- An unknown value fails **at boot**, not at first send — misconfiguration
  must be loud.
- The adapters themselves keep reading their own settings via injected
  `IConfiguration` (`SMTP_HOST`, `RESEND_API_KEY`, …) following the
  `OdesliStreamingLinkResolutionService` precedent — that is what lets the
  loopback integration tests feed them an in-memory configuration.

## Add-a-provider recipe (the swap guarantee, made concrete)

Adding SendGrid/Mailgun/SES later is exactly this, nothing more:

1. New `XxxEmailSender : IEmailSender` in `Infrastructure/Services/`,
   translating `EmailMessage` to the provider's API and mapping failures to
   `EmailDeliveryException` with the right `IsTransient`.
2. New `case "xxx":` branch in the selection switch.
3. New env vars documented in `.env.template` and spec 08's table.
4. Loopback integration tests for the adapter (spec 09 pattern).

No template, port, outbox, endpoint or consumer file changes. If a provider
change ever requires touching anything outside `Infrastructure/Services/` and
configuration, the abstraction has been broken — treat it as a defect.

## Checklist

- [x] MailKit pinned at its latest version; `SmtpEmailSender` implemented
- [x] `ResendEmailSender` implemented as a typed HTTP client, no SDK
- [x] `EMAIL_PROVIDER` switch with loud failure on unknown values
- [x] Both adapters map transient vs permanent failures per the tables above
- [x] Loopback integration tests green for both adapters (spec 09)

## Implementation notes

- MailKit pinned at 4.17.0 (latest on nuget at implementation time).
- Selection reads `AppEnvironment.EmailProvider()` with names from
  `MailerConstants.EmailProviders`; unknown values throw at boot as specced.
