# Authentication & authorization reference

## JWT configuration (environment variables)

```bash
JWT_SECRET=your-secret-key-min-32-chars
JWT_ISSUER=116_frontend
JWT_AUDIENCE=116_client
JWT_ACCESS_TOKEN_EXPIRATION_IN_MINUTES=60
JWT_REFRESH_TOKEN_EXPIRATION_IN_DAYS=30
```

## Auth flow

1. Login via `/api/v1/public/auth/login` (email + password) or social login.
2. Server returns `accessToken` (short-lived) and `refreshToken` (long-lived).
3. Client sends `Authorization: Bearer {accessToken}`.
4. On expiry, `/api/v1/public/auth/refresh` rotates the pair.

Token validation also checks the session revocation cache, the user's security stamp, and the
token version (`user_token_state`); a bumped version or rotated stamp kills outstanding tokens.

## Roles

Defined in `Identity/Domain/Enums/EnumCoreUserRole.cs`:

- `SuperAdmin` — full system access
- `Admin` — administrative access, cannot modify core system roles
- `Visitor` — standard public user

## Protecting endpoints

```csharp
.RequireAuthorization()                                   // any authenticated user
.RequireAuthorization(policy: UserRolePolicies.AdminOnly) // specific role policy
.AllowAnonymous()
```

Policies live in `Identity/Application/Shared/Authorizations/Policies/UserRolePolicies.cs`:
`RequireSuperAdminOnly`, `RequireAdminOnly`, `RequireVisitorOnly`, `RequireAdminOrSuperAdmin`.

Custom requirement: `AccountStatusRequirement` — the caller must be active and verified.
