# Spec 01 — Mailer Module Skeleton

## Goal

Stand up an empty but fully wired `Mailer` module: folder layout, DI
registration, its own PostgreSQL schema and `DbContext`, and a first (empty)
migration — so every later spec only adds files inside an already-running
module.

## Why a module (and not a Shared service)

- The service owns **state**: outbox rows and newsletter subscribers need
  tables, migrations and a schema — that is module territory, matching how
  `core`, `authentication` and `content` schemas each belong to one module.
- It has its own **API surface** (newsletter endpoints) and **background
  worker** (outbox dispatcher).
- Other modules consume it through a public application port (`IMailer`),
  exactly like Content consumes nothing from Core except `ICloudinaryService`
  via DI — no project-reference cycles.

## Folder layout

```text
src/Modules/Mailer/Mailer/
├── MailerModule.cs
├── Domain/
│   ├── Entities/            # OutboxEmailEntity (05), NewsletterSubscriberEntity (07)
│   ├── Enums/               # EnumOutboxEmailStatus (05), EnumEmailTemplate (04)
│   └── Constants/           # MailerConstants
├── Application/
│   ├── Shared/
│   │   ├── Services/        # IMailer, IEmailSender + their carrier records (02)
│   │   ├── Errors/          # NewsletterErrors + Messages/ (07)
│   │   └── Repositories/    # IOutboxEmailRepository, INewsletterRepository
│   ├── Templates/           # template catalog + .resx (04)
│   └── Newsletter/
│       └── UseCases/        # Public/ and Admin/ command + query slices (07)
└── Infrastructure/
    ├── Persistence/
    │   ├── MailerDbContext.cs
    │   ├── Configurations/
    │   └── Migrations/
    ├── Repositories/
    └── Services/            # SmtpEmailSender, ResendEmailSender (03), dispatcher (05)
```

`tests/Unit/Modules/Mailer/` and `tests/Integration/Modules/Mailer/` mirror the
`src` tree, per the testing rulebook.

## MailerModule registration

`MailerModule` extends `BaseModule` like `ContentModule`:

```csharp
public class MailerModule : BaseModule
{
    public override void RegisterModule(IServiceCollection services, IConfiguration configuration)
    {
        // DbContext on the "mailer" schema, snake_case naming — same options
        // pipeline as ContentModule.
        // Repositories, IMailer implementation, provider adapter selection (03),
        // outbox dispatcher hosted service (05).
    }
}
```

`Program.cs` picks it up next to the existing modules:

```csharp
Assembly mailerAssembly = typeof(MailerModule).Assembly;   // Carter + validators scan

builder.Services.AddIdentityModule().AddCoreModule().AddContentModule().AddMailerModule();
app.UseIdentityModule().UseCoreModule().UseContentModule().UseMailerModule();
```

## Database

- Schema: `mailer` (snake_case columns via EFCore.NamingConventions, automatic).
- First migration `InitMailerSchema` creates the schema only; the outbox and
  newsletter tables arrive with specs 05 and 07 in their own migrations.

```bash
dotnet ef migrations add InitMailerSchema \
  --project src/Modules/Mailer/Infrastructure \
  --startup-project src/Api \
  --context MailerDbContext
```

(Adjust `--project` to the real csproj path if the module is single-project
like Content — mirror whatever Content's migration command uses.)

## Checklist

- [x] Module folders created, `MailerModule` registered in `Program.cs`
- [x] `MailerDbContext` on schema `mailer`, wired into the module options pipeline
- [x] Carter assembly + FluentValidation scan includes the Mailer assembly
- [x] `InitMailerSchema` migration generated (left unapplied, per house practice)
- [x] `dotnet build` clean; API boots with the empty module

## Implementation notes

- The solution gained two projects: `Mailer.Contracts` (consumed by Identity
  and Content) and `Mailer`, mirroring the `Identity.Contracts` split.
- Modules here are static extension classes (`AddMailerModule` /
  `UseMailerModule` over `BaseModule.AddModuleDatabase`), matching
  `ContentModule` — not `BaseModule` subclasses as older docs suggested.
- One migration (`AddMailerOutboxAndNewsletter`, under
  `Infrastructure/Persistence/Migrations`) creates the schema and both tables
  in a single step instead of the three staged migrations sketched here — the
  whole model landed at once, so staging would have been artificial.
