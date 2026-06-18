# Mocks: Services, Infrastructure, and Factory Interfaces

---

## Service Mocks

### `MockJwtService`
**File:** `tests/Unit/Common/Mocks/Services/MockJwtService.cs`
**Mocks:** `IJwtService`

```csharp
var mock = MockJwtService.Create();

// Setup
mock.SetupGenerateToken(JwtGenerationResult result)
mock.SetupGenerateTokenWithValue(string token, DateTime? expiresAt = null)

// Verify
mock.VerifyGenerateTokenCalled()
mock.VerifyGenerateTokenCalledWithUserId(Guid userId)   // Checks specific userId in call
mock.VerifyGenerateTokenNotCalled()
```

**Defaults:** `GenerateToken` returns `new JwtGenerationResult(TestConstants.Jwt.ValidAccessToken, DateTime.UtcNow.AddMinutes(60))`

---

### `MockPasswordService`
**File:** `tests/Unit/Common/Mocks/Services/MockPasswordService.cs`
**Mocks:** `IPasswordService`

```csharp
var mock = MockPasswordService.Create();

// Setup
mock.SetupHash(string password, string hash)          // Specific password → specific hash
mock.SetupHashReturns(string hash)                    // Any password → hash
mock.SetupVerifySuccess(string password, string hash) // Specific pair → true
mock.SetupVerifyFailure(string password, string hash) // Specific pair → false
mock.SetupVerifyReturnsTrue()                         // Any pair → true
mock.SetupVerifyReturnsFalse()                        // Any pair → false

// Verify
mock.VerifyHashCalled(string password)                // Specific password
mock.VerifyHashCalled()                               // Any password
mock.VerifyVerifyCalled()
mock.VerifyVerifyNotCalled()
```

**Defaults:** `Hash` → `TestConstants.User.DefaultPasswordHash`; `Verify` → `true`

---

### `MockOtpService`
**File:** `tests/Unit/Common/Mocks/Services/MockOtpService.cs`
**Mocks:** `IOtpService`

```csharp
var mock = MockOtpService.Create();

// Setup
mock.SetupGenerateOtpCode(string code)
mock.SetupCreateOtp(OtpEntity otp)                          // Specific userId/purpose
mock.SetupCreateOtpReturns(OtpEntity otp)                   // Any userId/purpose
mock.SetupCalculateExpirationTime(DateTime expirationTime)

// Verify
mock.VerifyGenerateOtpCodeCalled()
mock.VerifyCreateOtpCalled(Guid userId, EnumOtpPurpose purpose)
mock.VerifyCreateOtpCalled()
```

**Defaults:** `GenerateOtpCode` → `TestConstants.Otp.DefaultCode`; `CalculateExpirationTime` → `DateTime.UtcNow.AddMinutes(TestConstants.Otp.ExpirationMinutes)`

---

### `MockRefreshTokenService`
**File:** `tests/Unit/Common/Mocks/Services/MockRefreshTokenService.cs`
**Mocks:** `IRefreshTokenService`

```csharp
var mock = MockRefreshTokenService.Create();

// Setup
mock.SetupGenerateRefreshToken(string token)
mock.SetupHashRefreshToken(string token, string hash)  // Specific token → hash
mock.SetupHashRefreshTokenReturns(string hash)         // Any token → hash

// Verify
mock.VerifyGenerateRefreshTokenCalled()
mock.VerifyGenerateRefreshTokenNotCalled()
mock.VerifyHashRefreshTokenCalled(string token)
mock.VerifyHashRefreshTokenCalled()
```

---

### `MockSessionMetadataService`
**File:** `tests/Unit/Common/Mocks/Services/MockSessionMetadataService.cs`
**Mocks:** `ISessionMetadataService`

```csharp
var mock = MockSessionMetadataService.Create();

// Setup
mock.SetupExtractIpAddress(string? ipAddress)
mock.SetupExtractUserAgent(string? userAgent)
mock.SetupGetClientOriginInfo(ClientOriginInfo info)
mock.SetupGetClientOriginInfo(EnumBrowser browser, EnumDevice device, EnumPlatform platform)
mock.SetupExtractClientApp(EnumClient clientApp)
mock.SetupExtractDeviceId(string? deviceId)

// All-in-one setup
mock.SetupAllMetadata(
    string? ipAddress,
    string? userAgent,
    EnumBrowser browser,
    EnumDevice device,
    EnumPlatform platform,
    EnumClient clientApp,
    string? deviceId
)

// Verify
mock.VerifyExtractIpAddressCalled()
mock.VerifyGetClientOriginInfoCalled()
mock.VerifyExtractDeviceIdCalled()
```

**Defaults:** All methods return `TestConstants.Session` default values.

---

### `MockCloudinaryService`
**File:** `tests/Unit/Common/Mocks/Services/MockCloudinaryService.cs`
**Mocks:** `ICloudinaryService`

```csharp
var mock = MockCloudinaryService.Create();

// Setup
mock.SetupUploadImage(CloudinaryUploadResult? result = null)  // null = use defaults
static CloudinaryUploadResult MockCloudinaryService.DefaultUploadResult()  // static helper

// Verify
mock.VerifyUploadCalled()
mock.VerifyDeleteImageCalled(string storageKey)
mock.VerifyDeleteImageNotCalled()
mock.VerifyDeleteImagesCalled()      // bulk delete
mock.VerifyDeleteImagesNotCalled()
```

**Defaults:** `UploadImageAsync` → `DefaultUploadResult()` with `TestConstants.File` values; `DeleteImageAsync`/`DeleteImagesAsync` → `true`

---

### `MockYoutubeThumbnailService`
**File:** `tests/Unit/Common/Mocks/Services/MockYoutubeThumbnailService.cs`
**Mocks:** `IYoutubeThumbnailService`

```csharp
var mock = MockYoutubeThumbnailService.Create();

// Setup
mock.SetupDownload(IFormFile? formFile = null)   // null = use default mock form file
static IFormFile MockYoutubeThumbnailService.CreateMockFormFile()  // creates mock IFormFile

// Verify
mock.VerifyDownloadCalled(string youtubeVideoId)
mock.VerifyDownloadNotCalled()
```

---

## Infrastructure Mocks

### `MockIdentityUnitOfWork`
**File:** `tests/Unit/Common/Mocks/Infrastructure/MockIdentityUnitOfWork.cs`
**Mocks:** `IIdentityUnitOfWork`

```csharp
var mock = MockIdentityUnitOfWork.Create();

// Setup
mock.SetupCommit(int result = 1)
mock.SetupCommitThrows(Exception exception)

// Verify
mock.VerifyCommitCalled()
mock.VerifyCommitNotCalled()
mock.VerifyCommitCalled(int times)          // Times.Exactly(times)
```

**Defaults:** `CommitAsync` → `ReturnsAsync(1)`

---

### `MockContentUnitOfWork`
**File:** `tests/Unit/Common/Mocks/Infrastructure/MockContentUnitOfWork.cs`
**Mocks:** `IContentUnitOfWork`

Identical API to `MockIdentityUnitOfWork`:
```csharp
var mock = MockContentUnitOfWork.Create();
mock.SetupCommit(int result = 1)
mock.SetupCommitThrows(Exception exception)
mock.VerifyCommitCalled()
mock.VerifyCommitNotCalled()
mock.VerifyCommitCalled(int times)
```

---

### `MockCoreUnitOfWork`
**File:** `tests/Unit/Common/Mocks/Infrastructure/MockCoreUnitOfWork.cs`
**Mocks:** `ICoreUnitOfWork`

Identical API to `MockIdentityUnitOfWork`.

---

### `MockDispatcher`
**File:** `tests/Unit/Common/Mocks/Infrastructure/MockDispatcher.cs`
**Mocks:** `IDispatcher`

```csharp
var mock = MockDispatcher.Create();

// Setup — commands/queries with return value
mock.SetupSend<TRequest, TResponse>(TResponse response)
mock.SetupSendForRequest<TRequest, TResponse>(TRequest request, TResponse response)  // Specific request instance
mock.SetupSendThrows<TRequest, TResponse>(Exception exception)

// Setup — void commands
mock.SetupSendVoid<TRequest>()
mock.SetupSendVoidThrows<TRequest>(Exception exception)

// Verify
mock.VerifySendCalled<TRequest, TResponse>(Func<TRequest, bool>? predicate = null)
mock.VerifySendVoidCalled<TRequest>(Func<TRequest, bool>? predicate = null)
mock.VerifySendNotCalled<TRequest, TResponse>()
```

---

## Factory Interface Mocks

These mock the factory interfaces that are injected into handlers.

### `MockAddItemTierFactory`
**File:** `tests/Unit/Common/Mocks/Factories/MockAddItemTierFactory.cs`
**Mocks:** `IAddItemTierFactory`

```csharp
var mock = MockAddItemTierFactory.Create();

mock.SetupAttachTierAsync((ContentItemTierEntity tier, string tierName) result)
mock.SetupAttachTierAsyncThrows(Exception exception)

mock.VerifyAttachTierCalled()
```

---

### `MockAddOrderItemFactory`
**File:** `tests/Unit/Common/Mocks/Factories/MockAddOrderItemFactory.cs`
**Mocks:** `IAddOrderItemFactory`

```csharp
var mock = MockAddOrderItemFactory.Create();

mock.SetupCreateItemAsync((ContentOrderItemEntity item, string categoryName, string? promoName) result)
mock.SetupCreateItemAsyncThrows(Exception exception)

mock.VerifyCreateItemCalled()
```

---

### `MockSubmitOrderFactory`
**File:** `tests/Unit/Common/Mocks/Factories/MockSubmitOrderFactory.cs`
**Mocks:** `ISubmitOrderFactory`

```csharp
var mock = MockSubmitOrderFactory.Create();

mock.SetupSubmitAsync()                        // Task.CompletedTask
mock.SetupSubmitAsyncThrows(Exception exception)

mock.VerifySubmitCalled()
```

---

### `MockVerifyPaymentFactory`
**File:** `tests/Unit/Common/Mocks/Factories/MockVerifyPaymentFactory.cs`
**Mocks:** `IVerifyPaymentFactory`

```csharp
var mock = MockVerifyPaymentFactory.Create();

mock.SetupVerifyAsync()                        // Task.CompletedTask
mock.SetupVerifyAsyncThrows(Exception exception)

mock.VerifyVerifyCalled()
```

---

### `MockOrderPaymentFactory`
**File:** `tests/Unit/Common/Mocks/Factories/MockOrderPaymentFactory.cs`
**Mocks:** `IOrderPaymentFactory`

```csharp
var mock = MockOrderPaymentFactory.Create();

mock.SetupGetByOrderId(Guid orderId, ContentPaymentEntity payment)
mock.SetupGetByOrderIdNotFound(Guid orderId)   // ThrowsAsync(NotFoundException)

mock.VerifyGetByOrderIdCalled(Guid orderId)
```
