# Assertions — Identity / Sessions

## Key response types
- `AdminGetAllSessionsResponse` (`PaginatedResult<SessionDto>`); `SessionDto`
  (Id, IpAddress, UserAgent, Browser, Device, Platform, Client, ExpiresAt,
  IsActive, IsCurrent).
- Refresh-token → token response; metrics → metrics DTO; export → CSV/XLSX bytes.

The GetAllSessions tests are the **reference example** — convert their
`JsonDocument` usage to typed `ReadAsAsync<AdminGetAllSessionsResponse>()` and
keep the per-item filter assertions.

## After (export — assert content type + payload)
```csharp
var response = await Client.GetAsync($"{Routes.Admin.Sessions.Export()}?format=csv");
response.StatusCode.Should().Be(HttpStatusCode.OK);
response.Content.Headers.ContentType!.MediaType.Should().Be("text/csv");
(await response.Content.ReadAsByteArrayAsync()).Should().NotBeEmpty();
// invalid format → ShouldBeProblem(BadRequest)
```

Revoke / force-logout / cleanup must re-query the DB to assert the session was
revoked/removed. Refresh-token success returns new tokens (assert non-empty,
expiries); expired/invalid token → `ShouldBeProblem(Forbidden)`.

## TODO checklist
- [ ] AdminCleanupExpiredSessionsEndpointV1Tests.cs
- [ ] AdminExportSessionDataEndpointV1Tests.cs
- [ ] AdminForceLogoutUserEndpointV1Tests.cs
- [ ] AdminGetAllSessionsEndpointV1Tests.cs (typed conversion)
- [ ] AdminGetOwnSessionByIdEndpointV1Tests.cs
- [ ] AdminGetOwnSessionsEndpointV1Tests.cs
- [ ] AdminGetSessionMetricsEndpointV1Tests.cs
- [ ] AdminRefreshTokenEndpointV1Tests.cs
- [ ] AdminRevokeSessionEndpointV1Tests.cs
- [ ] PublicGetOwnSessionByIdEndpointV1Tests.cs
- [ ] PublicGetOwnSessionsEndpointV1Tests.cs
- [ ] PublicRefreshTokenEndpointV1Tests.cs
- [ ] PublicRevokeSessionEndpointV1Tests.cs

## Acceptance
- No `JsonDocument` left in the Sessions tests; exports assert Content-Type;
  revoke/cleanup verify DB state.
