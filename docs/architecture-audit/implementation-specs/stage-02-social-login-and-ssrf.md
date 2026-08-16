# Stage 2 — Social-login verification & SSRF

Closes **[07 S1]** (`social-login` trusts a client-supplied email → account takeover) and
**[05 §1]** (the same endpoint's avatar-URL SSRF). Both live on
`POST /api/v1/public/auth/social-login`.

> **Breaking change.** The request body changes from
> `(Email, UserName, AvatarUrl, Provider)` to `(Provider, IdToken)`. Backend and every client
> (web + mobile) must deploy together. See [Rollout](#rollout).

> **Depends on Stage 1** (branch stacks on `fix-log-error-and-page-leaks`). Finalize this spec's
> code against the tree Stage 1 lands on.

---

## Checklist

- [x] 2.1 — `ISocialTokenVerifier` + `SocialTokenPayload` + `ISocialTokenVerifierFactory` + neutral exceptions + `SocialAuthConstants` (`Application/Adapters/SocialAuth`)
- [x] 2.2 — `GoogleTokenVerifier` (`Google.Apis.Auth`) + `FacebookTokenVerifier` (debug_token) + `SocialTokenVerifierFactory` (`Infrastructure/Adapters/SocialAuth`)
- [x] 2.3 — `AppEnvironment.SocialAuth()` + `.env.template` + typed options + DI registration
- [x] 2.4 — `PublicSocialLoginRequest`/`Command` → `(Provider, IdToken)`; validator rewrite
- [x] 2.5 — Handler verifies the token, rejects unverified email, passes the **verified** payload
- [x] 2.6 — `UserEntity.ProviderSubjectId` + `LinkProviderSubject`; `CreateExternal` takes the subject id
- [x] 2.7 — `UserConfiguration`: column + filtered unique `(AuthProvider, ProviderSubjectId)` index
- [x] 2.8 — `AuthRepository.GetOrCreateExternalUserAsync`: match subject-id first, reject email/subject mismatch, link legacy rows
- [x] 2.9 — New auth error messages (`InvalidProviderToken`, `ProviderEmailNotVerified`, `ProviderMismatch`, `UnsupportedProvider`) in all 3 `.resx`
- [x] 2.10 — `UrlSafetyGuard` in Core (loopback/private/link-local/ULA/multicast + non-default port + non-https-outside-dev)
- [x] 2.11 — `FileService`: run the guard, follow redirects manually (guarded, capped), stop echoing provider text
- [x] 2.12 — `CoreModule`: typed `HttpClient` with `AllowAutoRedirect=false` + 5s connect / 10s total timeout
- [x] 2.13 — Migration `AddUserProviderSubjectId` (leave unapplied)
- [x] 2.14 — Unit + integration tests (stub `ISocialTokenVerifier` in `ApiFixture`)
- [x] 2.15 — Verify (build 0/0, unit green; run integration locally)

---

## Part A — Provider-token verification `[07 S1]`

### 2.1 The port, DTO & factory (`Application/Adapters/SocialAuth`)

This is a ports-&-adapters pair, following the existing `Wangkanai.Detection` precedent
(`Application/Adapters/…` = port + boundary DTO, `Infrastructure/Adapters/…` = vendor-mapping
implementation). The port is **i18n-free**: adapters throw the neutral exceptions below, and the
**handler** maps them to localized errors (§2.5) — Infrastructure never depends on `IdentityI18n`.

`src/Modules/Identity/Identity/Application/Adapters/SocialAuth/ISocialTokenVerifier.cs` (new)

```csharp
using _116.Identity.Domain.Enums;

namespace _116.Identity.Application.Adapters.SocialAuth;

/// <summary>
/// The identity a social provider asserts once its token has been cryptographically verified. Every
/// field is taken from the verified token, never from the client request.
/// </summary>
/// <param name="ProviderSubjectId">
/// The provider's stable, opaque user identifier (Google <c>sub</c>, Facebook user id). Immutable for
/// the life of the account and the primary match key.
/// </param>
/// <param name="Email">The provider-asserted email address.</param>
/// <param name="EmailVerified">Whether the provider vouches the email is verified.</param>
/// <param name="Name">The display name, when the provider supplies one.</param>
/// <param name="PictureUrl">The avatar URL, when the provider supplies one.</param>
public sealed record SocialTokenPayload(
    string ProviderSubjectId,
    string Email,
    bool EmailVerified,
    string? Name,
    string? PictureUrl
);

/// <summary>
/// Verifies a social provider's identity token. One adapter per supported provider; the adapter
/// validates the token's signature, audience and expiry against the provider directly and translates
/// the provider's model into <see cref="SocialTokenPayload"/> — a token that does not verify must
/// throw <see cref="SocialTokenVerificationException"/>, never return a payload.
/// </summary>
public interface ISocialTokenVerifier
{
    /// <summary>
    /// The provider this verifier handles.
    /// </summary>
    EnumAuthProvider Provider { get; }

    /// <summary>
    /// Verifies <paramref name="idToken"/> with the provider and returns the asserted identity.
    /// </summary>
    Task<SocialTokenPayload> VerifyAsync(string idToken, CancellationToken cancellationToken);
}
```

`src/Modules/Identity/Identity/Application/Adapters/SocialAuth/ISocialTokenVerifierFactory.cs` (new)

```csharp
using _116.Identity.Domain.Enums;

namespace _116.Identity.Application.Adapters.SocialAuth;

/// <summary>
/// Resolves the <see cref="ISocialTokenVerifier"/> for a provider. Throws
/// <see cref="UnsupportedProviderException"/> when no adapter is registered, so "provider we do not
/// support" is one explicit failure rather than a null-ref deep in a handler.
/// </summary>
public interface ISocialTokenVerifierFactory
{
    ISocialTokenVerifier For(EnumAuthProvider provider);
}
```

`src/Modules/Identity/Identity/Application/Adapters/SocialAuth/SocialTokenExceptions.cs` (new) —
plain, i18n-free exceptions that form the port's failure contract:

```csharp
using _116.Identity.Domain.Enums;

namespace _116.Identity.Application.Adapters.SocialAuth;

/// <summary>
/// Thrown by an adapter when a provider token fails verification (bad signature, expired, wrong
/// audience/app). Carries no provider text; the handler maps it to a localized error.
/// </summary>
public sealed class SocialTokenVerificationException : Exception;

/// <summary>
/// Thrown by <see cref="ISocialTokenVerifierFactory"/> when no adapter is registered for a provider.
/// </summary>
public sealed class UnsupportedProviderException(EnumAuthProvider provider) : Exception
{
    public EnumAuthProvider Provider { get; } = provider;
}
```

`src/Modules/Identity/Identity/Application/Adapters/SocialAuth/SocialAuthConstants.cs` (new) — the
provider endpoints, kept out of the adapters as named constants:

```csharp
namespace _116.Identity.Application.Adapters.SocialAuth;

/// <summary>
/// Fixed provider endpoints for social-token verification. Pinned here so the Graph API version and
/// paths are declared once, not scattered as magic strings across the adapters and DI wiring.
/// </summary>
public static class SocialAuthConstants
{
    public const string FacebookGraphBaseUrl = "https://graph.facebook.com/v19.0/";
    public const string FacebookDebugTokenEndpoint = "debug_token";
    public const string FacebookProfileEndpoint = "me";
    public const string FacebookProfileFields = "id,name,email,picture";
}
```

`SocialAuthOptions` (the adapter credentials) also lives here — see §2.3.

### 2.2 Provider adapters (`Infrastructure/Adapters/SocialAuth`)

Add the package to `src/Modules/Identity/Identity/Identity.csproj` (pin the latest stable at
implementation time):

```xml
<PackageReference Include="Google.Apis.Auth" Version="<latest-stable>" />
```

`src/Modules/Identity/Identity/Infrastructure/Adapters/SocialAuth/GoogleTokenVerifier.cs` (new)

```csharp
using _116.Identity.Application.Adapters.SocialAuth;
using _116.Identity.Domain.Enums;
using Google.Apis.Auth;
using Microsoft.Extensions.Options;

namespace _116.Identity.Infrastructure.Adapters.SocialAuth;

/// <summary>
/// Verifies a Google ID token via <see cref="GoogleJsonWebSignature"/>, pinning the audience to the
/// configured client id so a token minted for another app is rejected. Maps Google's payload to
/// <see cref="SocialTokenPayload"/>; the handler owns localization, so failures throw the neutral
/// <see cref="SocialTokenVerificationException"/>.
/// </summary>
public sealed class GoogleTokenVerifier(IOptions<SocialAuthOptions> options) : ISocialTokenVerifier
{
    private readonly SocialAuthOptions _options = options.Value;

    /// <inheritdoc />
    public EnumAuthProvider Provider => EnumAuthProvider.Google;

    /// <inheritdoc />
    public async Task<SocialTokenPayload> VerifyAsync(string idToken, CancellationToken cancellationToken)
    {
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [_options.GoogleClientId],
            };

            GoogleJsonWebSignature.Payload payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

            return new SocialTokenPayload(
                ProviderSubjectId: payload.Subject,
                Email: payload.Email,
                EmailVerified: payload.EmailVerified,
                Name: payload.Name,
                PictureUrl: payload.Picture
            );
        }
        catch (InvalidJwtException)
        {
            throw new SocialTokenVerificationException();
        }
    }
}
```

`src/Modules/Identity/Identity/Infrastructure/Adapters/SocialAuth/FacebookTokenVerifier.cs` (new)

```csharp
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using _116.Identity.Application.Adapters.SocialAuth;
using _116.Identity.Domain.Enums;
using Microsoft.Extensions.Options;

namespace _116.Identity.Infrastructure.Adapters.SocialAuth;

/// <summary>
/// Verifies a Facebook access token by calling the Graph API's <c>debug_token</c> endpoint with the
/// app's own <c>{app-id}|{app-secret}</c> token, then reads the profile. A token whose <c>is_valid</c>
/// is false or whose <c>app_id</c> is not ours is rejected. Facebook only returns an email once the
/// user has confirmed it, so a present email is treated as verified. Failures throw the neutral
/// <see cref="SocialTokenVerificationException"/>.
/// </summary>
public sealed class FacebookTokenVerifier(HttpClient httpClient, IOptions<SocialAuthOptions> options)
    : ISocialTokenVerifier
{
    private readonly SocialAuthOptions _options = options.Value;

    /// <inheritdoc />
    public EnumAuthProvider Provider => EnumAuthProvider.Facebook;

    /// <inheritdoc />
    public async Task<SocialTokenPayload> VerifyAsync(string idToken, CancellationToken cancellationToken)
    {
        string appToken = $"{_options.FacebookAppId}|{_options.FacebookAppSecret}";

        DebugTokenResponse? debug = await httpClient.GetFromJsonAsync<DebugTokenResponse>(
            $"{SocialAuthConstants.FacebookDebugTokenEndpoint}"
                + $"?input_token={Uri.EscapeDataString(idToken)}&access_token={Uri.EscapeDataString(appToken)}",
            cancellationToken
        );

        if (debug?.Data is not { IsValid: true } data || data.AppId != _options.FacebookAppId)
        {
            throw new SocialTokenVerificationException();
        }

        ProfileResponse? profile = await httpClient.GetFromJsonAsync<ProfileResponse>(
            $"{SocialAuthConstants.FacebookProfileEndpoint}"
                + $"?fields={SocialAuthConstants.FacebookProfileFields}&access_token={Uri.EscapeDataString(idToken)}",
            cancellationToken
        );

        if (profile is null || string.IsNullOrWhiteSpace(profile.Id))
        {
            throw new SocialTokenVerificationException();
        }

        return new SocialTokenPayload(
            ProviderSubjectId: profile.Id,
            Email: profile.Email ?? string.Empty,
            EmailVerified: !string.IsNullOrWhiteSpace(profile.Email),
            Name: profile.Name,
            PictureUrl: profile.Picture?.Data?.Url
        );
    }

    private sealed record DebugTokenResponse([property: JsonPropertyName("data")] DebugTokenData? Data);

    private sealed record DebugTokenData(
        [property: JsonPropertyName("is_valid")] bool IsValid,
        [property: JsonPropertyName("app_id")] string? AppId
    );

    private sealed record ProfileResponse(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("name")] string? Name,
        [property: JsonPropertyName("email")] string? Email,
        [property: JsonPropertyName("picture")] PictureNode? Picture
    );

    private sealed record PictureNode([property: JsonPropertyName("data")] PictureData? Data);

    private sealed record PictureData([property: JsonPropertyName("url")] string? Url);
}
```

`src/Modules/Identity/Identity/Infrastructure/Adapters/SocialAuth/SocialTokenVerifierFactory.cs` (new)
— keyed-DI resolution, no scan:

```csharp
using _116.Identity.Application.Adapters.SocialAuth;
using _116.Identity.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace _116.Identity.Infrastructure.Adapters.SocialAuth;

/// <summary>
/// Resolves the keyed <see cref="ISocialTokenVerifier"/> registered for a provider.
/// </summary>
public sealed class SocialTokenVerifierFactory(IServiceProvider serviceProvider) : ISocialTokenVerifierFactory
{
    /// <inheritdoc />
    public ISocialTokenVerifier For(EnumAuthProvider provider)
    {
        ISocialTokenVerifier? verifier = serviceProvider.GetKeyedService<ISocialTokenVerifier>(provider);

        if (verifier is not null)
        {
            return verifier;
        }
        else
        {
            throw new UnsupportedProviderException(provider);
        }
    }
}
```

### 2.3 Config, env & DI

`src/Shared/Shared/Application/Configurations/Environment.cs` — add alongside `Jwt()`/`Cloudinary()`:

```csharp
/// <summary>
/// Social-auth provider credentials. Google needs its OAuth client id (used as the token audience);
/// Facebook needs its app id and secret for the debug_token app access token.
/// </summary>
public static (string? googleClientId, string? facebookAppId, string? facebookAppSecret) SocialAuth()
{
    string? googleClientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID");
    string? facebookAppId = Environment.GetEnvironmentVariable("FACEBOOK_APP_ID");
    string? facebookAppSecret = Environment.GetEnvironmentVariable("FACEBOOK_APP_SECRET");
    return (googleClientId, facebookAppId, facebookAppSecret);
}
```

`src/Modules/Identity/Identity/Application/Adapters/SocialAuth/SocialAuthOptions.cs` (new)

```csharp
namespace _116.Identity.Application.Adapters.SocialAuth;

/// <summary>
/// Verified-social-login provider credentials, bound once at startup so a misconfigured deploy fails
/// fast instead of at the first login attempt.
/// </summary>
public sealed class SocialAuthOptions
{
    public string GoogleClientId { get; init; } = string.Empty;
    public string FacebookAppId { get; init; } = string.Empty;
    public string FacebookAppSecret { get; init; } = string.Empty;
}
```

`.env.template` — add:

```
# Social login (verified provider tokens)
GOOGLE_CLIENT_ID=
FACEBOOK_APP_ID=
FACEBOOK_APP_SECRET=
```

`IdentityModule.cs` (`AddIdentityModule`) — bind options + register the adapters **keyed by provider**
+ the factory. Facebook is a typed `HttpClient` (Graph base address) bridged to its key:

```csharp
var (googleClientId, facebookAppId, facebookAppSecret) = AppEnvironment.SocialAuth();
services.Configure<SocialAuthOptions>(options =>
{
    options.GoogleClientId = googleClientId ?? string.Empty;
    options.FacebookAppId = facebookAppId ?? string.Empty;
    options.FacebookAppSecret = facebookAppSecret ?? string.Empty;
});

// Google: local JWT verification, keyed by provider.
services.AddKeyedScoped<ISocialTokenVerifier, GoogleTokenVerifier>(EnumAuthProvider.Google);

// Facebook: typed HttpClient (Graph introspection), bridged to its provider key.
services.AddHttpClient<FacebookTokenVerifier>(client =>
{
    client.BaseAddress = new Uri(SocialAuthConstants.FacebookGraphBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(10);
});
services.AddKeyedScoped<ISocialTokenVerifier>(
    EnumAuthProvider.Facebook,
    (sp, _) => sp.GetRequiredService<FacebookTokenVerifier>()
);

services.AddScoped<ISocialTokenVerifierFactory, SocialTokenVerifierFactory>();
```

> Keying by `EnumAuthProvider` lets the factory resolve the exact adapter via
> `GetKeyedService<ISocialTokenVerifier>(provider)` — no enumerate-and-filter. The Facebook typed
> client can't be keyed directly, so it's registered normally then bridged to its key by a factory
> delegate.

### 2.4 Request / command / validator (breaking)

`PublicSocialLoginEndpointV1.cs` — request + command construction:

```csharp
// before: public record PublicSocialLoginRequest(string Email, string UserName, string? AvatarUrl, string Provider);
/// <summary>
/// Request model for social login. The client sends only the provider and the provider-issued ID
/// token; identity (email, name, avatar) is read from the verified token, never from the client.
/// </summary>
/// <param name="Provider">The social provider ("Google" or "Facebook").</param>
/// <param name="IdToken">The provider-issued ID / access token to verify.</param>
public record PublicSocialLoginRequest(string Provider, string IdToken);
```

```csharp
// inside AddRoutes handler:
var command = new PublicSocialLoginCommand(Provider: request.Provider, IdToken: request.IdToken);
```

`PublicSocialLoginCommand.cs`:

```csharp
/// <summary>
/// Command for verified social login. Carries only the provider and its ID token; the verified
/// identity is resolved server-side.
/// </summary>
public record PublicSocialLoginCommand(string Provider, string IdToken) : ICommand<PublicSocialLoginResult>;
```

`PublicSocialLoginValidator.cs`:

```csharp
public PublicSocialLoginValidator(IdentityI18n i18n)
{
    RuleFor(x => x.Provider).ValidAuthProvider(i18n.User.Validation);
    RuleFor(x => x.IdToken).NotEmpty().WithMessage(i18n.User.Validation.IdTokenRequired());
}
```

Add `IdTokenRequired()` to `ValidationErrorMessage` (+ 3 `.resx`). The old `ValidAvatarUrl` rule is
removed here; `ValidAvatarUrl`/`ValidUsername`/`ValidEmail` extensions stay for other flows.

### 2.5 Handler — verify, map failures to i18n, then hand off the verified payload

The handler is the localization boundary: it catches the adapters' neutral exceptions and maps them
to the localized errors. Infrastructure stays i18n-free.

`PublicSocialLoginHandler.cs`:

```csharp
public async Task<PublicSocialLoginResult> Handle(PublicSocialLoginCommand command, CancellationToken cancellationToken)
{
    var provider = new AuthProvider(command.Provider).Value;

    ISocialTokenVerifier verifier;
    try
    {
        verifier = verifierFactory.For(provider);
    }
    catch (UnsupportedProviderException)
    {
        throw i18n.User.UnsupportedProvider(provider.ToString());
    }

    SocialTokenPayload payload;
    try
    {
        payload = await verifier.VerifyAsync(command.IdToken, cancellationToken);
    }
    catch (SocialTokenVerificationException)
    {
        throw i18n.User.InvalidProviderToken();
    }

    if (!payload.EmailVerified || string.IsNullOrWhiteSpace(payload.Email))
    {
        throw i18n.User.ProviderEmailNotVerified();
    }

    PublicSocialLoginAuthData authData = await authFactory.AuthenticateOrCreateAsync(
        payload: payload,
        provider: provider,
        cancellationToken: cancellationToken
    );

    // ... session creation, avatar fetch, DTO mapping unchanged ...
}
```

The handler gains `ISocialTokenVerifierFactory verifierFactory` and `IdentityI18n i18n` constructor
params.

`PublicSocialLoginAuthFactory` (+ contract) — new signature takes the verified payload and the
already-parsed provider; it forwards the `ProviderSubjectId` and reads name/avatar from the payload
(no client `avatarUrl`):

```csharp
Task<PublicSocialLoginAuthData> AuthenticateOrCreateAsync(
    SocialTokenPayload payload,
    EnumAuthProvider provider,
    CancellationToken cancellationToken
);
```

```csharp
public async Task<PublicSocialLoginAuthData> AuthenticateOrCreateAsync(
    SocialTokenPayload payload,
    EnumAuthProvider provider,
    CancellationToken cancellationToken
)
{
    UserEntity? user = await authRepository.GetOrCreateExternalUserAsync(
        email: payload.Email,
        userName: payload.Name ?? payload.Email,
        authProvider: provider,
        providerSubjectId: payload.ProviderSubjectId,
        cancellationToken: cancellationToken
    );

    bool isAvatarSourceManual = user!.AvatarSource == EnumAvatarSource.Manual;
    FileEntity? avatarFileEntity = await fileRepository.UpdateAvatarUrlFromSourceAsync(
        currentAvatarFileId: user.AvatarFileId,
        avatarUrl: payload.PictureUrl,
        user.Id.ToString(),
        isAvatarSourceManual: isAvatarSourceManual,
        cancellationToken: cancellationToken
    );

    if (avatarFileEntity != null)
    {
        user.UpdateAvatar(avatarFileId: avatarFileEntity.Id, avatarSource: EnumAvatarSource.Provider);
    }

    await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

    List<RolePermissionEntity> userPermissions = user.UserRoles.SelectMany(ur => ur.Role.RolePermissions).ToList();
    return new PublicSocialLoginAuthData(User: user, UserPermissions: userPermissions);
}
```

### 2.6 `UserEntity` — the subject id

`UserEntity.cs`:

```csharp
/// <summary>
/// The provider's stable subject id (Google <c>sub</c>, Facebook user id). Null for local accounts,
/// and for legacy external accounts that predate subject-id tracking until their first verified
/// login links it.
/// </summary>
public string? ProviderSubjectId { get; private set; }
```

`CreateExternal` gains a required `providerSubjectId`:

```csharp
public static UserEntity CreateExternal(
    Guid id,
    string userName,
    EnumAuthProvider authProvider,
    string providerSubjectId,
    UserErrors errors,
    string? email = null
)
{
    if (string.IsNullOrWhiteSpace(value: userName))
    {
        throw errors.InvalidUsernameFormat(username: userName);
    }

    return new UserEntity
    {
        Id = id,
        Email = email?.ToLowerInvariant(),
        UserName = userName,
        AuthProvider = authProvider,
        ProviderSubjectId = providerSubjectId,
        IsVerified = UserConstants.ExternalAuthIsVerified,
    };
}
```

Link a legacy external row on its first verified login:

```csharp
/// <summary>
/// Associates a provider subject id with an external account that predates subject-id tracking.
/// Refuses to rebind an account already tied to a different subject — that is a mismatched token.
/// </summary>
public void LinkProviderSubject(string providerSubjectId, UserErrors errors)
{
    if (!string.IsNullOrWhiteSpace(ProviderSubjectId) && ProviderSubjectId != providerSubjectId)
    {
        throw errors.ProviderMismatch();
    }

    ProviderSubjectId = providerSubjectId;
}
```

### 2.7 EF configuration

`UserConfiguration.cs` — add the property + a **filtered** unique composite index (Postgres treats
NULLs as distinct, but the filter makes the intent explicit and keeps the index off local accounts):

```csharp
builder
    .Property(u => u.ProviderSubjectId)
    .HasMaxLength(maxLength: UserConstants.MaxProviderSubjectIdLength)
    .IsRequired(false);

builder
    .HasIndex(u => new { u.AuthProvider, u.ProviderSubjectId })
    .IsUnique()
    .HasFilter("provider_subject_id IS NOT NULL");
```

Add `MaxProviderSubjectIdLength = 255` to `UserConstants`.

### 2.8 `AuthRepository` — subject-id first, then email

`GetOrCreateExternalUserAsync` gains `string providerSubjectId` and this order of resolution:

```csharp
public async Task<UserEntity?> GetOrCreateExternalUserAsync(
    string email,
    string? userName,
    AuthProvider authProvider,
    string providerSubjectId,
    CancellationToken cancellationToken = default
)
{
    // 1) Subject-id match — the authoritative key.
    UserEntity? user = await GetUserWithRolesAndPermissionsByProviderSubjectAsync(
        authProvider: authProvider.Value,
        providerSubjectId: providerSubjectId,
        cancellationToken: cancellationToken
    );
    if (user is not null)
    {
        return user;
    }

    try
    {
        // 2) Email match — link or reject; never silently take over.
        user = await GetUserWithRolesAndPermissionsByCredentialsOrThrow(email, cancellationToken);

        if (user!.AuthProvider == EnumAuthProvider.Local)
        {
            throw userErrors.EmailAlreadyExists(email: email);
        }

        // Existing external row: link if unlinked, reject if it belongs to another subject.
        user.LinkProviderSubject(providerSubjectId, userErrors);

        if (!string.IsNullOrWhiteSpace(userName) && user.UserName != userName)
        {
            bool usernameExists = await ExistsByUserNameAsync(userName, cancellationToken);
            if (!usernameExists)
            {
                user.UpdateUserName(newUserName: userName, errors: userErrors);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
        return user;
    }
    catch (NotFoundException)
    {
        // 3) Brand-new account.
        user = UserEntity.CreateExternal(
            Guid.NewGuid(),
            userName!,
            authProvider: authProvider.Value,
            providerSubjectId: providerSubjectId,
            errors: userErrors,
            email: email
        );

        await AddAsync(user: user, cancellationToken: cancellationToken);
        await AssignVisitorRoleAsync(userId: user.Id, cancellationToken: cancellationToken);
        await context.SaveChangesAsync(cancellationToken: cancellationToken);

        return await GetUserWithRolesAndPermissionsByProviderSubjectAsync(
            authProvider.Value,
            providerSubjectId,
            cancellationToken
        );
    }
}
```

Add `GetUserWithRolesAndPermissionsByProviderSubjectAsync` next to the existing credential loader —
same `Include` graph (roles + permissions), filtered by `AuthProvider` + `ProviderSubjectId`,
returning `null` when absent (do not throw; absence is the new-account path).

### 2.9 New auth error messages

`UserErrors.cs` — new factory methods (pick the exception type per case):

```csharp
public AuthenticationException InvalidProviderToken()   => new(authentication.InvalidProviderToken());
public AccountNotVerifiedException ProviderEmailNotVerified() => new(authorization.ProviderEmailNotVerified());
public ConflictException ProviderMismatch()             => new(conflict.ProviderMismatch());
public BadRequestException UnsupportedProvider(string provider) => new(validation.UnsupportedProvider(provider));
```

Add the matching methods + keys to `AuthenticationErrorMessage`, `AuthorizationErrorMessage`,
`ConflictErrorMessage`, `ValidationErrorMessage` and **all three** `.resx` files each (neutral/en/fr).
Suggested copy:

| Key | en |
|---|---|
| `InvalidProviderToken` | The social sign-in token could not be verified. |
| `ProviderEmailNotVerified` | Your social account's email is not verified. |
| `ProviderMismatch` | This account is linked to a different social identity. |
| `UnsupportedProvider` | The social provider '{0}' is not supported. |
| `IdTokenRequired` | The provider token is required. |

---

## Part B — Avatar-URL SSRF `[05 §1]`

### 2.10 `UrlSafetyGuard` (Core Infrastructure)

`src/Modules/Core/Core/Infrastructure/Services/UrlSafetyGuard.cs` (new)

```csharp
using System.Net;
using System.Net.Sockets;
using _116.Core.Application.Shared.Errors.Facade;
using Microsoft.Extensions.Hosting;

namespace _116.Core.Infrastructure.Services;

/// <summary>
/// Rejects URLs that would make the server dial itself or the private network (SSRF). Resolves the
/// host and refuses loopback, link-local, private, unique-local, and multicast addresses, non-default
/// ports, and — outside Development — any non-HTTPS scheme.
/// </summary>
public sealed class UrlSafetyGuard(IHostEnvironment environment, CoreI18n i18n)
{
    /// <summary>
    /// Validates <paramref name="uri"/> and throws a generic download failure if it is unsafe. Every
    /// hop (initial URL and each redirect target) must pass this before it is fetched.
    /// </summary>
    public async Task EnsureSafeAsync(Uri uri, CancellationToken cancellationToken)
    {
        bool isDevelopment = environment.IsDevelopment();

        if (uri.Scheme != Uri.UriSchemeHttps && !(isDevelopment && uri.Scheme == Uri.UriSchemeHttp))
        {
            throw i18n.File.FileDownloadFailed();
        }

        if (!uri.IsDefaultPort)
        {
            throw i18n.File.FileDownloadFailed();
        }

        IPAddress[] addresses = await Dns.GetHostAddressesAsync(uri.DnsSafeHost, cancellationToken);

        if (addresses.Length == 0 || addresses.Any(IsBlocked))
        {
            throw i18n.File.FileDownloadFailed();
        }
    }

    private static bool IsBlocked(IPAddress address)
    {
        if (IPAddress.IsLoopback(address) || address.IsIPv6Multicast || address.IsIPv6LinkLocal
            || address.IsIPv6SiteLocal || address.IsIPv6UniqueLocal)
        {
            return true;
        }

        if (address.AddressFamily == AddressFamily.InterNetwork)
        {
            byte[] b = address.GetAddressBytes();
            return b[0] == 10                                   // 10.0.0.0/8
                || (b[0] == 172 && b[1] >= 16 && b[1] <= 31)    // 172.16.0.0/12
                || (b[0] == 192 && b[1] == 168)                 // 192.168.0.0/16
                || (b[0] == 169 && b[1] == 254)                 // 169.254.0.0/16 link-local
                || b[0] == 127                                  // 127.0.0.0/8
                || b[0] >= 224;                                 // 224.0.0.0/4 multicast + reserved
        }

        return false;
    }
}
```

Add a no-arg `FileDownloadFailed()` to `FileErrors` + `InternalServerErrorMessage` (+ 3 `.resx`,
key `FileDownloadFailedGeneric` → "The file could not be downloaded.") that carries **no** URL or
provider text. Register `UrlSafetyGuard` as scoped in `CoreModule`.

### 2.11 `FileService` — guard every hop, stop echoing

- Inject `UrlSafetyGuard urlSafetyGuard` and `ILogger<FileService> logger`.
- `ValidateFileUrl` stays synchronous (null/parse checks); add `await urlSafetyGuard.EnsureSafeAsync(uri, ct)` right after it in `DownloadFileAsync`, before any request.
- Because auto-redirect is now off (2.12), replace the direct `httpClient.SendAsync` calls in the metadata helpers with a private `SendGuardedAsync` that, on a 3xx, re-runs the guard on the `Location`, re-issues the request, and caps the chain (e.g. 5 hops) — throwing the generic failure when the cap is hit.
- In the `catch (HttpRequestException ex)` block, **log** `ex` and throw the generic
  `i18n.File.FileDownloadFailed()` (no `fileUrl`, no `ex.Message`). Same for the `FileStorageFailed`
  path — log the detail, return generic.

```csharp
private async Task<HttpResponseMessage> SendGuardedAsync(
    HttpMethod method,
    Uri uri,
    RangeHeaderValue? range,
    HttpCompletionOption completion,
    CancellationToken cancellationToken)
{
    const int maxHops = 5;
    Uri current = uri;

    for (var hop = 0; hop < maxHops; hop++)
    {
        await urlSafetyGuard.EnsureSafeAsync(current, cancellationToken);

        using var request = new HttpRequestMessage(method, current);
        if (range is not null)
        {
            request.Headers.Range = range;
        }

        HttpResponseMessage response = await httpClient.SendAsync(request, completion, cancellationToken);

        if (response.StatusCode is not (>= HttpStatusCode.MovedPermanently and < (HttpStatusCode)400))
        {
            return response;
        }

        Uri? location = response.Headers.Location;
        response.Dispose();

        if (location is null)
        {
            throw i18n.File.FileDownloadFailed();
        }

        current = location.IsAbsoluteUri ? location : new Uri(current, location);
    }

    throw i18n.File.FileDownloadFailed();
}
```

Route `GetFileMetadataAsync`, `TryGetContentLengthWithRangeAsync`, `TryGetContentLengthFallbackAsync`
through `SendGuardedAsync` instead of `httpClient.SendAsync` directly.

### 2.12 `CoreModule` — lock down the HttpClient

```csharp
services
    .AddHttpClient<IFileService, FileService>(client => client.Timeout = TimeSpan.FromSeconds(10))
    .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
    {
        AllowAutoRedirect = false,
        ConnectTimeout = TimeSpan.FromSeconds(5),
    });
```

### 2.13 Migration

```bash
dotnet ef migrations add AddUserProviderSubjectId \
  --project src/Modules/Identity/Identity \
  --startup-project src/Api \
  --context IdentityDbContext
```

Leave it unapplied (same as every stage). Confirm it adds the `provider_subject_id` column and the
filtered unique index, and nothing else.

---

## Tests

**Stub the verifier in the test host** — add `StubSocialTokenVerifier : ISocialTokenVerifier`
under `tests/Integration/Common/Stubs/` (scriptable payload + a "throw invalid" toggle, `IResettableStub`)
plus a tiny stub `ISocialTokenVerifierFactory` that always returns it. In
`ApiFixture.StubExternalServices`, remove the real keyed adapters + `SocialTokenVerifierFactory` and
register the stub factory instead. This is the only way to drive social-login through real HTTP
without calling Google/Facebook — the same pattern as `StubEmailSender`/`StubStreamingLinkResolutionService`.

- **Unit**
  - `UrlSafetyGuard`: throws for loopback/127.0.0.1, 10/172.16/192.168/169.254, non-default port, and (non-dev) http; passes for a normal https host. Use a `Development` and a `Production` `IHostEnvironment`.
  - `GoogleTokenVerifier`/`FacebookTokenVerifier`: map a payload; a bad token throws `InvalidProviderToken` (Facebook via a stubbed `HttpMessageHandler`).
  - Handler: unverified email → `ProviderEmailNotVerified`; unsupported provider → `UnsupportedProvider`.
  - `UserEntity`: `CreateExternal` sets `ProviderSubjectId`; `LinkProviderSubject` links when null, throws `ProviderMismatch` on a different id.
- **Integration** (stub verifier)
  - New provider user is created with the subject id; a second login with the same subject id returns the same user (no duplicate).
  - A login whose token email matches an existing **local** account → 409.
  - A token whose email matches an existing external account but with a **different** subject id → `ProviderMismatch`.
  - `EmailVerified=false` → rejected, no user created.
  - Avatar `PictureUrl` pointing at a loopback/private host → the download is rejected and the response carries **no** URL/exception text (SSRF + no-echo).

---

## Rollout

1. Provision `GOOGLE_CLIENT_ID`, `FACEBOOK_APP_ID`, `FACEBOOK_APP_SECRET` in every environment.
2. Ship the migration.
3. Deploy backend + web + mobile **together** — the request body is a breaking change.
4. Legacy external accounts link their subject id transparently on first verified login (2.6/2.8).

## Verification

1. `dotnet build 116_backend.sln` — 0 warnings / 0 errors.
2. `dotnet test tests/Unit` — green.
3. Run `tests/Integration` locally.
4. Confirm the migration only adds the column + filtered unique index.

**PR title:** `fix(auth): verify social-login provider tokens and block avatar-url SSRF`
