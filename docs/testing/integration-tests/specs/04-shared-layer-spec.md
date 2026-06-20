# Phase 2: Shared Layer Tests Spec

## Tasks

### Interceptors
- [ ] `AuditableEntityInterceptorTests.cs` — via BaseRepositoryTest
  - [ ] SaveChanges_NewEntity_ShouldSetCreatedAtAndUpdatedAt
  - [ ] SaveChanges_UpdatedEntity_ShouldUpdateOnlyUpdatedAt
  - [ ] SaveChanges_ViaApi_ShouldSetCreatedByToAuthenticatedUserId (BaseApiTest)
- [ ] `DispatchDomainEventsInterceptorTests.cs` — via BaseRepositoryTest
  - [ ] SaveChanges_EntityWithDomainEvent_ShouldDispatchAndClearEvents
  - [ ] SaveChanges_EntityWithNoDomainEvents_ShouldNotThrow
  - [ ] SaveChanges_ViaApi_ShouldTriggerDomainEventHandler (BaseApiTest)

### Decorators
- [ ] `ValidationDecoratorTests.cs` — via BaseApiTest
  - [ ] Post_WithInvalidPayload_ShouldReturn422WithFieldErrors
  - [ ] Post_WithValidPayload_ShouldPassThroughToHandler
  - [ ] Post_WithMultipleValidationErrors_ShouldAggregateAllErrors
- [ ] `LoggingDecoratorTests.cs` — via BaseApiTest
  - [ ] Command_ShouldCompleteWithoutLoggingErrors
  - [ ] FailedCommand_ShouldStillReturnErrorResponse

### Exception Handlers (13 strategies)
- [ ] `BadRequestExceptionHandlerTests.cs`
- [ ] `ConflictExceptionHandlerTests.cs`
- [ ] `AuthenticationExceptionHandlerTests.cs`
- [ ] `AuthorizationExceptionHandlerTests.cs`
- [ ] `NotFoundExceptionHandlerTests.cs`
- [ ] `ResourceNotFoundExceptionHandlerTests.cs`
- [ ] `RateLimitExceptionHandlerTests.cs`
- [ ] `InternalServerExceptionHandlerTests.cs`
- [ ] `InvalidFormatExceptionHandlerTests.cs`
- [ ] `MethodNotAllowedExceptionHandlerTests.cs`
- [ ] `BadGatewayExceptionHandlerTests.cs`
- [ ] `OtpAttemptsLimitExceptionHandlerTests.cs`
- [ ] `OtpExpirationExceptionHandlerTests.cs`

Each exception handler test verifies:
- Correct HTTP status code returned
- ProblemDetails body structure
- Error message content

### Middleware
- [ ] `ResourceNotFoundMiddlewareTests.cs`
  - [ ] Request_ToNonExistentRoute_ShouldReturn404WithProblemDetails
- [ ] `SwaggerDescriptionMiddlewareTests.cs`
  - [ ] Swagger_ShouldRenderDescriptions

## File Locations

```
tests/_116.Integration.Tests/
└── Shared/
    ├── Interceptors/
    │   ├── AuditableEntityInterceptorTests.cs
    │   └── DispatchDomainEventsInterceptorTests.cs
    ├── Decorators/
    │   ├── ValidationDecoratorTests.cs
    │   └── LoggingDecoratorTests.cs
    ├── ExceptionHandlers/
    │   └── (13 test files)
    └── Middleware/
        ├── ResourceNotFoundMiddlewareTests.cs
        └── SwaggerDescriptionMiddlewareTests.cs
```

## Test Approach

- Interceptor tests use real EF Core `SaveChangesAsync` — cannot be mocked
- Decorator tests go through the full HTTP pipeline via `BaseApiTest`
- Exception handler tests trigger real exceptions through the API and verify the HTTP response
- Middleware tests send requests to specific routes and verify middleware behavior

## Acceptance Criteria

1. All interceptor tests verify real EF Core behavior
2. All decorator tests verify real CQRS pipeline behavior
3. All exception handlers return correct status codes and ProblemDetails
4. `./scripts/run-tests-with-coverage.sh integration` passes
