# Factory Pattern — Identity Module

This document captures exactly how the Identity module implements its factory pattern, so the same approach can be replicated in other modules.

---

## What a Factory Is (and Is Not)

A **factory** in this codebase is an application-layer service that encapsulates a **multi-step, stateful operation** that a handler should not own directly — typically because it:

- Coordinates multiple repositories and/or services in a specific sequence
- Shares identical logic across two or more handlers (e.g., Admin login and Public login both need a session)
- Would bloat a handler beyond its single responsibility

A factory is **not**:
- A domain factory (`Entity.Create()` static method — that lives on the entity itself)
- A DTO mapper (that is Mapster / the mapper extensions)
- A generic repository or service

---

## Anatomy of a Factory

Every factory follows the same four-file structure, co-located with the use case that owns it.

```
UseCases/Admin/Commands/UpdateAvatar/
├── Contracts/
│   └── IAdminUpdateAvatarAuthFactory.cs   ← interface + output record (same file)
├── AdminUpdateAvatarAuthFactory.cs         ← implementation
├── AdminUpdateAvatarHandler.cs             ← handler that injects the factory
├── AdminUpdateAvatarCommand.cs
├── AdminUpdateAvatarValidator.cs
└── V1/
    └── AdminUpdateAvatarEndpointV1.cs
```

The one exception is `ISessionFactory` / `SessionFactory`, which live in `Application/Session/Factories/` because they are shared across every login, signup, social-login, and refresh-token flow.

---

## Part 1 — Output Record + Interface (same file, `Contracts/`)

The output record and interface are declared together in the `Contracts/` subfolder.

**Pattern: simple data carrier**
```csharp
// Contracts/IPublicLoginAuthFactory.cs

public record PublicLoginAuthData(UserEntity User, List<RolePermissionEntity> UserPermissions);

public interface IPublicLoginAuthFactory
{
    Task<PublicLoginAuthData> AuthenticateAsync(
        string credentials,
        string password,
        CancellationToken cancellationToken
    );
}
```

**Pattern: multi-method interface (two-phase update)**
```csharp
// Contracts/IAdminUpdateAvatarAuthFactory.cs

public record AdminUpdateAvatarAuthData(UserEntity User);

public interface IAdminUpdateAvatarAuthFactory
{
    // Phase 1: load and validate
    Task<AdminUpdateAvatarAuthData> GetUserForAvatarUpdateAsync(
        Guid userId,
        Guid sessionId,
        CancellationToken cancellationToken
    );

    // Phase 2: mutate and persist
    Task<AdminUpdateAvatarAuthData> UpdateAvatarAsync(
        UserEntity user,
        Guid avatarFileId,
        CancellationToken cancellationToken
    );
}
```

**Pattern: OTP factory (returns entity directly)**
```csharp
// Contracts/IPublicForgotPasswordOtpFactory.cs

public interface IPublicForgotPasswordOtpFactory
{
    Task<OtpEntity> CreatePasswordResetOtpAsync(Guid userId, CancellationToken cancellationToken);
}
```

Rules:
- The output record is always in the same file as the interface (no separate file).
- The record uses named positional properties (`User`, `UserPermissions`), never anonymous objects.
- If the factory is shared (like `ISessionFactory`), the output record and interface live in the shared `Factories/` folder.

---

## Part 2 — Implementation

The implementation class lives next to the `Contracts/` folder (one level up).

### Authentication factory (read-only + validation)

```csharp
// PublicLoginAuthFactory.cs

public class PublicLoginAuthFactory(
    IAuthRepository authRepository,
    IPasswordService passwordService
) : IPublicLoginAuthFactory
{
    public async Task<PublicLoginAuthData> AuthenticateAsync(
        string credentials,
        string password,
        CancellationToken cancellationToken
    )
    {
        UserEntity? user = await authRepository.GetUserWithRolesAndPermissionsByCredentialsOrThrow(
            credentials: credentials,
            cancellationToken: cancellationToken
        );

        if (!passwordService.Verify(password: password, hash: user!.PasswordHash))
        {
            throw UserErrors.InvalidCredentials();
        }

        user.ValidateCanLogin();

        List<RolePermissionEntity> userPermissions =
            user.UserRoles.SelectMany(ur => ur.Role.RolePermissions).ToList();

        return new PublicLoginAuthData(User: user, UserPermissions: userPermissions);
    }
}
```

### Registration factory (creates entities + commits)

```csharp
// PublicSignUpAuthFactory.cs

public class PublicSignUpAuthFactory(
    IAuthRepository authRepository,
    IOtpRepository otpRepository,
    IPasswordService passwordService,
    IOtpService otpService,
    IIdentityUnitOfWork unitOfWork
) : IPublicSignUpAuthFactory
{
    public async Task<PublicSignUpAuthData> RegisterAsync(
        string email,
        string userName,
        string password,
        CancellationToken cancellationToken
    )
    {
        await authRepository.ValidateUniqueCredentialsAsync(
            userName: userName,
            email: new Email(value: email),
            cancellationToken: cancellationToken
        );

        string hashedPassword = passwordService.Hash(password: password);

        var newUser = UserEntity.Create(
            Guid.NewGuid(),
            userName: userName,
            passwordHash: hashedPassword,
            email: new Email(value: email)
        );

        await authRepository.AddAsync(user: newUser, cancellationToken: cancellationToken);
        await authRepository.AssignVisitorRoleAsync(userId: newUser.Id, cancellationToken: cancellationToken);

        OtpEntity verificationOtp = otpService.CreateOtp(
            userId: newUser.Id,
            purpose: EnumOtpPurpose.EmailVerification
        );

        await otpRepository.AddAsync(otp: verificationOtp, cancellationToken: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        UserEntity? userWithRoles = await authRepository.GetUserWithRolesAndPermissionsByCredentialsOrThrow(
            new Email(value: email),
            cancellationToken: cancellationToken
        );

        List<RolePermissionEntity> userPermissions = userWithRoles!
            .UserRoles.SelectMany(ur => ur.Role.RolePermissions)
            .ToList();

        return new PublicSignUpAuthData(User: userWithRoles, UserPermissions: userPermissions);
    }
}
```

### OTP factory (minimal — create + persist)

```csharp
// PublicForgotPasswordOtpFactory.cs

public class PublicForgotPasswordOtpFactory(
    IOtpRepository otpRepository,
    IOtpService otpService,
    IIdentityUnitOfWork unitOfWork
) : IPublicForgotPasswordOtpFactory
{
    public async Task<OtpEntity> CreatePasswordResetOtpAsync(
        Guid userId,
        CancellationToken cancellationToken
    )
    {
        OtpEntity passwordResetOtp = otpService.CreateOtp(
            userId: userId,
            purpose: EnumOtpPurpose.PasswordReset
        );
        await otpRepository.AddAsync(otp: passwordResetOtp, cancellationToken: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return passwordResetOtp;
    }
}
```

### Two-phase update factory (validate → mutate)

```csharp
// AdminUpdateAvatarAuthFactory.cs

public class AdminUpdateAvatarAuthFactory(
    IAuthRepository authRepository,
    IIdentityUnitOfWork unitOfWork
) : IAdminUpdateAvatarAuthFactory
{
    public async Task<AdminUpdateAvatarAuthData> GetUserForAvatarUpdateAsync(
        Guid userId,
        Guid sessionId,
        CancellationToken cancellationToken
    )
    {
        UserEntity? user = await authRepository.GetUserWithRolesAndPermissionsByIdOrThrow(
            userId: userId,
            cancellationToken: cancellationToken
        );

        authRepository.IsUserAccountActive(user!);
        await authRepository.IsSessionValidAsync(sessionId, cancellationToken);

        return new AdminUpdateAvatarAuthData(User: user!);
    }

    public async Task<AdminUpdateAvatarAuthData> UpdateAvatarAsync(
        UserEntity user,
        Guid avatarFileId,
        CancellationToken cancellationToken
    )
    {
        user.UpdateAvatar(avatarFileId: avatarFileId, avatarSource: EnumAvatarSource.Manual);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminUpdateAvatarAuthData(User: user);
    }
}
```

### Shared session factory (cross-use-case, lives in `Session/Factories/`)

```csharp
// SessionFactory.cs

public class SessionFactory(
    IJwtService jwtService,
    IRefreshTokenService refreshTokenService,
    ISessionRepository sessionRepository,
    ISessionMetadataService sessionMetadataService,
    IIdentityUnitOfWork unitOfWork
) : ISessionFactory
{
    public async Task<SessionResult> CreateSessionAsync(
        UserEntity user,
        List<RolePermissionEntity> userPermissions,
        CancellationToken cancellationToken
    )
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        var (_, _, _, _, refreshTokenExpirationMinutes) = AppEnvironment.Jwt();
        string refreshToken = refreshTokenService.GenerateRefreshToken();
        string refreshTokenHash = refreshTokenService.HashRefreshToken(refreshToken: refreshToken);
        DateTime refreshTokenExpiresAt = now
            .AddMinutes(int.Parse(refreshTokenExpirationMinutes!))
            .UtcDateTime;

        string? deviceId = sessionMetadataService.ExtractDeviceId();
        if (string.IsNullOrWhiteSpace(deviceId))
            throw SessionErrors.DeviceIdRequired();

        SessionEntity? existingActiveSession = await sessionRepository.GetActiveSessionByUserIdAndDeviceIdAsync(
            userId: user.Id,
            deviceId: deviceId,
            cancellationToken: cancellationToken
        );

        Guid sessionId;
        if (existingActiveSession != null)
        {
            sessionId = existingActiveSession.Id;
            existingActiveSession.UpdateRefreshToken(refreshTokenHash, refreshTokenExpiresAt);
        }
        else
        {
            sessionId = Guid.NewGuid();
            string? ipAddress = sessionMetadataService.ExtractIpAddress();
            string? userAgent = sessionMetadataService.ExtractUserAgent();
            ClientOriginInfo clientOrigin = sessionMetadataService.GetClientOriginInfo();
            EnumClient clientApp = sessionMetadataService.ExtractClientApp();

            var session = SessionEntity.Create(
                id: sessionId,
                userId: user.Id,
                deviceId: deviceId,
                refreshTokenHash: refreshTokenHash,
                expiresAt: refreshTokenExpiresAt,
                browser: clientOrigin.Browser,
                device: clientOrigin.Device,
                platform: clientOrigin.Platform,
                client: clientApp,
                ipAddress: ipAddress,
                userAgent: userAgent
            );

            await sessionRepository.CreateAsync(session: session, cancellationToken: cancellationToken);
        }

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        JwtGenerationResult accessToken = jwtService.GenerateToken(
            userId: user.Id,
            sessionId: sessionId,
            email: user.Email!,
            userName: user.UserName,
            userRoles: user.UserRoles,
            userPermissions: userPermissions,
            isVerified: user.IsVerified,
            isActive: user.IsActive,
            authProvider: user.AuthProvider
        );

        return new SessionResult(
            RefreshToken: refreshToken,
            AccessToken: accessToken.Token,
            AccessTokenExpiresAt: accessToken.ExpiresAt,
            RefreshTokenExpiresAt: refreshTokenExpiresAt
        );
    }
}
```

---

## Part 3 — How Handlers Use Factories

Handlers inject factories via primary constructor parameters. The handler never touches repositories or services directly for anything the factory already owns.

```csharp
// PublicLoginHandler.cs

public class PublicLoginHandler(
    IPublicLoginAuthFactory authFactory,    // ← auth factory
    ISessionFactory sessionFactory,         // ← shared session factory
    IFileRepository fileRepository,         // ← cross-module (Core)
    IMapper mapper
) : ICommandHandler<PublicLoginCommand, PublicLoginResult>
{
    public async Task<PublicLoginResult> Handle(
        PublicLoginCommand command,
        CancellationToken cancellationToken
    )
    {
        // Step 1 — authenticate via factory
        PublicLoginAuthData authData = await authFactory.AuthenticateAsync(
            credentials: command.Credentials,
            password: command.Password,
            cancellationToken: cancellationToken
        );

        // Step 2 — create session via shared factory
        SessionResult sessionData = await sessionFactory.CreateSessionAsync(
            user: authData.User,
            userPermissions: authData.UserPermissions,
            cancellationToken: cancellationToken
        );

        // Step 3 — resolve avatar (cross-module concern, handler owns this)
        FileEntity? avatarFile = await fileRepository.GetAvatarFileAsync(
            avatarFileId: authData.User.AvatarFileId,
            cancellationToken: cancellationToken
        );

        // Step 4 — map to response
        var avatarDto = avatarFile?.ToFileDto(mapper);
        var userDto = authData.User.ToUserResponseDto(
            mapper: mapper,
            roles: authData.User.UserRoles.ToRoleDtos(mapper),
            permissions: authData.User.UserRoles.ToPermissionDtos(mapper),
            avatar: avatarDto
        );

        var authResult = new AuthenticationResult(
            User: userDto,
            AccessToken: sessionData.AccessToken,
            AccessTokenExpiresAt: sessionData.AccessTokenExpiresAt,
            RefreshToken: sessionData.RefreshToken,
            RefreshTokenExpiresAt: sessionData.RefreshTokenExpiresAt
        );

        return new PublicLoginResult(AuthenticationResult: authResult);
    }
}
```

**Key rule**: The handler only maps and assembles the final response. All business logic, persistence, and domain mutations belong in the factory.

---

## Part 4 — DI Registration

All factories are registered as **`Scoped`** in `IdentityModule.cs`. Scoped ensures they share the same DbContext instance as the repositories within a single HTTP request.

```csharp
// IdentityModule.cs (lines 138–155)

services.AddScoped<ISessionFactory, SessionFactory>();
services.AddScoped<IPublicSignUpAuthFactory, PublicSignUpAuthFactory>();
services.AddScoped<IAdminLoginAuthFactory, AdminLoginAuthFactory>();
services.AddScoped<IPublicLoginAuthFactory, PublicLoginAuthFactory>();
services.AddScoped<IPublicSocialLoginAuthFactory, PublicSocialLoginAuthFactory>();
services.AddScoped<IPublicUpdateProfileAuthFactory, PublicUpdateProfileAuthFactory>();
services.AddScoped<IAdminUpdateProfileAuthFactory, AdminUpdateProfileAuthFactory>();
services.AddScoped<IPublicUpdateAvatarAuthFactory, PublicUpdateAvatarAuthFactory>();
services.AddScoped<IAdminUpdateAvatarAuthFactory, AdminUpdateAvatarAuthFactory>();
services.AddScoped<IPublicResetPasswordAuthFactory, PublicResetPasswordAuthFactory>();
services.AddScoped<IAdminResetPasswordAuthFactory, AdminResetPasswordAuthFactory>();
services.AddScoped<IPublicForgotPasswordOtpFactory, PublicForgotPasswordOtpFactory>();
services.AddScoped<IAdminForgotPasswordOtpFactory, AdminForgotPasswordOtpFactory>();
services.AddScoped<IPublicResendOtpFactory, PublicResendOtpFactory>();
services.AddScoped<IAdminResendOtpFactory, AdminResendOtpFactory>();
services.AddScoped<IPublicSignOutSessionFactory, PublicSignOutSessionFactory>();
services.AddScoped<IAdminSignOutSessionFactory, AdminSignOutSessionFactory>();
services.AddScoped<IPublicRefreshTokenFactory, PublicRefreshTokenFactory>();
```

---

## Summary Table

| Type | Scope | Output record | UoW owned by factory? | Shared? |
|------|-------|---------------|----------------------|---------|
| Auth factory (login) | use-case | `XxxAuthData` | No | No |
| Auth factory (signup) | use-case | `XxxAuthData` | Yes — registers user + OTP | No |
| OTP factory | use-case | `OtpEntity` (direct) | Yes | No (Public + Admin are separate) |
| Update factory (profile/avatar) | use-case | `XxxAuthData` | Yes — phase 2 only | No |
| Session factory | `Session/Factories/` | `SessionResult` | Yes — creates/rotates session | **Yes** — reused by all auth flows |

---

## Decision Guide — When to Create a Factory

| Situation | Decision |
|-----------|----------|
| Handler only calls one repository and maps | No factory needed |
| Logic is identical for Public and Admin variants | One shared factory (like `ISessionFactory`) |
| Logic differs per variant but steps are complex | Separate factories per variant |
| Handler would call 3+ services/repositories in sequence | Extract into factory |
| The sequence is a distinct business sub-operation (authenticate, register, reset) | Extract into factory |