# Stage 21 — Handler dependency budget

Every command and query handler holds **at most 4 constructor dependencies**. This stage states
the rule, measures the tree against it, specifies the factory extraction that brings every
violator under budget, and adds the architecture test that keeps the count from creeping back.

Measured on the current tree: **294 handlers, 53 over budget** — 39 in Content, 14 in Identity.
Distribution: 1 handler at 9, 2 at 8, 1 at 7, 15 at 6, 34 at 5.

> The Identity slice can land immediately. The Content slice deliberately waits for the
> Content pass of Stage 15 (15.9–15.16): most Content extractions touch the same handlers that
> stage rewrites, and doing them together avoids re-touching every file twice.

---

## Part 0 — The rule

### 0.1 Statement

A class implementing `ICommandHandler<,>`, `ICommandHandler<>`, or `IQueryHandler<,>` declares
at most **4 primary-constructor parameters**. Every parameter counts — repositories, factories,
services, `IUnitOfWork`, `IMapper`, i18n facades, `TimeProvider`. No exclusion list: an
exclusion list turns a mechanical check into a judgement call, and the check must stay
mechanical to sit in a test.

Factories and other application components are **not** capped by this rule, but a factory that
accumulates more than ~6 dependencies is a smell reviewed case by case: it usually means two
phases were merged into one component.

### 0.2 Why 4

A handler is the use case's orchestrator: it retrieves, delegates, applies domain transitions,
and commits. Four seams cover that shape — typically one retrieval dependency, one extracted
step component, one commit seam, and one cross-cutting service. A fifth dependency is the
signal that the handler owns a phase it should delegate: association validation, response
assembly, a security fan-out, or a protocol shared with a sibling slice.

### 0.3 What an extraction must never do

The budget is met by moving a **cohesive phase** into a factory, never by hiding wiring. Each
of these turns the fix into a worse problem than the count:

| Forbidden move | Why |
| --- | --- |
| Pass-through wrapper (`RoleDtoFactory(mapper)` with one delegating method) | Adds a seam with no responsibility; the dependency count drops on paper while coupling is unchanged. |
| God factory (a factory that executes the whole use case) | The handler becomes a pass-through and the factory becomes an unnamed handler; SRP is violated one level down. |
| Cross-slice dependency (an Admin handler injecting a `Public*` factory or vice versa) | Breaks the module's Admin/Public prefix isolation. Shared logic lives in an unprefixed factory in the area `Factories/` folder. |
| Bundling unrelated dependencies to lower the count (`IAuthServices` exposing repository + password service + i18n) | A facade over unrelated seams is constructor over-injection with extra steps; nothing gains a responsibility. |
| Moving domain rules into the factory (guards, state transitions) | R12: aggregates decide; factories load, validate associations, and assemble. A factory may *call* a guarded transition, never re-implement one. |
| A factory hiding a repository the handler also injects for the same query | Double-loading and split ownership of one read. Each read has exactly one owner. |

### 0.4 What a factory legitimately owns

The four factory species this stage uses, all with precedent in the tree:

| Species | Owns | Precedent |
| --- | --- | --- |
| **Protocol factory** (shared, unprefixed) | A multi-step invariant-bearing sequence used by sibling slices | `OtpVerificationFactory`, `SessionFactory`, `RefreshTokenFactory` |
| **Resolution factory** (per-slice, prefixed) | Loading and validating the use case's associations, throwing localized errors | `PublicLoginAuthFactory`, `ICreateOrderFactory` |
| **Security fan-out factory** (shared, unprefixed) | The revoke/rotate/bump reaction that must follow a credential change | new in this stage (`CredentialInvalidationFactory`) |
| **Response factory** (area-shared, unprefixed) | Assembling a response DTO from an aggregate plus its enrichment services (`IMapper`, `IFileRepository`, `IUserLookupService`) | new in this stage (`LyricsResponseFactory`, `CategoryResponseFactory`) |

Placement follows the existing conventions exactly:

- Per-slice factory: in the use-case folder, `Admin`/`Public`-prefixed, interface in the
  slice's `Contracts/` subfolder.
- Shared factory: in the area `Factories/` folder (`Application/Auth/Factories/`,
  `Application/Editorial/Factories/`, …), unprefixed, interface in `Factories/Contracts/`.
- Registration in the module class next to the existing factory registrations.

---

## Part 1 — The enforcement test

Lands **first**, with the current violators as an explicit burn-down list. The test fails when
a handler over budget is *not* on the list (no new debt) and when a listed handler drops under
budget without being removed (the list only shrinks). When the list is empty it is deleted.

`tests/Unit/Architecture/HandlerDependencyBudgetTests.cs`:

```csharp
using System.Reflection;
using _116.Shared.Contracts.Application.CQRS;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Architecture;

/// <summary>
/// Enforces the handler dependency budget: a command or query handler declares at most
/// <see cref="MaxDependencies"/> constructor parameters. Existing violators burn down through
/// <see cref="KnownViolators"/>; the list only ever shrinks.
/// </summary>
public class HandlerDependencyBudgetTests
{
    private const int MaxDependencies = 4;

    private static readonly Assembly[] ModuleAssemblies =
    [
        typeof(_116.Identity.IdentityModule).Assembly,
        typeof(_116.Core.CoreModule).Assembly,
        typeof(_116.Content.ContentModule).Assembly,
        typeof(_116.Mailer.MailerModule).Assembly,
    ];

    /// <summary>
    /// Handlers over budget on the day the rule landed. Remove entries as extractions ship;
    /// never add one.
    /// </summary>
    private static readonly HashSet<string> KnownViolators =
    [
        // Stage 21 Part 2 — Identity
        "AdminVerifyOtpHandler",
        "PublicVerifyOtpHandler",
        "AdminChangePasswordHandler",
        "PublicChangePasswordHandler",
        "PublicSetPasswordHandler",
        "PublicSocialLoginHandler",
        "AdminAssignRoleToUserHandler",
        "AdminRemoveRoleFromUserHandler",
        "AdminAssignPermissionToRoleHandler",
        "AdminRemovePermissionFromRoleHandler",
        "AdminSoftDeleteRoleHandler",
        "AdminSoftDeletePermissionHandler",
        "AdminUpdateAvatarHandler",
        "PublicUpdateAvatarHandler",
        // Stage 21 Part 3 — Content (lands with the Content pass of stage 15)
        "PublicGetLyricsBySlugHandler",
        "AdminCreateLyricsHandler",
        "AdminUpdateLyricsHandler",
        "AdminUpdateLyricsMetadataHandler",
        "AdminUpdateLyricsSeoHandler",
        "PublicGetLyricsByVideoIdHandler",
        "PublicSubmitLyricsHandler",
        "AdminApproveLyricsSubmissionHandler",
        "AdminCreateVideoHandler",
        "AdminUpdateVideoHandler",
        "AdminCreateShortVideoHandler",
        "PublicGetVideoBySlugHandler",
        "PublicGetPublicShortBySlugHandler",
        "PublicGetArtistBySlugHandler",
        "AdminCreateArticleHandler",
        "AdminUploadArticleImageHandler",
        "PublicGetArticleBySlugHandler",
        "PublicGetArticlePromotionFeedHandler",
        "PublicAddCommentReplyHandler",
        "AdminCreateCategoryHandler",
        "AdminUpdateCategoryHandler",
        "AdminActivateCategoryHandler",
        "AdminDeactivateCategoryHandler",
        "AdminSetExclusiveCategoryHandler",
        "AdminPinCategoryToFeedHandler",
        "AdminUploadCategoryPosterHandler",
        "AdminAddCategoryPricingHandler",
        "PublicGetExclusiveCategoryHandler",
        "AdminAddPackageSlotHandler",
        "AdminCreateOrderHandler",
        "AdminEditOrderHandler",
        "AdminEditOrderItemHandler",
        "AdminGetOrderByIdHandler",
        "AdminGetOrderPaymentHandler",
        "AdminAttachPaymentProofHandler",
        "PublicVoteOnLyricsRevisionHandler",
        "PublicVoteOnTranslationRevisionHandler",
        "AdminResolveAlbumStreamingLinksHandler",
        "AdminResolveSingleStreamingLinksHandler",
    ];

    private static bool IsHandler(Type type)
    {
        return type is { IsClass: true, IsAbstract: false }
            && type.GetInterfaces()
                .Any(i =>
                    i.IsGenericType
                    && (
                        i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>)
                        || i.GetGenericTypeDefinition() == typeof(ICommandHandler<>)
                        || i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>)
                    )
                );
    }

    private static int DependencyCount(Type handler)
    {
        return handler.GetConstructors().Max(c => c.GetParameters().Length);
    }

    [Fact]
    public void EveryHandler_StaysWithinTheDependencyBudget()
    {
        List<string> newViolators = ModuleAssemblies
            .SelectMany(a => a.GetTypes())
            .Where(IsHandler)
            .Where(h => DependencyCount(h) > MaxDependencies && !KnownViolators.Contains(h.Name))
            .Select(h => $"{h.Name} ({DependencyCount(h)})")
            .ToList();

        newViolators
            .Should()
            .BeEmpty("a handler over {0} dependencies delegates a phase to a factory instead", MaxDependencies);
    }

    [Fact]
    public void TheBurnDownList_OnlyShrinks()
    {
        List<string> alreadyFixed = ModuleAssemblies
            .SelectMany(a => a.GetTypes())
            .Where(IsHandler)
            .Where(h => KnownViolators.Contains(h.Name) && DependencyCount(h) <= MaxDependencies)
            .Select(h => h.Name)
            .ToList();

        alreadyFixed.Should().BeEmpty("remove fixed handlers from KnownViolators so the list only shrinks");
    }
}
```

---

## Part 2 — Identity (14 handlers)

### 21.1 The OTP verification protocol factory (7 → 3, 6 → 4)

`OtpVerificationFactory` currently owns only the judge-and-meter block; the handlers still
inject `IOtpRepository`, `IAccountLockoutRepository`, and `TimeProvider` around it. The factory
grows to own the **whole consumption protocol** — load, judge, consume, invalidate, clear —
which is one cohesive sequence with one reason to change. The handler keeps what is genuinely
use-case-specific: the user gates, the aggregate transition, and the commit.

The failure path still commits the metered attempt before throwing; the success path stays a
single commit, issued by the handler, covering `MarkAsUsed` and `MarkVerifiedByOtp` together —
transaction boundaries do not move.

`Application/Auth/Factories/Contracts/IOtpVerificationFactory.cs`:

```csharp
using _116.Identity.Domain.ValueObjects;

namespace _116.Identity.Application.Auth.Factories.Contracts;

/// <summary>
/// Owns the OTP consumption protocol shared by the VerifyOtp handlers: load the outstanding
/// code, judge the presented one, consume it, and retire its siblings and counters.
/// </summary>
public interface IOtpVerificationFactory
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

`Application/Auth/Factories/OtpVerificationFactory.cs`:

```csharp
using _116.Identity.Application.Auth.Factories.Contracts;
using _116.Identity.Application.Auth.Repositories;
using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Shared.Errors.Facade;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.ValueObjects;

namespace _116.Identity.Application.Auth.Factories;

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
public class OtpVerificationFactory(
    IOtpRepository otpRepository,
    IOtpService otpService,
    IAccountLockoutRepository lockoutRepository,
    IIdentityUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    IdentityI18n i18n
) : IOtpVerificationFactory
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
using _116.Identity.Application.Auth.Factories.Contracts;
using _116.Identity.Application.Auth.Repositories;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.ValueObjects;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Auth.UseCases.Admin.Commands.VerifyOtp;

/// <summary>
/// Handles the <see cref="AdminVerifyOtpCommand" /> to verify OTP codes for admin account verification.
/// </summary>
/// <param name="authRepository">Repository for user data access operations.</param>
/// <param name="otpVerificationFactory">Factory consuming the outstanding OTP.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminVerifyOtpHandler(
    IAuthRepository authRepository,
    IOtpVerificationFactory otpVerificationFactory,
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

        await otpVerificationFactory.ConsumeOtpAsync(
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
to a new `OtpVerificationFactoryTests` (mocked seams, including the commit-before-throw
inversion test); the handler tests shrink to orchestration (gates → factory call → transition
→ commit). Integration tests are untouched — the wire behavior is identical.

### 21.2 The credential invalidation factory (6/6/5 → 4/4/3)

`AdminChangePasswordHandler`, `PublicChangePasswordHandler`, and `PublicSetPasswordHandler`
each carry the same three-dependency tail: revoke the account's other sessions, commit, rotate
the security stamp. That is one security reaction — "this credential changed, no other session
may outlive it" — duplicated three times.

`Application/Auth/Factories/Contracts/ICredentialInvalidationFactory.cs`:

```csharp
namespace _116.Identity.Application.Auth.Factories.Contracts;

/// <summary>
/// Owns the security reaction to a credential change: the account's other sessions are revoked
/// and its security stamp rotated in the same flow as the new credential.
/// </summary>
public interface ICredentialInvalidationFactory
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

`Application/Auth/Factories/CredentialInvalidationFactory.cs`:

```csharp
using _116.Identity.Application.Auth.Factories.Contracts;
using _116.Identity.Application.Session.Repositories;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Enums;

namespace _116.Identity.Application.Auth.Factories;

/// <summary>
/// Commits a credential change together with the revocation of the account's other sessions,
/// then rotates the security stamp so outstanding tokens die at validation.
/// </summary>
/// <param name="sessionRepository">Repository revoking the account's sessions.</param>
/// <param name="tokenStateRepository">Repository rotating the account's security stamp.</param>
/// <param name="unitOfWork">Unit of Work committing the change and the revocations together.</param>
public class CredentialInvalidationFactory(
    ISessionRepository sessionRepository,
    IUserTokenStateRepository tokenStateRepository,
    IIdentityUnitOfWork unitOfWork
) : ICredentialInvalidationFactory
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
    ICredentialInvalidationFactory credentialInvalidationFactory,
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

        await credentialInvalidationFactory.CommitCredentialChangeAsync(
            userId: user.Id,
            exemptSessionId: command.SessionId,
            cancellationToken: cancellationToken
        );

        return new AdminChangePasswordResult(IsSuccess: true);
    }
}
```

`PublicChangePasswordHandler` mirrors it (4). `PublicSetPasswordHandler` drops to
`IAuthRepository`, `IPasswordService`, `ICredentialInvalidationFactory` (3).

### 21.3 Role and permission grant factories (6/6/6/5 → 4)

The four assignment handlers share one phase: resolve the role/permission, gate it
(deleted-first, then inactive), apply the aggregate transition, and throw the localized error
on a no-op. Each slice gets its own prefixed resolution factory — the gates differ per use
case, so this is the per-slice species, exactly like the existing `*AuthFactory`s.

Exemplar — `Application/User/UseCases/Admin/Commands/AssignRoleToUser/Contracts/IAdminAssignRoleToUserFactory.cs`:

```csharp
using _116.Identity.Domain.Entities;

namespace _116.Identity.Application.User.UseCases.Admin.Commands.AssignRoleToUser.Contracts;

/// <summary>
/// Resolves and applies a role grant: loads the role and the user, gates deleted and inactive
/// roles, and grants through the user aggregate.
/// </summary>
public interface IAdminAssignRoleToUserFactory
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

`AdminAssignRoleToUserFactory.cs` (same folder):

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
public class AdminAssignRoleToUserFactory(
    IRoleRepository roleRepository,
    IAuthRepository authRepository,
    IdentityI18n i18n
) : IAdminAssignRoleToUserFactory
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
    IAdminAssignRoleToUserFactory assignRoleFactory,
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

        RoleGrantData grant = await assignRoleFactory.GrantAsync(
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

| Handler | Factory | Handler deps after |
| --- | --- | --- |
| `AdminRemoveRoleFromUserHandler` | `AdminRemoveRoleFromUserFactory(authRepository, roleRepository, i18n)` | factory, tokenState, uow, mapper — **4** |
| `AdminAssignPermissionToRoleHandler` | `AdminAssignPermissionToRoleFactory(roleRepository, permissionRepository, i18n)` | factory, tokenState, uow, mapper — **4** |
| `AdminRemovePermissionFromRoleHandler` | `AdminRemovePermissionFromRoleFactory(roleRepository, i18n)` | factory, tokenState, uow, mapper — **4** |

### 21.4 Lifecycle factories for soft deletion (5 → 3)

`AdminSoftDeleteRoleHandler` and `AdminSoftDeletePermissionHandler` each hold
repository + uow + mapper + i18n + `TimeProvider`. The load-guard-stamp phase moves to a
per-slice factory; the handler keeps commit and response.

```csharp
/// <summary>
/// Loads the role and applies the clocked soft deletion, throwing when already deleted.
/// </summary>
public class AdminSoftDeleteRoleFactory(
    IRoleRepository roleRepository,
    IdentityI18n i18n,
    TimeProvider timeProvider
) : IAdminSoftDeleteRoleFactory
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

Handler after: factory, uow, mapper — **3**. The permission twin is identical with
`IPermissionRepository` and the permission i18n family. (Adjust the exact i18n error names to
the ones the current handlers throw; the shape is what this spec fixes.)

### 21.5 Social login and avatar handlers (6/5/5 → 4)

- `PublicSocialLoginHandler` (6): `ISocialTokenVerifierFactory` folds into
  `IPublicSocialLoginAuthFactory` — provider-token verification is the first step of social
  authentication, not a separate phase the handler sequences. The auth factory's
  `AuthenticateAsync` takes the raw provider token and verifies internally. Handler after:
  authFactory, sessionFactory, fileRepository + mapper collapse into the Identity response
  assembly below — target: authFactory, sessionFactory, userResponseFactory, i18n — **4**.
- `AdminUpdateAvatarHandler` / `PublicUpdateAvatarHandler` (5 each): `IFileUploadService` +
  `IFileRepository` + `IMapper` are one phase — store the new avatar and assemble the response.
  Shared `AvatarStorageFactory(fileUploadService, fileRepository, mapper)` in
  `Application/User/Factories/`; handler after: authFactory, avatarStorageFactory, uow — **3**.

The Identity response assembly (`fileRepository` + `mapper` pairs in the login family) reuses
the response-factory species defined for Content in 21.6; the login handlers themselves are
under budget today, so the factory is introduced only where a violator needs it.

---

## Part 3 — Content (39 handlers, lands with the Stage 15 Content pass)

Two extractions recur across nearly all 39; the remaining cases are per-slice resolution
factories.

### 21.6 Response factories — the enrichment trio (fixes ~20 handlers)

`IMapper` + `IFileRepository` (+ `IUserLookupService`) appear together wherever a handler
builds its response DTO: the mapping, the author profile, and the avatar/poster URL resolution
are one phase — response assembly. Today the trio is threaded through extension methods
(`ToLyricsDetailDtoAsync(mapper, userLookup, fileRepository, …)`), which pushes three
dependencies into every calling handler. Each content area gets one injectable response
factory wrapping its existing extensions:

`Application/Editorial/Factories/LyricsResponseFactory.cs`:

```csharp
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Identity.Contracts.Application;
using MapsterMapper;

namespace _116.Content.Application.Editorial.Factories;

/// <summary>
/// Assembles lyrics response DTOs, resolving the author profile and file URLs the wire shape
/// carries.
/// </summary>
/// <param name="mapper">The Mapster mapper used for tags.</param>
/// <param name="userLookup">Service for resolving author profiles from the Identity module.</param>
/// <param name="fileRepository">Repository for resolving avatar and cover image file URLs.</param>
public class LyricsResponseFactory(
    IMapper mapper,
    IUserLookupService userLookup,
    IFileRepository fileRepository
) : ILyricsResponseFactory
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
become its private implementation detail once all callers migrate. One factory per area:
`LyricsResponseFactory`, `VideoResponseFactory`, `ArticleResponseFactory`,
`CategoryResponseFactory(fileRepository, mapper)`, `OrderResponseFactory(mapper, userLookup,
fileRepository)`, `ShortVideoResponseFactory`.

### 21.7 The lyrics page read (9 → 4)

`PublicGetLyricsBySlugHandler` stitches five repositories into one page. The link-and-sibling
assembly is one phase with one reason to change — what the lyrics page shows around the lyrics:

`Application/Editorial/UseCases/Public/Queries/GetLyricsBySlug/Contracts/IPublicLyricsPageFactory.cs`:

```csharp
namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetLyricsBySlug.Contracts;

/// <summary>
/// Assembles the navigation shell of a lyrics page: linked slugs, sibling album tracks, and
/// streaming links.
/// </summary>
public interface IPublicLyricsPageFactory
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

The implementation (`PublicLyricsPageFactory`) takes `IVideoRepository`, `IArtistRepository`,
`IAlbumRepository`, `IStreamingLinkRepository`, `ILyricsRepository` and moves the existing
lines 54–112 of the handler verbatim — no behavior change, including the album/no-album
branch and the `StreamingLinkFactory` composition. Handler after (**4**):

```csharp
public class PublicGetLyricsBySlugHandler(
    ILyricsRepository lyricsRepository,
    IPublicLyricsPageFactory pageFactory,
    ILyricsResponseFactory responseFactory,
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

        LyricsPageLinks links = await pageFactory.ResolveLinksAsync(
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

        var dto = await responseFactory.ToPublicDetailDtoAsync(
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

### 21.8 Lyrics create/update association factories (8 → 4)

`AdminCreateLyricsHandler`'s pre-persist phase — category existence, slug uniqueness, video
resolution and `MarkHasLyrics` — moves to a per-slice resolution factory:

```csharp
/// <summary>
/// Validates a lyrics creation's associations: the category exists, the slug is free, and the
/// linked video (when present) is resolved and marked as having lyrics.
/// </summary>
public class AdminCreateLyricsFactory(
    ICategoryRepository categoryRepository,
    ILyricsRepository lyricsRepository,
    IVideoRepository videoRepository,
    ContentI18n i18n
) : IAdminCreateLyricsFactory
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

Handler after: `createLyricsFactory`, `lyricsRepository`, `unitOfWork`,
`lyricsResponseFactory` — **4**. The entity construction (`CreatePaid`/`CreateFree`) stays in
the handler: it is the use case's decision, not association plumbing. `AdminUpdateLyricsHandler`
gets the mirror-image `AdminUpdateLyricsFactory`.

### 21.9 Assignment table — every remaining Content violator

Extraction species: **R** = response factory (21.6), **P** = per-slice resolution factory,
**S** = existing/shared factory absorbs a step.

| Handler (deps now) | Extraction | Target dependencies (count) |
| --- | --- | --- |
| `AdminUpdateLyricsMetadataHandler` (5) | R | lyricsRepo, uow, lyricsResponse — **3** |
| `AdminUpdateLyricsSeoHandler` (5) | R | lyricsRepo, uow, lyricsResponse — **3** |
| `PublicGetLyricsByVideoIdHandler` (5) | R | lyricsRepo, lyricsResponse, i18n — **3** |
| `PublicSubmitLyricsHandler` (6) | P `PublicSubmitLyricsFactory(artistRepo, categoryRepo, lyricsRepo, i18n)` | factory, submissionRepo, uow — **3** |
| `AdminApproveLyricsSubmissionHandler` (5) | P `AdminApproveLyricsSubmissionFactory(categoryRepo, lyricsRepo, i18n)` | factory, submissionRepo, uow — **3** |
| `AdminCreateVideoHandler` (6) | P `AdminCreateVideoFactory(categoryRepo, i18n)` + R | factory, videoRepo, uow, videoResponse — **4** |
| `AdminUpdateVideoHandler` (6) | same shapes | factory, videoRepo, uow, videoResponse — **4** |
| `AdminCreateShortVideoHandler` (5) | R | shortVideoRepo, uow, shortVideoResponse, i18n — **4** |
| `PublicGetVideoBySlugHandler` (5) | P `PublicVideoPageFactory(artistRepo, fileRepo)` + R | videoRepo, pageFactory, videoResponse, i18n — **4** |
| `PublicGetPublicShortBySlugHandler` (5) | R | shortVideoRepo, shortVideoResponse, i18n — **3** |
| `PublicGetArtistBySlugHandler` (5) | P `PublicArtistPageFactory(lyricsRepo, videoRepo, fileRepo)` | artistRepo, pageFactory, i18n — **3** |
| `AdminCreateArticleHandler` (6) | P `AdminCreateArticleFactory(categoryRepo, i18n)` + R | factory, articleRepo, uow, articleResponse — **4** |
| `AdminUploadArticleImageHandler` (5) | S storage: `ArticleImageStorageFactory(cloudinary, fileUploadService, mapper)` | articleRepo, storageFactory, uow — **3** |
| `PublicGetArticleBySlugHandler` (5) | R (absorbs interactionRepo like-lookup into the response call or keep) | articleRepo, interactionRepo, articleResponse, i18n — **4** |
| `PublicGetArticlePromotionFeedHandler` (5) | R | articleRepo, interactionRepo, categoryRepo, articleResponse — **4** |
| `PublicAddCommentReplyHandler` (5) | R (comment author enrichment) | commentRepo, uow, commentResponse, i18n — **4** |
| `AdminCreateCategoryHandler` (6) | P `AdminCreateCategoryFactory(contentTypeRepo, i18n)` + R | factory, categoryRepo, uow, categoryResponse — **4** |
| `AdminUpdateCategoryHandler` (5) | R | categoryRepo, uow, categoryResponse, i18n — **4** |
| `AdminActivateCategoryHandler` (5) | R | categoryRepo, uow, categoryResponse, i18n — **4** |
| `AdminDeactivateCategoryHandler` (5) | R | categoryRepo, uow, categoryResponse, i18n — **4** |
| `AdminSetExclusiveCategoryHandler` (5) | R | categoryRepo, uow, categoryResponse, i18n — **4** |
| `AdminPinCategoryToFeedHandler` (6) | P (video resolution) + R | factory, categoryRepo, uow, categoryResponse — **4** |
| `AdminUploadCategoryPosterHandler` (5) | S storage: `CategoryPosterStorageFactory(fileUploadService, fileRepo, mapper)` | categoryRepo, storageFactory, uow — **3** |
| `AdminAddCategoryPricingHandler` (5) | P `AdminAddCategoryPricingFactory(pricingTierRepo, i18n)` | factory, categoryRepo, uow, mapper — **4** |
| `PublicGetExclusiveCategoryHandler` (5) | P (video resolution) + R | categoryRepo, pageFactory, categoryResponse, i18n — **4** |
| `AdminAddPackageSlotHandler` (5) | P `AdminAddPackageSlotFactory(categoryRepo, i18n)` | factory, packageRepo, uow, mapper — **4** |
| `AdminCreateOrderHandler` (6) | S: `ICreateOrderFactory` absorbs customer/package resolution + i18n | createOrderFactory, orderRepo, uow — **3** |
| `AdminEditOrderHandler` (5) | P `AdminEditOrderFactory(customerRepo, i18n)` | factory, orderRepo, uow, mapper — **4** |
| `AdminEditOrderItemHandler` (6) | P `AdminEditOrderItemFactory(categoryRepo, promotionLevelRepo, i18n)` | factory, orderRepo, uow, mapper — **4** |
| `AdminGetOrderByIdHandler` (5) | R order | orderRepo, orderResponse, i18n — **3** |
| `AdminGetOrderPaymentHandler` (5) | R order (absorbs userLookup/fileRepo/mapper) | orderPaymentFactory, orderRepo, orderResponse — **3** |
| `AdminAttachPaymentProofHandler` (5) | S: `IOrderPaymentFactory` absorbs the upload step | orderPaymentFactory, orderRepo, uow, mapper — **4** |
| `PublicVoteOnLyricsRevisionHandler` (5) | P `PublicLyricsRevisionVoteFactory(revisionRepo, voteRepo, i18n)` | factory, lyricsRepo, uow — **3** |
| `PublicVoteOnTranslationRevisionHandler` (5) | P mirror | factory, translationRepo, uow — **3** |
| `AdminResolveAlbumStreamingLinksHandler` (5) | P `AdminAlbumLinkResolutionFactory(albumRepo, resolutionService, i18n)` | factory, streamingLinkRepo, uow — **3** |
| `AdminResolveSingleStreamingLinksHandler` (5) | P mirror over lyricsRepo | factory, streamingLinkRepo, uow — **3** |

Target dependency sets are the specification of *shape*; if a listed set proves wrong at
implementation time (a dependency the table missed), the budget still binds — re-cut the
factory, do not add a fifth parameter.

---

## Part 4 — Testing and verification

### 21.10 Test moves

- Each new factory gets a unit test file owning its branch matrix (mocked seams), following
  the existing `*AuthFactoryTests` shape. Behavior tests that lived in the handler test move
  with the behavior; the handler tests keep orchestration only.
- Integration tests do not change: every extraction is behavior-preserving, and the rulebook's
  entry points (real HTTP, real repository) are unaffected. A green integration suite before
  and after each extraction is the proof it stayed mechanical.
- The architecture test (Part 1) lands first with all 53 names; every extraction PR removes
  its handlers from `KnownViolators` in the same commit that fixes them.

### 21.11 Verification

1. Build 0/0, csharpier, full unit + integration suites green after every extraction batch.
2. `HandlerDependencyBudgetTests` green with a shrinking `KnownViolators`; empty at stage end,
   then the list and its second test are deleted.
3. No new cross-slice references: `grep -rn "Public.*Factory" src/Modules/*/*/Application/*/UseCases/Admin/` and the mirror stay empty.
4. No handler regained a removed dependency: spot-diff the extracted handlers against this
   spec's target sets.
5. Factory registrations sit with the existing factory blocks in each module class; every new
   interface resolves (the DI smoke integration tests stay green).

## Decisions

| # | Decision |
| --- | --- |
| D1 | The budget counts **every** constructor parameter, cross-cutting seams included. Mechanical rules stay mechanical; the pressure this creates is intentional and is what pushes response assembly and security fan-outs into factories. |
| D2 | Factories are exempt from the budget but soft-capped at ~6 dependencies, reviewed case by case. |
| D3 | Response factories are the sanctioned owner of the enrichment trio (`IMapper`, `IFileRepository`, `IUserLookupService`); the DTO extension methods become their implementation detail. |
| D4 | The Content slice waits for the Stage 15 Content pass; the Identity slice and the enforcement test land immediately. The burn-down list makes the interim state explicit and monotonic. |
| D5 | Transaction boundaries never move in an extraction: the handler that committed before commits after. A factory commits only where the current code already commits mid-flow (the OTP metered-attempt write). |
