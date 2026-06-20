# Phase 5: Identity Module — Services Tests Spec

## Tasks

### JwtService
- [ ] `JwtServiceTests.cs`
  - [ ] GenerateAccessToken_WithValidClaims_ShouldReturnValidJwt
  - [ ] GenerateAccessToken_ShouldContainExpectedClaims
  - [ ] GenerateAccessToken_ShouldExpireAtConfiguredTime
  - [ ] ValidateToken_WithValidToken_ShouldReturnPrincipal
  - [ ] ValidateToken_WithExpiredToken_ShouldThrow
  - [ ] ValidateToken_WithWrongSecret_ShouldThrow

### OtpService
- [ ] `OtpServiceTests.cs`
  - [ ] GenerateAsync_ShouldCreateAndPersistOtp
  - [ ] VerifyAsync_WithValidOtp_ShouldReturnTrue
  - [ ] VerifyAsync_WithExpiredOtp_ShouldReturnFalse
  - [ ] VerifyAsync_WithUsedOtp_ShouldReturnFalse
  - [ ] IncrementAttemptAsync_ShouldUpdateAttemptCount
  - [ ] IsAttemptLimitReached_ShouldReturnCorrectResult

### PasswordService
- [ ] `PasswordServiceTests.cs`
  - [ ] Hash_ShouldReturnNonEmptyHash
  - [ ] Verify_WithCorrectPassword_ShouldReturnTrue
  - [ ] Verify_WithWrongPassword_ShouldReturnFalse
  - [ ] Hash_SamePassword_ShouldProduceDifferentHashes (salt)

### RefreshTokenService
- [ ] `RefreshTokenServiceTests.cs`
  - [ ] GenerateAsync_ShouldReturnTokenString
  - [ ] ValidateAsync_WithValidToken_ShouldReturnTrue
  - [ ] ValidateAsync_WithExpiredToken_ShouldReturnFalse
  - [ ] RevokeAsync_ShouldInvalidateToken

### SessionExportService
- [ ] `SessionExportServiceTests.cs`
  - [ ] ExportAsCsv_WithSessions_ShouldReturnCsvBytes
  - [ ] ExportAsXlsx_WithSessions_ShouldReturnXlsxBytes
  - [ ] Export_WithNoSessions_ShouldReturnEmptyFile

### SessionMetadataService
- [ ] `SessionMetadataServiceTests.cs`
  - [ ] ExtractMetadata_WithValidRequest_ShouldReturnMetadata
  - [ ] ExtractMetadata_ShouldParseUserAgent
  - [ ] ExtractMetadata_ShouldExtractIpAddress

### TokenDeliveryService
- [ ] `TokenDeliveryServiceTests.cs`
  - [ ] Deliver_ShouldCallEmailService (or equivalent)

### UserLookupService
- [ ] `UserLookupServiceTests.cs`
  - [ ] GetByIdAsync_ExistingUser_ShouldReturn
  - [ ] GetByIdAsync_NonExistent_ShouldThrow
  - [ ] GetByEmailAsync_ExistingUser_ShouldReturn

## Test Approach

Services that need real DB: resolve from `ApiFixture.Services` via `IServiceScope`.
Services that are pure logic (Password, JWT): can instantiate directly with test config.

```csharp
[Collection("Database")]
public class JwtServiceTests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public void GenerateAccessToken_ShouldContainExpectedClaims()
    {
        using var scope = Api.Services.CreateScope();
        var jwtService = scope.ServiceProvider.GetRequiredService<IJwtService>();

        var token = jwtService.GenerateAccessToken(userId, email, role);
        token.Split('.').Should().HaveCount(3);
    }
}
```

## File Locations

```
tests/_116.Integration.Tests/Identity/Services/
├── JwtServiceTests.cs
├── OtpServiceTests.cs
├── PasswordServiceTests.cs
├── RefreshTokenServiceTests.cs
├── SessionExportServiceTests.cs
├── SessionMetadataServiceTests.cs
├── TokenDeliveryServiceTests.cs
└── UserLookupServiceTests.cs
```

## Acceptance Criteria

1. Every public method on each service has at least one test
2. Services that interact with the DB verify real persistence
3. JWT tests validate actual token structure and claims
4. `./scripts/run-tests-with-coverage.sh integration` passes
