# Phase 1: Test Infrastructure Spec

## Tasks

- [ ] Implement `PostgresFixture.cs`
- [ ] Implement `ApiFixture.cs`
- [ ] Implement `BaseApiTest.cs`
- [ ] Implement `BaseRepositoryTest.cs`
- [ ] Implement `HttpClientExtensions.cs`
- [ ] Implement `TestDataSeeder.cs`
- [ ] Implement `StubCloudinaryService.cs`
- [ ] Implement `StubYoutubeThumbnailService.cs`
- [ ] Verify all infrastructure compiles

## PostgresFixture

Reference: `02-testcontainers-fixture.md`

```
Location: Common/Fixtures/PostgresFixture.cs
Base: IAsyncLifetime
Container: PostgreSqlContainer (Testcontainers)
Image: postgres:16-alpine
Respawn: schemas = ["identity", "core", "content"]
Exposes: ConnectionString, ResetAsync()
Collection: [CollectionDefinition("Database")]
```

Key behaviors:
- `InitializeAsync()` — start container, run EF migrations for all 3 DbContexts, create Respawner
- `ResetAsync()` — truncate all tables via Respawn
- `DisposeAsync()` — stop container

## ApiFixture

Reference: `03-webapplicationfactory.md`

```
Location: Common/Fixtures/ApiFixture.cs
Base: WebApplicationFactory<Program>
Dependencies: PostgresFixture
```

Key behaviors:
- `SetEnvironmentVariables()` — JWT, Postgres, Cloudinary, external service env vars
- `ReplaceDbContext<T>()` — swap all 3 DbContexts to Testcontainers
- `StubExternalServices()` — replace ICloudinaryService, IYoutubeThumbnailService

## BaseApiTest

Reference: `04-base-test-classes.md`

```
Location: Common/Base/BaseApiTest.cs
Constructor: (PostgresFixture db)
Provides: Client (HttpClient), Api (ApiFixture), Db (PostgresFixture)
Methods: CreateDbContext<TDbContext>(), SeedAsync() (virtual), ResetAsync()
Lifecycle: IAsyncLifetime — ResetAsync() + SeedAsync() in InitializeAsync()
```

## BaseRepositoryTest

Reference: `04-base-test-classes.md`

```
Location: Common/Base/BaseRepositoryTest.cs
Constructor: (PostgresFixture postgres)
Provides: CreateDbContext<TDbContext>()
Lifecycle: IAsyncLifetime — ResetAsync() in InitializeAsync()
```

## HttpClientExtensions

Reference: `09-authentication-testing.md`

```
Location: Common/Extensions/HttpClientExtensions.cs
Methods:
  - AuthenticateAsSuperAdmin() — void, synchronous
  - AuthenticateAsAdmin() — void, synchronous
  - AuthenticateAsVisitor() — void, synchronous
  - AuthenticateAs(Guid userId, string role) — void, synchronous
  - ClearAuthentication() — void, synchronous
  - AuthenticateViaLoginAsync() — async, hits real endpoint
  - GenerateToken() — private, mints JWT in-memory
```

## TestDataSeeder

Reference: `08-test-data-seeding.md`

```
Location: Common/Seeders/TestDataSeeder.cs
Methods:
  - SeedAuthenticationDataAsync() — seeds SuperAdmin + Visitor role
  - SeedContentTypesAsync() — seeds Article, Video, ShortVideo, Lyrics
  - SeedAllAsync() — both
```

## Stub Services

Reference: `07-external-service-stubs.md`

```
StubCloudinaryService — implements ICloudinaryService, returns fake URLs
StubYoutubeThumbnailService — implements IYoutubeThumbnailService, returns fake thumbnails
```

## Acceptance Criteria

1. A trivial test using `BaseApiTest` can send a GET request and receive a response
2. A trivial test using `BaseRepositoryTest` can insert and query an entity
3. `ResetAsync()` clears all data between tests
4. Auth helpers produce valid JWTs accepted by the test server
