# API surface reference

## URL pattern

```text
/api/v{version}/{scope}/{resource}/{action}
```

- **Scopes**: `public` (unauthenticated), `admin` (authenticated).
- **Versions**: URL path (`/api/v1/...`); the segment is always required. The
  `X-Api-Version` header is read as well and must agree with the path segment; a header naming
  a different version is refused.

## Endpoint examples

```text
POST /api/v1/public/auth/login          # Public login
POST /api/v1/public/auth/signup         # Public signup
POST /api/v1/admin/auth/login           # Admin login
GET  /api/v1/admin/users                # List users (admin)
GET  /api/v1/admin/sessions             # List sessions
POST /api/v1/admin/roles                # Create role
```

## Creating an endpoint (Carter module)

```csharp
public class MyEndpointV1 : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/admin/resource", HandleAsync)
            .WithName("MyEndpoint")
            .WithApiVersionSet(VersionSets.Default)
            .MapToApiVersion(1)
            .RequireAuthorization()
            .WithRateLimiting(RateLimitPolicies.ContentBrowsing);
    }

    private static async Task<IResult> HandleAsync(
        MyRequest request,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var command = new MyCommand(request.Data);
        var result = await dispatcher.DispatchAsync(command, ct);
        return Results.Ok(result);
    }
}
```

## Rate limiting policies

| Policy | Use case | Strategy |
| --- | --- | --- |
| `Authentication` | Login, credentials | Sliding window |
| `Otp` | OTP verification | Sliding window |
| `PasswordManagement` | Password reset/change | Sliding window |
| `FileUpload` | File uploads | Token bucket |
| `DataExport` | Session/data export | Token bucket |
| `ContentBrowsing` | Read endpoints | Fixed window |
| `UserProfile` | Profile operations | Fixed window |
| `SessionManagement` | Session operations | Fixed window |
| `AdminMetrics` | Admin metrics | Fixed window |
