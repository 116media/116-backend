# Stage 21 — Application services

One stage, two halves of the same problem. The word `Service` names two different things in this
tree, the word `Factory` names a third thing that is really the first, and 53 handlers are over
the dependency budget because the phases they should delegate have nowhere unambiguous to go.
Fixing the vocabulary and cutting the handlers apart are the same edit: every extraction this
stage specifies creates one more of exactly the kind of class the naming rules are about, so
doing them separately means touching the same files twice.

Measured on the current tree:

| Measurement | Count |
| --- | --- |
| Classes named `*Factory` implemented in Application | 34 (plus `SocialTokenVerifierFactory`, a genuine factory) |
| Of those, pure construction — no injected collaborator, no `await` | 0 |
| Interfaces named `*Service` declared in Application, implemented in Infrastructure | 18 |
| Interfaces named `*Service` declared in Application, implemented in Application | 4 |
| Classes under `<M>.Infrastructure/Services/` that coordinate instead of naming a technology | 5 |
| Classes under `<M>.Infrastructure/Services/` that inject nothing | 12 |
| Handlers | 297, of which 61 over budget (49 Content, 12 Identity) |

Over-budget distribution: 1 handler at 10, 2 at 9, 1 at 8, 5 at 7, 18 at 6, 34 at 5.

> Order inside the stage: the naming and folder rules land first (21.1–21.5), then the Identity
> extractions, then the Content extractions. Everything shipped in one branch: the Content pass of
> Stage 15 the earlier draft waited on had already landed.

---

## Part 0 — Vocabulary

### 0.1 The two kinds of service

Told apart by **which layer implements the interface**, never by the name.

| | Interface declared in | Implemented in | Coordinates | Example |
| --- | --- | --- | --- | --- |
| **Application service** | Application | **Application** | repositories, other services, entities | `PublicLoginAuthService` |
| **Infrastructure service** | Application | **Infrastructure** | one technology | `CloudinaryService`, `JwtService` |

Both end in `Service`. The suffix does not encode the layer, because the folder does (0.3) and a
second suffix only invites a third.

### 0.2 When a class is still a factory

`Factory` survives only where the class **constructs**: it injects no collaborator and awaits
nothing, or it selects an implementation at runtime. `SocialTokenVerifierFactory` (picks the
Google or Apple verifier) and the entity `Create` methods qualify. None of the other 34 do —
they load, validate, sequence and commit, which is what an application service is.

The test to apply per class:

1. Does it name a vendor, a driver, a protocol, the clock or the file system? **Infrastructure service.**
2. Does it only sequence repositories, entities and other services? **Application service.**
3. Does it inject nothing and await nothing? It is a **factory** or a pure function, and never lives under `Services/`; see 21.4.

### 0.3 Folder structure

The confusion this stage removes is "where is the implementation of this interface". One rule
answers it on sight: **`Ports/` means the implementation is in Infrastructure. Everywhere else in
Application, the implementation is in Application.**

```
src/modules/<M>/<M>/src/
├── <M>.Application/
│   └── <Area>/                         Auth, Session, User, Catalog, Commerce, Editorial, …
│       ├── Ports/                      interfaces implemented in <M>.Infrastructure
│       ├── Repositories/               repository interfaces (a port by another name, kept)
│       ├── Services/                   application services: interface + implementation together
│       └── UseCases/<Admin|Public>/<Commands|Queries>/<UseCase>/
│           ├── Contracts/              the slice's own application-service interface
│           └── <Admin|Public><UseCase>Service.cs
└── <M>.Infrastructure/
    ├── Services/                       one implementation per Ports/ interface, each naming a technology
    └── Repositories/
```

Cross-module interfaces keep the existing home: `<M>.Contracts/Application/Services/`, published
to other modules and implemented inside the owning module.

| You are looking at | Its implementation is |
| --- | --- |
| `<Area>/Ports/IFoo.cs` | `<M>.Infrastructure/Services/Foo.cs` |
| `<Area>/Services/IFoo.cs` | `<Area>/Services/Foo.cs`, next to it |
| `UseCases/.../Contracts/IAdminFooService.cs` | `UseCases/.../AdminFooService.cs`, one folder up |
| `<M>.Contracts/Application/Services/IFoo.cs` | inside `<M>`, Application or Infrastructure |

Placement of a new application service follows the same split the tree already uses:

- Used by one slice: in the use-case folder, `Admin`/`Public`-prefixed, interface in the slice's
  `Contracts/` subfolder.
- Used by sibling slices: in `<Area>/Services/`, unprefixed, interface beside it.
- Registered in the module class next to the existing registrations.

---

## Part 1 — The dependency budget

### 1.1 Statement

A class implementing `ICommandHandler<,>`, `ICommandHandler<>`, or `IQueryHandler<,>` declares
at most **4 primary-constructor parameters**. Every parameter counts — repositories, services,
`IUnitOfWork`, `IMapper`, i18n facades, `TimeProvider`. No exclusion list: an exclusion list
turns a mechanical check into a judgement call, and the check must stay mechanical to sit in a
test.

Application services are **not** capped by this rule, but one that accumulates more than ~6
dependencies is a smell reviewed case by case: it usually means two phases were merged into one
component.

### 1.2 Why 4

A handler is the use case's orchestrator: it retrieves, delegates, applies domain transitions,
and commits. Four seams cover that shape — typically one retrieval dependency, one extracted
step component, one commit seam, and one cross-cutting service. A fifth dependency is the
signal that the handler owns a phase it should delegate: association validation, response
assembly, a security fan-out, or a protocol shared with a sibling slice.

### 1.3 What an extraction must never do

The budget is met by moving a **cohesive phase** into an application service, never by hiding
wiring. Each of these turns the fix into a worse problem than the count:

| Forbidden move | Why |
| --- | --- |
| Pass-through wrapper (`RoleDtoService(mapper)` with one delegating method) | Adds a seam with no responsibility; the dependency count drops on paper while coupling is unchanged. |
| God service (one that executes the whole use case) | The handler becomes a pass-through and the service becomes an unnamed handler; SRP is violated one level down. |
| Cross-slice dependency (an Admin handler injecting a `Public*` service or vice versa) | Breaks the module's Admin/Public prefix isolation. Shared logic lives in an unprefixed service in `<Area>/Services/`. |
| Bundling unrelated dependencies to lower the count (`IAuthServices` exposing repository + password service + i18n) | A facade over unrelated seams is constructor over-injection with extra steps; nothing gains a responsibility. |
| Moving domain rules into the service (guards, state transitions) | R12: aggregates decide; application services load, validate associations, and assemble. A service may *call* a guarded transition, never re-implement one. |
| A service hiding a repository the handler also injects for the same query | Double-loading and split ownership of one read. Each read has exactly one owner. |

### 1.4 What an application service legitimately owns

The four species this stage uses, all with precedent in the tree:

| Species | Owns | Precedent |
| --- | --- | --- |
| **Protocol service** (shared, unprefixed) | A multi-step invariant-bearing sequence used by sibling slices | `OtpVerificationService`, `SessionService`, `RefreshTokenRotationService` |
| **Resolution service** (per-slice, prefixed) | Loading and validating the use case's associations, throwing localized errors | `PublicLoginAuthService`, `ICreateOrderService` |
| **Security fan-out service** (shared, unprefixed) | The revoke/rotate/bump reaction that must follow a credential change | new in this stage (`CredentialInvalidationService`) |
| **Response service** (area-shared, unprefixed) | Assembling a response DTO from an aggregate plus its enrichment services (`IMapper`, `IFileRepository`, `IUserLookupService`) | new in this stage (`LyricsResponseService`, `CategoryResponseService`) |

---

## Decisions

| # | Question | Decision |
| --- | --- | --- |
| D1 | Suffix for application services — keep `Factory`, use `Service`, or role suffixes (`Assembler`, `Issuer`, `Authenticator`) | **`Service`.** Role suffixes read well individually but add four words for a distinction the folder already makes. `Factory` is worse: it means "constructs" everywhere outside this repository, and 34 of our 35 do not. |
| D2 | How the two kinds of service stay apart — suffix, folder, or namespace | **Folder.** `<Area>/Ports/` is implemented in Infrastructure; everything else in Application is implemented in Application. One rule, mechanically testable, no second suffix. |
| D3 | Where an application service's interface lives | **Beside its implementation** — the same folder for an area service, the slice's `Contracts/` subfolder for a slice-local one, which the tree already does 21 times. |
| D4 | The dependency-free classes under `Infrastructure/Services/` | **All stay** (21.4). Re-measured: the ones that inject nothing still name a technology in their body — `System.Security.Cryptography`, `CsvHelper`, `ClosedXML`, `Polly`. Emptiness of a constructor is not evidence of a domain rule. |
| D5 | The budget counts which parameters | **Every** constructor parameter, cross-cutting seams included. Mechanical rules stay mechanical; the pressure this creates is what pushes response assembly and security fan-outs out of handlers. |
| D6 | Application services under the budget too | **No**, soft-capped at ~6 and reviewed case by case. |
| D7 | Owner of the enrichment trio (`IMapper`, `IFileRepository`, `IUserLookupService`) | **Response services** (21.13); the DTO extension methods become their implementation detail. |
| D8 | Order of the two halves | **Naming and folders first** (21.1–21.5), then the extractions. Extracting under the old vocabulary means renaming the same files a week later. |
| D9 | When the Content half lands | **In the same branch.** The Stage 15 Content pass it was to wait on had already shipped, so the burn-down list went from 61 to 0 in one stage. |
| D10 | Transaction boundaries in an extraction | **Never move.** The handler that committed before commits after. A service commits only where the current code already commits mid-flow (the OTP metered-attempt write). |

---

## Checklist

- [x] 21.1 — Rename the 34 `*Factory` application services to `*Service`: files, interfaces, output records, DI registrations, test files
- [x] 21.2 — Split `<Area>/Services/`: 18 port interfaces to `<Area>/Ports/`, `<Area>/Factories/` folders become `<Area>/Services/`
- [x] 21.3 — Move the 5 workflow coordinators out of `Infrastructure/Services/` into Application
- [x] 21.4 — Re-measure the dependency-free Infrastructure classes and record why each stays
- [x] 21.5 — Rewrite `docs/factory-pattern.md` as `docs/application-services.md`
- [x] 21.6 — Architecture test: the handler dependency budget, with the burn-down list
- [x] 21.7 — Architecture tests: no `I*Factory` implemented in Application, no `Ports/` interface implemented in Application
- [x] 21.8 — Identity: the OTP verification protocol service
- [x] 21.9 — Identity: the credential invalidation service
- [x] 21.10 — Identity: role and permission grant services
- [x] 21.11 — Identity: lifecycle services for soft deletion
- [x] 21.12 — Identity: social login and avatar handlers
- [x] 21.13 — Content: response services
- [x] 21.14 — Content: the lyrics page read
- [x] 21.15 — Content: lyrics create/update association services
- [x] 21.16 — Content: every remaining violator in the assignment table
- [x] 21.17 — Test moves
- [x] 21.18 — Verify

---

## Part 2 — Naming and placement

### 21.1 The 34 renames

Both halves move together: `IPublicLoginAuthFactory` to `IPublicLoginAuthService`,
`PublicLoginAuthFactory` to `PublicLoginAuthService`. Output records keep their own names
(`PublicLoginAuthData`, `RoleGrantData`) — they already read correctly.

Placement does not change in this item: the 21 slice-local ones stay in their use-case folder
with their `Contracts/`, the 13 area-level ones stay put and their folder is renamed in 21.2.

| Area | Renamed |
| --- | --- |
| `Identity.Application/Auth` | `OtpVerificationFactory` and the 11 slice-local `*AuthFactory` / `*OtpFactory` / `*SessionFactory` classes |
| `Identity.Application/Session` | `SessionFactory` → `SessionService`, `RefreshTokenFactory` → `RefreshTokenRotationService` |
| `Identity.Application/User` | the 4 `*AuthFactory` classes of the profile and avatar slices |
| `Content.Application/Catalog` | `CategoryDtoFactory`, `PackageDtoFactory` |
| `Content.Application/Commerce` | `ContentOrderDtoFactory`, `PaymentDtoFactory`, `OrderPaymentFactory`, and the 5 slice-local order services |
| `Content.Application/Editorial` | `AlbumDtoFactory`, `ArtistDtoFactory`, `VideoDtoFactory` |
| `Content.Application/Interactions` | `PlaylistDtoFactory` |
| `Content.Application/Shared` | `ContentLookupFactory` |

Two names need more than a suffix swap:

- `RefreshTokenFactory` → **`RefreshTokenRotationService`**. A plain rename collides with the
  existing `IRefreshTokenService` port, which generates and hashes the token value. The
  application service validates and rotates; the port produces bytes. Different jobs, different
  names.
- `SocialTokenVerifierFactory` **keeps its name**. It picks the Google or Apple verifier at
  runtime and injects nothing else.
- `StreamingLinkFactory` **keeps its name and its folder**. It is static, injects nothing and
  awaits nothing: a pure constructor of links, so `Editorial/Factories/` stays with that one file.

### 21.2 `Ports/` and `Services/`

Today `<Area>/Services/` holds both kinds of interface, which is the confusion this stage exists
to remove: 18 of its 22 interfaces are implemented in Infrastructure, 4 in Application, and
nothing in the folder name says which.

| Module | Moves to `<Area>/Ports/` |
| --- | --- |
| Identity | `IJwtService`, `IOtpService`, `IPasswordService`, `IRefreshTokenService`, `ITokenDeliveryService` (Auth); `IExportStrategy`, `ISessionExportService`, `ISessionMetadataService` (Session); `IAvatarService` (User) |
| Storage | `ICloudStorageClient`, `ICloudinaryService`, `IFileService`, `IImageColorService`, `IUrlSafetyGuard` (Shared) |
| Content | `IStreamingLinkResolutionService`, `ITranslationService` (Shared); `IYoutubeThumbnailService` (Editorial) |
| Mailer | `IEmailSenderService` (Shared) |

Staying in `<Area>/Services/`, because their implementation is in Application: `IFileUploadService`
(after 21.3) and `ICommerceCustomerNotifier`, plus the two interface-less classes
`ContentPublicLinks` and `NewsletterLinkBuilder`.

`IEmailTemplateRenderer` and `INotificationRenderer` leave `Shared/Services` altogether: a renderer
injects only its resx catalog and awaits nothing, so it is a template, not a service. Both
interfaces, both renderers and both `Messages/` catalogs live under `Mailer.Application/Shared/Templates/`
(`Emails/`, `Notifications/`), and `Shared/Services` disappears.

Then the `<Area>/Factories/` folders — `Identity.Application/{Auth,Session}`,
`Content.Application/{Catalog,Commerce,Editorial,Interactions}` — are renamed to
`<Area>/Services/`, merging into the folder the ports just left. Their `Factories/Contracts/`
subfolders disappear: the interface sits beside the implementation.

### 21.3 The 5 that are in the wrong layer

Each coordinates repositories and other services and names no technology, which makes it an
application service living one layer too low:

| Class | Today | Moves to |
| --- | --- | --- |
| `NotificationService` | `Mailer.Infrastructure/Services` | `Mailer.Application/Notifications/Services` |
| `OutboxEmailService` | `Mailer.Infrastructure/Services` | `Mailer.Application/Notifications/Services` |
| `EmailDispatcher` | `Mailer.Infrastructure/Services` | `Mailer.Application/Newsletter/Services` |
| `FileUploadService` | `Storage.Infrastructure/Services` | `Storage.Application/Shared/Services` |
| `FileStorageService` | `Storage.Infrastructure/Services` | `Storage.Application/Shared/Services` |

`FileStorageService` is the one to check before moving: it implements `IFileStorageService`, which
`Storage.Contracts` publishes to other modules, so the move must keep that contract intact.

Mailer is the module worth doing first. Its entire outbound workflow lives one layer below the one
that should own it, which is why `Mailer.Application` currently holds almost no logic.

### 21.4 The classes that inject nothing

Twelve classes under `Infrastructure/Services/` have no constructor dependencies, which reads at
first like misplaced domain logic — the reason the tree has no `Domain/Services/` folder and does
not need one. Re-measured against the test in 0.2, every one of them names a
technology in its body and stays:

| Class | Technology it names |
| --- | --- |
| `OtpService`, `RefreshTokenService`, `PasswordService`, `JwtService` | `System.Security.Cryptography` |
| `CsvExportStrategy`, `XlsxExportStrategy`, `SessionExportBase` | `CsvHelper`, `ClosedXML` |
| `CloudStorageResilience` | `Polly` |
| `ImageColorService`, `SmtpEmailSenderService` | image decoding, `MailKit` |
| `PlaceholderTranslationService` | the null implementation of a translation provider |
| `SessionExportService` | selects between the two export strategies at runtime |

An empty constructor is not evidence of a domain rule. The rule that decides is "does it name a
technology", and these do.

`SessionExportService` is the only one worth a second look: it `new`s its two strategies instead
of taking them injected. That is a runtime selector — genuinely a factory — but the things it
selects are `CsvHelper` and `ClosedXML`, so it stays in Infrastructure beside them.

### 21.5 The documentation

`docs/factory-pattern.md` teaches "factory" as this codebase's word for an application service,
which is the source of the vocabulary this stage removes. It is rewritten as
`docs/application-services.md`: the two kinds of service, the folder structure of 0.3, the four
species of 1.4, and the dependency budget. The module `README.md` reference-direction blocks get
the `Ports/` row.

---

## Part 3 — Enforcement

Both tests land **first**, before any rename, so the burn-down is visible from the first commit.

### 21.6 The handler dependency budget

The test fails when a handler over budget is *not* on the burn-down list (no new debt) and when a
listed handler drops under budget without being removed (the list only shrinks). When the list is
empty it is deleted, together with the second test.

Shipped as `tests/Architecture.Tests/HandlerDependencyBudgetTests.cs`, over every module's three
layer assemblies. The 61 names live in `tests/Architecture.Tests/HandlerBudgetBurnDown.txt`, copied
to the output directory the way `KnownViolations.txt` already is. A text file rather than a
`HashSet` literal keeps every extraction PR's diff to the one line it earned.

### 21.7 The taxonomy rules

`tests/Architecture.Tests/ServiceTaxonomyTests.cs` holds three rules, all mechanical:

| Rule | Fails when |
| --- | --- |
| `NoFactoryInterface_IsImplementedInApplication` | an interface named `I*Factory` has an implementation in an Application assembly, `SocialTokenVerifierFactory` excepted |
| `Ports_AreImplementedOnlyInInfrastructure` | a type in a `*.Ports` namespace has an implementation inside the same module's Application assembly, or none in its Infrastructure assembly |
| `InfrastructureServices_ImplementOnlyPorts` | a class in `<M>.Infrastructure.Services` implements an interface declared in `<M>.Application` outside a `*.Ports` namespace |

The third is what keeps 21.3 from silently reverting: a coordinator that drifts back into
`Infrastructure/Services/` fails it, because the interface it implements will not be in `Ports/`.
`SocialTokenVerifierFactory` goes in `KnownViolations.txt` with its one-line reason rather than
being special-cased in the rule.

---

## Part 4 — Identity (14 handlers)

### 21.8 The OTP verification protocol service (7 → 3, 6 → 4)

`OtpVerificationService` currently owns only the judge-and-meter block; the handlers still
inject `IOtpRepository`, `IAccountLockoutRepository`, and `TimeProvider` around it. The service
grows to own the **whole consumption protocol** — load, judge, consume, invalidate, clear —
which is one cohesive sequence with one reason to change. The handler keeps what is genuinely
use-case-specific: the user gates, the aggregate transition, and the commit.

The failure path still commits the metered attempt before throwing; the success path stays a
single commit, issued by the handler, covering `MarkAsUsed` and `MarkVerifiedByOtp` together —
transaction boundaries do not move.

`Application/Auth/Services/IOtpVerificationService.cs`:

```csharp
using _116.Identity.Domain.ValueObjects;

namespace _116.Identity.Application.Auth.Services;

/// <summary>
/// Owns the OTP consumption protocol shared by the VerifyOtp handlers: load the outstanding
/// code, judge the presented one, consume it, and retire its siblings and counters.
/// </summary>
public interface IOtpVerificationService
{
    /// <summary>
    /// Consumes the outstanding OTP for the account and purpose. Returns only when the
    /// presented code is valid; otherwise meters the missed attempt, commits it, and throws
    /// the matching localized error. The caller owns the success-path commit.
    /// </summary>
    /// <param name="userId">The account presenting the code.</param>
    /// <param name="purpose">The purpose the code was issued for.</param>
    /// <param name="code">The presented code.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task ConsumeOtpAsync(Guid userId, OtpPurpose purpose, string code, CancellationToken cancellationToken);
}
```

`Application/Auth/Services/OtpVerificationService.cs`:

```csharp
using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Auth.Repositories;
using _116.Identity.Application.Auth.Ports;
using _116.Identity.Application.Shared.Errors.Facade;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.ValueObjects;

namespace _116.Identity.Application.Auth.Services;

/// <summary>
/// Consumes the outstanding OTP for an account: judges the presented code, meters and commits
/// a missed attempt before throwing, and on success marks the code used, invalidates its
/// siblings and clears the account failure counter.
/// </summary>
/// <param name="otpRepository">Repository for OTP data access operations.</param>
/// <param name="otpService">Service comparing the presented code against the stored keyed hash.</param>
/// <param name="lockoutRepository">Repository metering and clearing account failures.</param>
/// <param name="unitOfWork">Unit of Work committing the consumed attempt.</param>
/// <param name="timeProvider">Clock supplying the instant the code is verified against.</param>
/// <param name="i18n">Single i18n entry point for the Identity module.</param>
public class OtpVerificationService(
    IOtpRepository otpRepository,
    IOtpService otpService,
    IAccountLockoutRepository lockoutRepository,
    IIdentityUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    IdentityI18n i18n
) : IOtpVerificationService
{
    /// <inheritdoc />
    public async Task ConsumeOtpAsync(
        Guid userId,
        OtpPurpose purpose,
        string code,
        CancellationToken cancellationToken
    )
    {
        OtpEntity otp = await otpRepository.GetLatestOutstandingOtpOrThrowAsync(
            userId: userId,
            purpose: purpose,
            cancellationToken: cancellationToken
        );

        int attemptsBefore = otp.AttemptCount;
        EnumOtpVerificationStatus verificationStatus = otp.Verify(
            suppliedCodeMatches: otpService.Verify(code: code, hash: otp.CodeHash),
            now: timeProvider.GetUtcNow().UtcDateTime
        );

        if (verificationStatus != EnumOtpVerificationStatus.Valid)
        {
            // A missed code consumed an attempt on the OTP row and is metered against the
            // account; committing before the throw keeps both durable.
            if (otp.AttemptCount != attemptsBefore)
            {
                await lockoutRepository.RegisterFailedOtpAsync(userId: userId, cancellationToken: cancellationToken);
                await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
            }

            throw verificationStatus switch
            {
                EnumOtpVerificationStatus.Expired => i18n.User.OtpExpired(),
                EnumOtpVerificationStatus.AttemptsExhausted => i18n.User.MaxOtpAttemptsReached(),
                _ => i18n.User.InvalidOtpCode(),
            };
        }

        otp.MarkAsUsed(now: timeProvider.GetUtcNow().UtcDateTime);
        await otpRepository.InvalidateExistingOtpsAsync(
            userId: userId,
            purpose: purpose,
            exceptOtpId: otp.Id,
            cancellationToken: cancellationToken
        );
        await lockoutRepository.ClearFailedOtpAsync(userId: userId, cancellationToken: cancellationToken);
    }
}
```

`AdminVerifyOtpHandler` after (3 dependencies):

```csharp
using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Auth.Repositories;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.ValueObjects;
using _116.BuildingBlocks.Application.CQRS;

namespace _116.Identity.Application.Auth.UseCases.Admin.Commands.VerifyOtp;

/// <summary>
/// Handles the <see cref="AdminVerifyOtpCommand" /> to verify OTP codes for admin account verification.
/// </summary>
/// <param name="authRepository">Repository for user data access operations.</param>
/// <param name="otpVerificationService">Service consuming the outstanding OTP.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminVerifyOtpHandler(
    IAuthRepository authRepository,
    IOtpVerificationService otpVerificationService,
    IIdentityUnitOfWork unitOfWork
) : ICommandHandler<AdminVerifyOtpCommand, AdminVerifyOtpResult>
{
    /// <inheritdoc />
    public async Task<AdminVerifyOtpResult> Handle(AdminVerifyOtpCommand command, CancellationToken cancellationToken)
    {
        var email = new Email(value: command.Email);
        var purpose = new OtpPurpose(value: command.Purpose);
        UserEntity? user = await authRepository.GetUserWithRolesByEmailOrThrow(
            email: email,
            cancellationToken: cancellationToken
        );

        authRepository.IsUserAdmin(user!);
        authRepository.IsUserAccountActive(user!);

        await otpVerificationService.ConsumeOtpAsync(
            userId: user!.Id,
            purpose: purpose,
            code: command.Code,
            cancellationToken: cancellationToken
        );

        user.MarkVerifiedByOtp(purpose: purpose);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
        return new AdminVerifyOtpResult(IsSuccess: true);
    }
}
```

`PublicVerifyOtpHandler` after: identical shape plus `IdentityI18n` for the already-verified
409 pre-check — **4 dependencies**.

Test moves: the consumption-protocol matrix currently in the two handler unit test files moves
to a new `OtpVerificationServiceTests` (mocked seams, including the commit-before-throw
inversion test); the handler tests shrink to orchestration (gates → service call → transition
→ commit). Integration tests are untouched — the wire behavior is identical.

### 21.9 The credential invalidation service (6/6/5 → 4/4/3)

`AdminChangePasswordHandler`, `PublicChangePasswordHandler`, and `PublicSetPasswordHandler`
each carry the same three-dependency tail: revoke the account's other sessions, commit, rotate
the security stamp. That is one security reaction — "this credential changed, no other session
may outlive it" — duplicated three times.

`Application/Auth/Services/ICredentialInvalidationService.cs`:

```csharp
namespace _116.Identity.Application.Auth.Services;

/// <summary>
/// Owns the security reaction to a credential change: the account's other sessions are revoked
/// and its security stamp rotated in the same flow as the new credential.
/// </summary>
public interface ICredentialInvalidationService
{
    /// <summary>
    /// Commits the pending credential change together with the revocation of every other
    /// session of the account, then rotates the security stamp.
    /// </summary>
    /// <param name="userId">The account whose credential changed.</param>
    /// <param name="exemptSessionId">The acting session that survives, or null to revoke all.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task CommitCredentialChangeAsync(Guid userId, Guid? exemptSessionId, CancellationToken cancellationToken);
}
```

`Application/Auth/Services/CredentialInvalidationService.cs`:

```csharp
using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Session.Repositories;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Enums;

namespace _116.Identity.Application.Auth.Services;

/// <summary>
/// Commits a credential change together with the revocation of the account's other sessions,
/// then rotates the security stamp so outstanding tokens die at validation.
/// </summary>
/// <param name="sessionRepository">Repository revoking the account's sessions.</param>
/// <param name="tokenStateRepository">Repository rotating the account's security stamp.</param>
/// <param name="unitOfWork">Unit of Work committing the change and the revocations together.</param>
public class CredentialInvalidationService(
    ISessionRepository sessionRepository,
    IUserTokenStateRepository tokenStateRepository,
    IIdentityUnitOfWork unitOfWork
) : ICredentialInvalidationService
{
    /// <inheritdoc />
    public async Task CommitCredentialChangeAsync(
        Guid userId,
        Guid? exemptSessionId,
        CancellationToken cancellationToken
    )
    {
        // The new hash and the revocations commit together, so the old credential can never
        // outlive the change.
        await sessionRepository.DeleteAllByUserIdAsync(
            userId: userId,
            reason: EnumSessionRevokeReason.SecurityInvalidation,
            exemptSessionId: exemptSessionId,
            cancellationToken: cancellationToken
        );

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
        await tokenStateRepository.RotateSecurityStampAsync(userId: userId, cancellationToken: cancellationToken);
    }
}
```

`AdminChangePasswordHandler` after (4 dependencies) — the password checks stay in the handler,
because they are this use case's policy, not part of the shared reaction:

```csharp
public class AdminChangePasswordHandler(
    IAuthRepository authRepository,
    IPasswordService passwordService,
    ICredentialInvalidationService credentialInvalidationService,
    IdentityI18n i18n
) : ICommandHandler<AdminChangePasswordCommand, AdminChangePasswordResult>
{
    /// <inheritdoc />
    public async Task<AdminChangePasswordResult> Handle(
        AdminChangePasswordCommand command,
        CancellationToken cancellationToken
    )
    {
        UserEntity? user = await authRepository.FindUserByIdOrThrow(
            userId: command.UserId,
            cancellationToken: cancellationToken
        );

        authRepository.IsUserAccountActive(user!);
        await authRepository.IsSessionValidAsync(command.SessionId, cancellationToken);

        if (!passwordService.Verify(password: command.OldPassword, hash: user!.PasswordHash))
        {
            throw i18n.User.IncorrectCurrentPassword();
        }

        if (passwordService.Verify(password: command.NewPassword, hash: user.PasswordHash))
        {
            throw i18n.User.NewPasswordSameAsOld();
        }

        user.UpdatePassword(
            newPasswordHash: passwordService.Hash(password: command.NewPassword),
            origin: EnumPasswordChangeOrigin.Changed
        );

        await credentialInvalidationService.CommitCredentialChangeAsync(
            userId: user.Id,
            exemptSessionId: command.SessionId,
            cancellationToken: cancellationToken
        );

        return new AdminChangePasswordResult(IsSuccess: true);
    }
}
```

`PublicChangePasswordHandler` mirrors it (4). `PublicSetPasswordHandler` drops to
`IAuthRepository`, `IPasswordService`, `ICredentialInvalidationService` (3).

### 21.10 Role and permission grant services (6/6/6/5 → 4)

The four assignment handlers share one phase: resolve the role/permission, gate it
(deleted-first, then inactive), apply the aggregate transition, and throw the localized error
on a no-op. Each slice gets its own prefixed resolution service — the gates differ per use
case, so this is the per-slice species, exactly like the existing `*AuthService`s.

Exemplar — `Application/User/UseCases/Admin/Commands/AssignRoleToUser/Contracts/IAdminAssignRoleToUserService.cs`:

```csharp
using _116.Identity.Domain.Entities;

namespace _116.Identity.Application.User.UseCases.Admin.Commands.AssignRoleToUser.Contracts;

/// <summary>
/// Resolves and applies a role grant: loads the role and the user, gates deleted and inactive
/// roles, and grants through the user aggregate.
/// </summary>
public interface IAdminAssignRoleToUserService
{
    /// <summary>
    /// Grants the role to the user, throwing the localized error when the role is deleted,
    /// inactive, or already assigned. The caller owns the commit.
    /// </summary>
    /// <param name="userId">The user receiving the role.</param>
    /// <param name="roleId">The role being granted.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The user with the grant applied and the granted role.</returns>
    Task<RoleGrantData> GrantAsync(Guid userId, Guid roleId, CancellationToken cancellationToken);
}

/// <summary>
/// The user carrying the fresh grant and the role that was granted.
/// </summary>
/// <param name="User">The user aggregate with the grant applied.</param>
/// <param name="Role">The granted role.</param>
public record RoleGrantData(UserEntity User, RoleEntity Role);
```

`AdminAssignRoleToUserService.cs` (same folder):

```csharp
using _116.Identity.Application.Shared.Errors.Facade;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Application.User.UseCases.Admin.Commands.AssignRoleToUser.Contracts;
using _116.Identity.Domain.Entities;

namespace _116.Identity.Application.User.UseCases.Admin.Commands.AssignRoleToUser;

/// <summary>
/// Resolves and applies a role grant for the admin assign-role use case.
/// </summary>
/// <param name="roleRepository">Repository for role data access operations.</param>
/// <param name="authRepository">Repository loading the user aggregate with its roles.</param>
/// <param name="i18n">Single i18n entry point for the Identity module.</param>
public class AdminAssignRoleToUserService(
    IRoleRepository roleRepository,
    IAuthRepository authRepository,
    IdentityI18n i18n
) : IAdminAssignRoleToUserService
{
    /// <inheritdoc />
    public async Task<RoleGrantData> GrantAsync(Guid userId, Guid roleId, CancellationToken cancellationToken)
    {
        RoleEntity? role = await roleRepository.GetRoleByIdOrThrowAsync(
            roleId: roleId,
            cancellationToken: cancellationToken
        );

        // Soft deletion also clears IsActive, so the deleted check comes first to stay reachable.
        if (role!.IsDeleted)
        {
            throw i18n.User.RoleIsDeleted();
        }

        if (!role.IsActive)
        {
            throw i18n.User.RoleIsInactive();
        }

        UserEntity? user = await authRepository.GetUserWithRolesByIdOrThrow(
            userId: userId,
            cancellationToken: cancellationToken
        );

        if (!user!.GrantRole(roleId: roleId, roleName: role.Name))
        {
            throw i18n.User.RoleAlreadyAssignedToUser();
        }

        return new RoleGrantData(User: user, Role: role);
    }
}
```

`AdminAssignRoleToUserHandler` after (4 dependencies):

```csharp
public class AdminAssignRoleToUserHandler(
    IAdminAssignRoleToUserService assignRoleService,
    IUserTokenStateRepository tokenStateRepository,
    IIdentityUnitOfWork unitOfWork,
    IMapper mapper
) : ICommandHandler<AdminAssignRoleToUserCommand, AdminAssignRoleToUserResult>
{
    /// <inheritdoc />
    public async Task<AdminAssignRoleToUserResult> Handle(
        AdminAssignRoleToUserCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid userId = Guid.Parse(input: command.UserId);

        RoleGrantData grant = await assignRoleService.GrantAsync(
            userId: userId,
            roleId: command.RoleId,
            cancellationToken: cancellationToken
        );

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
        await tokenStateRepository.BumpTokenVersionAsync(userId: userId, cancellationToken: cancellationToken);

        // The freshly granted association has no Role navigation loaded yet; the role in hand fills it.
        IReadOnlyCollection<RoleDto> roles = grant
            .User.UserRoles.Select(ur => (ur.RoleId == grant.Role.Id ? grant.Role : ur.Role).ToRoleDto(mapper))
            .ToList();
        return new AdminAssignRoleToUserResult(Roles: roles);
    }
}
```

Same treatment, same shapes:

| Handler | Service | Handler deps after |
| --- | --- | --- |
| `AdminRemoveRoleFromUserHandler` | `AdminRemoveRoleFromUserService(authRepository, roleRepository, i18n)` | service, tokenState, uow, mapper — **4** |
| `AdminAssignPermissionToRoleHandler` | `AdminAssignPermissionToRoleService(roleRepository, permissionRepository, i18n)` | service, tokenState, uow, mapper — **4** |
| `AdminRemovePermissionFromRoleHandler` | `AdminRemovePermissionFromRoleService(roleRepository, i18n)` | service, tokenState, uow, mapper — **4** |

### 21.11 Lifecycle services for soft deletion (5 → 3)

`AdminSoftDeleteRoleHandler` and `AdminSoftDeletePermissionHandler` each hold
repository + uow + mapper + i18n + `TimeProvider`. The load-guard-stamp phase moves to a
per-slice service; the handler keeps commit and response.

```csharp
/// <summary>
/// Loads the role and applies the clocked soft deletion, throwing when already deleted.
/// </summary>
public class AdminSoftDeleteRoleService(
    IRoleRepository roleRepository,
    IdentityI18n i18n,
    TimeProvider timeProvider
) : IAdminSoftDeleteRoleService
{
    /// <inheritdoc />
    public async Task<RoleEntity> SoftDeleteAsync(Guid roleId, CancellationToken cancellationToken)
    {
        RoleEntity? role = await roleRepository.GetRoleByIdOrThrowAsync(
            roleId: roleId,
            cancellationToken: cancellationToken
        );

        if (!role!.SoftDelete(now: timeProvider.GetUtcNow().UtcDateTime))
        {
            throw i18n.Role.RoleAlreadyDeleted();
        }

        return role;
    }
}
```

Handler after: service, uow, mapper — **3**. The permission twin is identical with
`IPermissionRepository` and the permission i18n family. (Adjust the exact i18n error names to
the ones the current handlers throw; the shape is what this spec fixes.)

### 21.12 Social login (6 → 4)

`PublicSocialLoginHandler` sequenced the token verification itself: `ISocialTokenVerifierFactory`,
the email-verified check and its `IdentityI18n` sat beside the auth service. Provider-token
verification is the first step of social authentication, not a phase the handler owns, so
`IPublicSocialLoginAuthService.AuthenticateAsync(provider, idToken)` now verifies internally and
refuses an unverified provider email. Handler after: authService, sessionService, avatarService,
mapper — **4**, the same shape as `PublicLoginHandler`.

The avatar handlers the first measurement listed were already under budget on the current tree;
the `AvatarStorageService` the earlier draft planned is not needed.

---

## Part 5 — Content (49 handlers)

Two extractions recur across nearly all 49; the remaining cases are per-slice resolution
services. The names below are the ones that shipped.

### 21.13 Response services — the enrichment trio (fixes ~20 handlers)

`IMapper` + `IFileRepository` (+ `IUserLookupService`) appear together wherever a handler
builds its response DTO: the mapping, the author profile, and the avatar/poster URL resolution
are one phase — response assembly. Today the trio is threaded through extension methods
(`ToLyricsDetailDtoAsync(mapper, userLookup, fileRepository, …)`), which pushes three
dependencies into every calling handler. Each content area gets one injectable response
service wrapping its existing extensions:

`Application/Editorial/Services/LyricsResponseService.cs`:

```csharp
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Identity.Contracts.Application;
using MapsterMapper;

namespace _116.Content.Application.Editorial.Services;

/// <summary>
/// Assembles lyrics response DTOs, resolving the author profile and file URLs the wire shape
/// carries.
/// </summary>
/// <param name="mapper">The Mapster mapper used for tags.</param>
/// <param name="userLookup">Service for resolving author profiles from the Identity module.</param>
/// <param name="fileRepository">Repository for resolving avatar and cover image file URLs.</param>
public class LyricsResponseService(
    IMapper mapper,
    IUserLookupService userLookup,
    IFileRepository fileRepository
) : ILyricsResponseService
{
    /// <inheritdoc />
    public Task<LyricsDetailDto> ToDetailDtoAsync(LyricsEntity lyrics, CancellationToken cancellationToken)
    {
        return lyrics.ToLyricsDetailDtoAsync(mapper, userLookup, fileRepository, cancellationToken);
    }

    /// <inheritdoc />
    public Task<PublicLyricsDetailDto> ToPublicDetailDtoAsync(
        LyricsEntity lyrics,
        bool isLiked,
        CancellationToken cancellationToken
    )
    {
        return lyrics.ToPublicLyricsDetailDtoAsync(mapper, userLookup, fileRepository, cancellationToken, isLiked);
    }
}
```

This is not a pass-through wrapper: it is the single owner of "entity → wire shape" for the
area, it collapses three seams into one at every call site, and the existing extension methods
become its private implementation detail once all callers migrate. Shipped as the `*DtoService`
family the tree already used for videos and categories: new `LyricsDtoService`, `ArticleDtoService`,
`ShortVideoDtoService` (Editorial), `ArticleCommentDtoService` (Interactions),
`CategoryPricingDtoService` (Catalog), plus `PaymentDtoService.CreateWithProofAsync` and
`VideoDtoService.ResolvePublicContextAsync` so a feed can assemble several groups from one batch.

### 21.14 The lyrics page read (9 → 4)

`PublicGetLyricsBySlugHandler` stitches five repositories into one page. The link-and-sibling
assembly is one phase with one reason to change — what the lyrics page shows around the lyrics:

`Application/Editorial/UseCases/Public/Queries/GetLyricsBySlug/Contracts/IPublicLyricsPageService.cs`:

```csharp
namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetLyricsBySlug.Contracts;

/// <summary>
/// Assembles the navigation shell of a lyrics page: linked slugs, sibling album tracks, and
/// streaming links.
/// </summary>
public interface IPublicLyricsPageService
{
    /// <summary>
    /// Resolves the page links for the given lyrics.
    /// </summary>
    /// <param name="lyrics">The published lyrics the page is built around.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task<LyricsPageLinks> ResolveLinksAsync(LyricsEntity lyrics, CancellationToken cancellationToken);
}

/// <summary>
/// The navigation shell of a lyrics page.
/// </summary>
/// <param name="VideoSlug">The linked video's slug, when one exists.</param>
/// <param name="ArtistSlug">The linked artist's slug, when one exists.</param>
/// <param name="AlbumTracks">The album's other published tracks.</param>
/// <param name="StreamingLinks">The curated and generated streaming links.</param>
public record LyricsPageLinks(
    string? VideoSlug,
    string? ArtistSlug,
    IReadOnlyList<AlbumTrackDto> AlbumTracks,
    IReadOnlyList<StreamingLinkDto> StreamingLinks
);
```

The implementation (`PublicLyricsPageService`) takes `IVideoRepository`, `IArtistRepository`,
`IAlbumRepository`, `IStreamingLinkRepository`, `ILyricsRepository` and moves the existing
lines 54–112 of the handler verbatim — no behavior change, including the album/no-album
branch and the `StreamingLinkService` composition. Handler after (**4**):

```csharp
public class PublicGetLyricsBySlugHandler(
    ILyricsRepository lyricsRepository,
    IPublicLyricsPageService pageService,
    ILyricsResponseService responseService,
    ContentI18n i18n
) : IQueryHandler<PublicGetLyricsBySlugQuery, PublicGetLyricsBySlugResult>
{
    /// <inheritdoc />
    public async Task<PublicGetLyricsBySlugResult> Handle(
        PublicGetLyricsBySlugQuery query,
        CancellationToken cancellationToken
    )
    {
        LyricsEntity? lyrics = await lyricsRepository.GetBySlugAsync(
            slug: query.Slug,
            cancellationToken: cancellationToken
        );

        if (lyrics is null || lyrics.Status != EnumContentStatus.Published)
        {
            throw i18n.Lyrics.NotFound(id: Guid.Empty);
        }

        LyricsPageLinks links = await pageService.ResolveLinksAsync(
            lyrics: lyrics,
            cancellationToken: cancellationToken
        );

        bool isLiked =
            query.CurrentUserId is Guid currentUserId
            && await lyricsRepository.HasLikedAsync(
                userId: currentUserId,
                lyricsId: lyrics.Id,
                cancellationToken: cancellationToken
            );

        var dto = await responseService.ToPublicDetailDtoAsync(
            lyrics: lyrics,
            isLiked: isLiked,
            cancellationToken: cancellationToken
        );
        return new PublicGetLyricsBySlugResult(
            Lyrics: dto,
            VideoSlug: links.VideoSlug,
            ArtistSlug: links.ArtistSlug,
            AlbumTracks: links.AlbumTracks,
            StreamingLinks: links.StreamingLinks
        );
    }
}
```

### 21.15 Lyrics create/update association services (8 → 4)

`AdminCreateLyricsHandler`'s pre-persist phase — category existence, slug uniqueness, video
resolution and `MarkHasLyrics` — moves to a per-slice resolution service:

```csharp
/// <summary>
/// Validates a lyrics creation's associations: the category exists, the slug is free, and the
/// linked video (when present) is resolved and marked as having lyrics.
/// </summary>
public class AdminCreateLyricsService(
    ICategoryRepository categoryRepository,
    ILyricsRepository lyricsRepository,
    IVideoRepository videoRepository,
    ContentI18n i18n
) : IAdminCreateLyricsService
{
    /// <inheritdoc />
    public async Task EnsureCreatableAsync(AdminCreateLyricsCommand command, CancellationToken cancellationToken)
    {
        await categoryRepository.GetByIdOrThrowAsync(id: command.CategoryId, cancellationToken: cancellationToken);

        LyricsEntity? existing = await lyricsRepository.GetBySlugAsync(
            slug: command.Slug,
            cancellationToken: cancellationToken
        );

        if (existing is not null)
        {
            throw i18n.Lyrics.SlugAlreadyExists(slug: command.Slug);
        }

        if (command.VideoId.HasValue)
        {
            VideoEntity video = await videoRepository.GetByIdOrThrowAsync(
                id: command.VideoId.Value,
                cancellationToken: cancellationToken
            );
            video.MarkHasLyrics();
            videoRepository.Update(video: video);
        }
    }
}
```

Handler after: `createLyricsService`, `lyricsRepository`, `unitOfWork`,
`lyricsResponseService` — **4**. The entity construction (`CreatePaid`/`CreateFree`) stays in
the handler: it is the use case's decision, not association plumbing. `AdminUpdateLyricsHandler`
gets the mirror-image `AdminUpdateLyricsService`.

### 21.16 What shipped for every Content violator

Re-measured before the work: 49 Content handlers over budget, not 39. Every one is at 2–4
dependencies now. **R** = response service, **P** = per-slice resolution service, **S** = an
existing service absorbed a step.

| Area | Handler | Extraction | After |
| --- | --- | --- | --- |
| Catalog | `AdminAddCategoryPricing` | P `AdminAddCategoryPricingService(categoryRepo, pricingTierRepo, i18n)` | 3 |
| Catalog | `AdminRemoveCategoryPricing`, `AdminUpdateCategoryPricing` | R `CategoryPricingDtoService` | 4 |
| Catalog | `AdminAddPackageSlot` | P `AdminAddPackageSlotService(packageRepo, categoryRepo, i18n)` | 3 |
| Catalog | `AdminCreateCategory` | P `AdminCreateCategoryService(contentTypeRepo, categoryRepo, i18n)` | 4 |
| Catalog | `AdminPinCategoryToFeed` | P `AdminPinCategoryToFeedService(categoryRepo, contentTypeRepo, videoRepo, i18n, clock)` | 4 |
| Catalog | `AdminSetExclusiveCategory` | P `AdminSetExclusiveCategoryService(categoryRepo, contentTypeRepo, i18n)` | 4 |
| Catalog | `AdminUpdateCategory` | P `AdminUpdateCategoryService(categoryRepo, contentTypeRepo, i18n)` | 4 |
| Catalog | `PublicGetExclusiveCategory` | P `PublicExclusiveCategoryVideosService(videoRepo, videoDto)` | 4 |
| Commerce | `AdminAttachPaymentProof` | S `IOrderPaymentService.AttachProofAsync` absorbs the upload and the transaction | 3 |
| Commerce | `AdminCreateOrder` | S `ICreateOrderService.CreateAsync` absorbs the customer and package resolution | 2 |
| Commerce | `AdminEditOrder` | P `AdminEditOrderService(orderRepo, customerRepo, i18n)` | 3 |
| Commerce | `AdminEditOrderItem` | P `AdminEditOrderItemService(orderRepo, categoryRepo, promotionLevelRepo, i18n)` | 3 |
| Commerce | `AdminGetOrderById`, `AdminGetOrderPayment` | R `PaymentDtoService.CreateWithProofAsync` | 4, 3 |
| Editorial | `AdminCreateLyrics`, `AdminUpdateLyrics` | P `AdminCreate/UpdateLyricsService(categoryRepo, lyricsRepo, videoRepo, i18n)` + R | 4 |
| Editorial | `AdminUpdateLyricsMetadata`, `AdminUpdateLyricsSeo`, `PublicGetLyricsByVideoId` | R `LyricsDtoService` | 3 |
| Editorial | `PublicGetLyricsBySlug` | P `PublicLyricsPageService(videoRepo, artistRepo, albumRepo, streamingLinkRepo, lyricsRepo)` + R | 4 |
| Editorial | `AdminCreateArticle`, `AdminUpdateArticle` | P `AdminCreate/UpdateArticleService(categoryRepo, articleRepo, i18n)` + R | 4 |
| Editorial | `AdminUpdateArticleSeo`, `AdminGetArticleById`, `PublicGetArticleBySlug`, `PublicGetArticlePromotionFeed`, `PublicGetArtistArticles` | R `ArticleDtoService` | 2–4 |
| Editorial | `AdminCreateArtist` | P `AdminCreateArtistService(artistRepo, i18n, clock)` | 3 |
| Editorial | `AdminCreateVideo`, `AdminUpdateVideo` | P `AdminCreate/UpdateVideoService(categoryRepo, videoRepo, i18n)` | 4 |
| Editorial | the seven short-video handlers | R `ShortVideoDtoService` | 2–4 |
| Editorial | `AdminForceUnpromoteArticle`, `AdminForceUnpromoteVideo` | P `AdminForceUnpromote*Service(repo, currentActor, i18n, clock)` | 2 |
| Editorial | `AdminResolveAlbumStreamingLinks`, `AdminResolveSingleStreamingLinks` | P `Admin*LinkResolutionService(repo, resolutionPort, i18n)` | 3 |
| Editorial | `AdminApproveLyricsSubmission` | P `AdminApproveLyricsSubmissionService(submissionRepo, lyricsRepo, categoryRepo, i18n)` | 3 |
| Editorial | `PublicSubmitLyrics` | P `PublicSubmitLyricsService(artistRepo, categoryRepo, lyricsRepo, i18n)` | 4 |
| Editorial | `PublicVoteOnLyricsRevision`, `PublicVoteOnTranslationRevision` | P `Public*RevisionVoteService(revisionRepo, voteRepo, i18n)` | 3 |
| Editorial | `PublicGetArtistBySlug` | P `PublicArtistPageService(lyricsRepo, videoRepo, lyricsDto, videoDto)` | 4 |
| Editorial | `PublicGetVideoFeed` | P `PublicVideoFeedService(categoryRepo, contentTypeRepo, videoRepo)` + R | 3 |
| Interactions | `PublicAddCommentReply` | R `ArticleCommentDtoService(userLookup, fileStorage)` | 4 |

Transaction boundaries did not move (D10) with one deliberate exception: `AttachProofAsync` carries
the proof's record-and-attach transaction into `OrderPaymentService`, because the file row and the
attachment must land together and the handler no longer sees either.

## Part 6 — Testing and verification

### 21.17 Test moves

- Each new application service gets a unit test file owning its gates (mocked seams), following
  the existing `*AuthServiceTests` shape. In Identity the handler tests shrank to orchestration
  with the service mocked. In Content the handler tests compose the real slice service over the
  same repository mocks instead, so their 3,700 behaviour assertions stayed valid through a
  constructor-only change; the three editorial `*DtoService`s are covered that way too.
- Integration tests do not change: every extraction is behavior-preserving, and the rulebook's
  entry points (real HTTP, real repository) are unaffected. A green integration suite before
  and after each extraction is the proof it stayed mechanical.
- The architecture test (21.6) lands first with all 53 names; every extraction PR removes its
  handlers from `HandlerBudgetBurnDown.txt` in the same commit that fixes them.

### 21.18 Verification

```bash
dotnet build --no-incremental
dotnet csharpier check .
dotnet test tests/unit.slnf
dotnet test tests/integration.slnf
dotnet test tests/Architecture.Tests
```

The port/service split is covered by the three rules in 21.7. Then the greps this stage
exists to make return nothing:

```bash
# no application service still called a factory
grep -rn "class [A-Za-z]*Factory" --include='*.cs' src/modules/*/*/src/*.Application | grep -v SocialTokenVerifier

# no coordinator left under Infrastructure/Services
grep -rln "IFileRepository\|IMailerUnitOfWork\|INotificationRepository" --include='*.cs' \
  src/modules/*/*/src/*.Infrastructure/Services
```

And by hand:

1. `HandlerDependencyBudgetTests` green with a shrinking burn-down list; empty at stage end, then
   the file and the second test are deleted.
2. No new cross-slice references: `grep -rn "Public.*Service" src/modules/*/*/src/*.Application/*/UseCases/Admin/`
   and the mirror stay empty.
3. No handler regained a removed dependency: spot-diff the extracted handlers against this spec's
   target sets.
4. Registrations sit with the existing blocks in each module class; every new interface resolves
   (the DI smoke integration tests stay green).
