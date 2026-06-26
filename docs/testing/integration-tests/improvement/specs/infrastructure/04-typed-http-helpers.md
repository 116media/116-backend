# Infra Spec 04 — Typed HTTP / ProblemDetails / pagination helpers

Prerequisite for all assertion specs. Add to
`tests/Integration/Common/Extensions/HttpClientExtensions.cs` (or a new
`HttpResponseAssertions.cs`).

## Problem
Tests have no typed way to read a response body, so 87% don't read it at all and
the rest use stringly-typed `JsonDocument`. There's no helper to assert a
ProblemDetails error or pagination envelope.

## After — helpers

```csharp
public static class HttpResponseExtensions
{
    private static readonly JsonSerializerOptions Json =
        new(JsonSerializerDefaults.Web);   // match API casing

    /// Deserialize the body into the real src response record.
    public static async Task<T> ReadAsAsync<T>(this HttpResponseMessage res)
    {
        var value = await res.Content.ReadFromJsonAsync<T>(Json);
        value.Should().NotBeNull();
        return value!;
    }

    /// Assert an RFC7807 ProblemDetails error with the expected status (+ optional code).
    public static async Task ShouldBeProblem(
        this HttpResponseMessage res, HttpStatusCode status, string? errorCode = null)
    {
        res.StatusCode.Should().Be(status);
        var problem = await res.Content.ReadFromJsonAsync<ProblemDetails>(Json);
        problem.Should().NotBeNull();
        problem!.Status.Should().Be((int)status);
        problem.Title.Should().NotBeNullOrWhiteSpace();
        if (errorCode is not null)
            problem.Extensions.Should().ContainKey("code")   // adjust to actual problem shape
                   .WhoseValue!.ToString().Should().Contain(errorCode);
    }
}
```

Confirm the API's actual error contract first (inspect
`src/Shared/Shared/Application/Exceptions/Handlers/*` and one real 400/409
response) so `ShouldBeProblem` matches the emitted shape (standard
`ProblemDetails` vs a custom envelope, and whether a stable error code is
present). Adjust the helper accordingly.

For pagination, prefer deserializing into the real `PaginatedResult<T>` envelope
used by src responses and asserting `Items`, `PageIndex`, `PageSize`, `Count`
directly — no separate helper needed once `ReadAsAsync<T>` exists.

## TODO checklist
- [ ] Inspect the real error body shape; finalize `ShouldBeProblem`.
- [ ] Add `ReadAsAsync<T>` + `ShouldBeProblem` (+ JSON options matching the API).
- [ ] Confirm `PaginatedResult<T>` is deserializable from the response and document the property names.
- [ ] `dotnet build tests/Integration` — 0 errors.

## Acceptance
- A test can do `var body = await res.ReadAsAsync<AdminGetAllSessionsResponse>();`
  and assert typed fields.
- A test can do `await res.ShouldBeProblem(HttpStatusCode.Conflict);`.
