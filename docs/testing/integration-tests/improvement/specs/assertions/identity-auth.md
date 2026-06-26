# Assertions — Identity / Auth

Upgrade every Auth endpoint test to typed body + ProblemDetails + side-effect
assertions per [`../../01-assertion-quality.md`](../../01-assertion-quality.md).

## Key response types
- `PublicLoginMobileResponse` (User, AccessToken, AccessTokenExpiresAt,
  RefreshToken, RefreshTokenExpiresAt, TokenType) — login/social-login.
- OTP/verify/password commands → typed result records in each use case's `V1`
  endpoint file (confirm per endpoint).

## Before
```csharp
var response = await Client.PostAsJsonAsync(Routes.Public.Auth.Login(), request);
response.StatusCode.Should().Be(HttpStatusCode.OK);
```

## After
```csharp
var response = await Client.PostAsJsonAsync(Routes.Public.Auth.Login(), request);
response.StatusCode.Should().Be(HttpStatusCode.OK);

var body = await response.ReadAsAsync<PublicLoginMobileResponse>();
body.TokenType.Should().Be("Bearer");
body.AccessToken.Should().NotBeNullOrWhiteSpace();
body.RefreshToken.Should().NotBeNullOrWhiteSpace();
body.AccessTokenExpiresAt.Should().BeAfter(DateTime.UtcNow);
body.RefreshTokenExpiresAt.Should().BeAfter(body.AccessTokenExpiresAt);
body.User.Email.Should().Be(request.Email);
```

Error paths (wrong password, expired/invalid OTP, already verified, max attempts,
same-as-old, etc.): `await response.ShouldBeProblem(<status>)` with the right
status (401/400/409/410/429/423). For OTP/sign-out, re-query the DB to confirm
the session/OTP state changed.

## TODO checklist
- [ ] AdminChangePasswordEndpointV1Tests.cs
- [ ] AdminForgotPasswordEndpointV1Tests.cs
- [ ] AdminLoginEndpointV1Tests.cs
- [ ] AdminResendOtpEndpointV1Tests.cs
- [ ] AdminResetPasswordEndpointV1Tests.cs
- [ ] AdminSignOutEndpointV1Tests.cs
- [ ] AdminSignOutFromAllDevicesEndpointV1Tests.cs
- [ ] AdminVerifyOtpEndpointV1Tests.cs
- [ ] PublicChangePasswordEndpointV1Tests.cs
- [ ] PublicForgotPasswordEndpointV1Tests.cs
- [ ] PublicLoginEndpointV1Tests.cs
- [ ] PublicResendOtpEndpointV1Tests.cs
- [ ] PublicResetPasswordEndpointV1Tests.cs
- [ ] PublicSetPasswordEndpointV1Tests.cs
- [ ] PublicSignOutEndpointV1Tests.cs
- [ ] PublicSignOutFromAllDevicesEndpointV1Tests.cs
- [ ] PublicSignUpEndpointV1Tests.cs
- [ ] PublicSocialLoginEndpointV1Tests.cs
- [ ] PublicVerifyOtpEndpointV1Tests.cs

## Acceptance
- Each happy path deserializes a typed response and asserts ≥2 meaningful fields.
- Each error path uses `ShouldBeProblem`.
- Sign-out / OTP tests verify DB session/OTP state.
