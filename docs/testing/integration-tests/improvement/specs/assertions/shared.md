# Assertions — Shared

Cross-cutting tests: exception handling, decorators, EF interceptors,
not-found middleware.

## Guidance
- `ExceptionHandlerTests.cs` — already covers 400/401/403/404/405/409. Convert to
  `ShouldBeProblem(status)` and assert the ProblemDetails shape (status/title and,
  if present, the stable error code) rather than status-only.
- `ValidationDecoratorTests.cs` / `LoggingDecoratorTests.cs` — thin smoke tests.
  Strengthen: validation returns a ProblemDetails enumerating the failed fields;
  logging decorator path asserts the request still succeeds/fails as expected.
- `AuditableEntityInterceptorTests.cs` — assert `CreatedAt/UpdatedAt/CreatedBy/
  UpdatedBy` populated and `UpdatedAt` changes on update.
- `DispatchDomainEventsInterceptorTests.cs` — assert the side-effect of a domain
  event firing (e.g. a projection/row written) on `SaveChanges`.
- `ResourceNotFoundMiddlewareTests.cs` — assert the 404 ProblemDetails body.

## TODO checklist
- [ ] ExceptionHandlerTests.cs — `ShouldBeProblem` for every status.
- [ ] ValidationDecoratorTests.cs — assert field-level validation problem body.
- [ ] LoggingDecoratorTests.cs — assert behavior, not just "no throw".
- [ ] AuditableEntityInterceptorTests.cs — assert audit columns.
- [ ] DispatchDomainEventsInterceptorTests.cs — assert event side-effect.
- [ ] ResourceNotFoundMiddlewareTests.cs — assert 404 problem body.

## Acceptance
- Error/validation tests assert ProblemDetails; interceptor tests assert the
  concrete persisted effect.
