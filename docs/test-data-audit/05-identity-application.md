# Identity — Application suites

Area: `Identity.Unit.Tests/Application/`, `Identity.Integration.Tests/Application/`. 261 files
(195 unit / 42,344 lines, 66 integration / 7,405 lines). **All 66 integration files read in full**;
102 of 195 unit files read in full, 1 partially, 92 covered by targeted greps for each defect shape
already established from the files that were read.

This area has the largest number of adoptable sites in the repository, and almost all of it is mock
arrangement rather than entity data — a class the pattern-matching pass missed entirely.

## F6 — 69 mock setups that stub nothing, with a helper that exists

```csharp
_authRepositoryMock.Setup(x => x.IsUserAccountActive(user));   // no .Returns(...)
```

There is no `.Returns(...)`, so the `bool`-returning member returns `false` at every one of these sites.
That is harmless today only because production discards the value — the guard throws from inside the
repository (`IAuthRepository.cs:227-235`, and `AdminResetPasswordAuthFactory.cs:48-49` calls
`authRepository.IsUserAdmin(user!);` as a bare statement) — but the setup expresses the opposite of what
the test means.

| Member | Sites | Use |
| --- | --- | --- |
| `IsUserAccountActive` | 41 | `SetupIsUserAccountActiveReturnsTrue()` (`MockAuthRepository.cs:259-268`) |
| `IsUserAccountVerified` | 23 | `SetupIsUserAccountVerifiedReturnsTrue()` (`:270-275`) |
| `IsUserAdmin` | 5 | `SetupIsUserAdminReturnsTrue()` (`:328-337`) |

Files: `AdminResetPasswordAuthFactoryTests.cs`, `PublicResetPasswordAuthFactoryTests.cs`,
`AdminUpdateProfileAuthFactoryTests.cs` (9 sites), `PublicUpdateProfileAuthFactoryTests.cs` (13),
`AdminUpdateAvatarAuthFactoryTests.cs` (4), `PublicUpdateAvatarAuthFactoryTests.cs` (5).
The `Verify(..., Times.Once)` assertions keep passing because the helper matches `It.IsAny<UserEntity>()`.

## F8 — 82 refresh-token and session-lookup setups with existing helpers

| Member | Sites | Use |
| --- | --- | --- |
| `HashRefreshToken` / `GenerateRefreshToken` | 55 | `SetupHashRefreshToken(token, hash)` / `SetupGenerateRefreshToken(token)` (`MockRefreshTokenService.cs:29-52`) |
| `GetByRefreshTokenHashAsync` | 27 | `SetupGetByRefreshTokenHash(hash, session)` / `…ReturnsNull(hash)` (`MockSessionRepository.cs:55-80`) |

Files: `AdminSignOutSessionFactoryTests.cs` (16), `PublicSignOutSessionFactoryTests.cs` (16),
`SessionFactoryTests.cs` (16), `PublicRefreshTokenFactoryTests.cs` (40). The helper bodies are literal
copies of the call sites.

## F7 — 52 repository setups with existing helpers

| Member | Sites | Use |
| --- | --- | --- |
| `GetUserWithRolesAndPermissionsByIdOrThrow` | 35 | `SetupGetUserWithRolesAndPermissionsById(user)` (`MockAuthRepository.cs:133-141`) |
| `FindUserByIdOrThrow` | 8 | `SetupFindUserByIdOrThrow(user)` (`:32-36`) |
| `GetUserWithRolesByEmailOrThrow` | 5 | `SetupGetUserWithRolesByEmailOrThrow(new Email(email), user)` (`:58`) |
| `IsSessionValidAsync` | 4 | `SetupIsSessionValid(sessionId)` (`:282-291`) |

Behaviour-preserving wherever `user.Id == userId`, which holds at every site (all build via
`UserFactory.CreateWithId(userId)` or `new UserBuilder().WithId(userId)`). One exception:
`AccountStatusRequirementHandlerTests.cs:167-170` returns `null` deliberately and the `NotFound` helper
throws instead — leave it.

## F4 — 53 password-service setups with existing helpers

| Pattern | Sites | Use |
| --- | --- | --- |
| `Verify(...).Returns(true)` | 10 | `SetupVerifySuccess(password, hash)` |
| `Verify(...).Returns(false)` | 20 | `SetupVerifyFailure(password, hash)` |
| `Hash(...)` | 23 | `SetupHash(password, hash)` |

(`MockPasswordService.cs:30-34, 87-93, 104-112`.) `SetupVerifySuccess` returns `hash is not null`, and
every `user` here comes from `UserFactory`, which always sets `TestConstants.User.DefaultPasswordHash`.
Files: the two `ChangePassword` handler tests, the two `ResetPassword` auth-factory tests, and
`PublicSetPasswordHandlerTests.cs` (7 `Hash` sites).

## F5 — 10 test classes build raw `new Mock<T>()` where `Mock*.Create()` exists

`IAuthRepository` at `AdminResetPasswordAuthFactoryTests.cs:40`, `PublicResetPasswordAuthFactoryTests.cs:40`,
`PublicSocialLoginAuthFactoryTests.cs:42`, `AccountStatusRequirementHandlerTests.cs:27`,
`AdminUpdateAvatarAuthFactoryTests.cs:32`, `AdminUpdateProfileAuthFactoryTests.cs:28`,
`PublicUpdateAvatarAuthFactoryTests.cs:32`, `PublicUpdateProfileAuthFactoryTests.cs:33`;
`ISessionRepository` at 7 sites; `IIdentityUnitOfWork` in the same 10 files; `IRefreshTokenService` at 4;
`IJwtService` at 3; `IPasswordService` at 2; `ISessionMetadataService` at 1.

Each `Create()` adds only *default* setups, and every test that cares re-stubs the same member
afterwards, where a later `Setup` wins in Moq. One caveat: `MockPasswordService.Create()`'s default
`Verify → false` becomes live for argument pairs the two ResetPassword files never stub — same outcome
as today (Moq's `bool` default), different route.

## F9 — factory plus post-hoc mutation where a factory names the state

| Sites | Today | Use |
| --- | --- | --- |
| `OtpSpecificationsTests.cs:241-242, 309-310, 346-347` | `OtpFactory.Create(...); otp.MarkAsUsed(now)` | `OtpFactory.CreateUsed(...)` (`OtpFactory.cs:197-198`, `:167-168`) |
| `PermissionSpecificationsTests.cs:122-123, 222-223, 257-258` | `PermissionFactory.Create(); permission.Deactivate()` | `PermissionFactory.CreateInactive()` |
| `RoleSpecificationsTests.cs:122-123, 222-223, 253-254, 273-274` | `RoleFactory.Create(); role.Deactivate()` | `RoleFactory.CreateInactive()` |
| 12 integration sites across `PublicChangePassword`, `AdminChangePassword`, `AdminForgotPassword`, `AdminLogin`, `PublicLogin`, `PublicResetPassword` endpoint tests | `UserFactory.Create(email); user.MarkAsVerified(); user.Activate()` | `new UserBuilder().WithEmail(email).AsVerified().Build()` — `Activate()` is a no-op, the builder defaults to active |

The last row only shortens the block: the sites that follow with `user.InitializePasswordHash(hash)`
still need a mutation, because the builder has no `WithPasswordHash`.

## Smaller findings

- **F1** — `AdminGetAllSessionsEndpointV1Tests.cs:50-65` holds a local `CreateSessionWithIp` helper calling `SessionEntity.Create` with 11 arguments; the builder's defaults are already Chrome/Desktop/Windows/WebApp with a random device id → `new SessionBuilder().WithUserId(userId).WithIpAddress(ipAddress).Build()`. Used at `:113, 114, 236, 237, 239`.
- **F3** — `AdminLoginHandlerTests.cs:61-67` news a `SessionResult` with the same four literals as `AuthTestHelpers.CreateDefaultSessionResult()`, which lines `94, 121, 151, 287, 315, 343` of the same file already call. (`:180-186` is correctly direct — it captures the expiry locals to assert on them.)
- **F10** — `FileValidationTests.cs:82-91` holds a local `CreateMockFile` that duplicates `FileTestHelpers.CreateMockFormFile` line for line; every other file in the area already uses the shared one. Used at `:136, 163, 191, 218, 234`.
- **F11** — `RoleFactory.CreateWithId(Guid.NewGuid(), "Visitor")` at 8 integration sites where the id is never used → `RoleFactory.Create(TestConstants.Role.VisitorName)`; the builder's default id is already random.
- **F2** — three request records newed inline (`AdminUpdateOwnProfileEndpointV1Tests.cs:57, 78`; `PublicUpdateOwnProfileEndpointV1Tests.cs:167`) where the builder exists but **cannot express the shape**: it has no `WithUserName`/`WithCountryName`/`WithCountryIsoCode` and defaults the country fields to US values. Extend the builder first; this is a two-part change, not a swap.

## Dead for a reason — do not "fix"

`MockSessionRepository.SetupGetActiveSessionByUserIdAndDeviceId{,ReturnsNull}` target
`GetActiveSessionByUserIdAndDeviceIdAsync`, while the only caller-shaped code (`SessionFactoryTests`)
stubs the **different** method `GetSessionByUserIdAndDeviceIdAsync` (`ISessionRepository.cs:89` vs `:113`).
Swapping in the existing helper would silently stop matching. The fix is a new helper — see
[10-missing-factories.md](10-missing-factories.md).

## Two dead-line observations

- `permission.Activate()` / `role.Activate()` on a builder-built entity is a no-op — the builders default `_isActive = true`. 10 dead lines: `PermissionSpecificationsTests.cs:108, 208, 238, 273, 339`; `RoleSpecificationsTests.cs:108, 208, 238, 289, 351`.
- `"123456"` / `"12345"` literals stand in for `TestConstants.Otp.ValidCode` and a too-short code at ~30 sites, while `AdminResetPasswordValidatorTests.cs:37` already uses the constant — so the intent is settled, the rest just drifted.
