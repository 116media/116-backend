# Integration Tests — Folder Structure Convention

This document defines the folder structure convention for the integration test project.
The integration tests must mirror two existing conventions:

1. The **unit test project** (`tests/Unit/`) folder hierarchy
2. The **source code** (`src/`) module and vertical-slice layout

By following both, any developer can locate the integration test for a given source file
by applying the same path transformation used for unit tests.

---

## Guiding Principles

- **Mirror `src/`** — every integration test file lives at the same relative path as the
  source file it exercises, rooted under `tests/Integration/` instead of `src/`.
- **Match `tests/Unit/`** — the folder depth, naming, and grouping conventions are
  identical between unit and integration tests. The only difference is the project root.
- **One use case per folder** — endpoint tests live inside the use case's version folder
  (e.g., `V1/`), not in a flat `Api/` bucket.
- **Infrastructure tests stay under `Infrastructure/`** — repository and service tests
  belong under `Infrastructure/Repositories/` and `Infrastructure/Services/`, not at the
  module root.
- **Shared test infra stays in `Common/`** — fixtures, base classes, stubs, seeders, and
  extensions that support tests but do not test production code live in `Common/`.

---

## Path Transformation Rule

Given a source file at:

```
src/Modules/{Module}/{Module}/Application/{Feature}/UseCases/{Scope}/Commands/{UseCase}/V1/{File}.cs
```

The integration test lives at:

```
tests/Integration/Modules/{Module}/Application/{Feature}/UseCases/{Scope}/Commands/{UseCase}/V1/{File}Tests.cs
```

For infrastructure code at:

```
src/Modules/{Module}/{Module}/Infrastructure/Repositories/{File}.cs
```

The integration test lives at:

```
tests/Integration/Modules/{Module}/Infrastructure/Repositories/{File}Tests.cs
```

For shared code at:

```
src/Shared/Shared/Application/Decorators/{File}.cs
src/Shared/Shared/Infrastructure/interceptors/{File}.cs
```

The integration tests live at:

```
tests/Integration/Shared/Application/Decorators/{File}Tests.cs
tests/Integration/Shared/Infrastructure/Interceptors/{File}Tests.cs
```

---

## Full Directory Tree

```
tests/Integration/
│
├── Common/                                    # Test infrastructure (not mirroring src/)
│   ├── Base/
│   │   ├── BaseApiTest.cs
│   │   └── BaseRepositoryTest.cs
│   ├── Constants/
│   ├── Extensions/
│   │   └── HttpClientExtensions.cs
│   ├── Fixtures/
│   │   ├── ApiFixture.cs
│   │   └── PostgresFixture.cs
│   ├── Seeders/
│   └── Stubs/
│       ├── StubCloudinaryService.cs
│       └── StubYoutubeThumbnailService.cs
│
├── Shared/                                    # Mirrors src/Shared/Shared/
│   ├── Application/
│   │   └── Decorators/
│   │       ├── LoggingDecoratorTests.cs
│   │       └── ValidationDecoratorTests.cs
│   ├── Exceptions/
│   │   └── Handlers/
│   │       └── ExceptionHandlerTests.cs
│   ├── Infrastructure/
│   │   ├── Interceptors/
│   │   │   ├── AuditableEntityInterceptorTests.cs
│   │   │   └── DispatchDomainEventsInterceptorTests.cs
│   │   └── Middleware/
│   │       └── ResourceNotFoundMiddlewareTests.cs
│   └── ...
│
├── Modules/                                   # Mirrors src/Modules/
│   │
│   ├── Identity/                              # Mirrors src/Modules/Identity/Identity/
│   │   ├── Application/
│   │   │   ├── Auth/
│   │   │   │   └── UseCases/
│   │   │   │       ├── Admin/
│   │   │   │       │   └── Commands/
│   │   │   │       │       ├── Login/
│   │   │   │       │       │   └── V1/
│   │   │   │       │       │       └── AdminLoginEndpointV1Tests.cs
│   │   │   │       │       ├── ForgotPassword/
│   │   │   │       │       │   └── V1/
│   │   │   │       │       │       └── AdminForgotPasswordEndpointV1Tests.cs
│   │   │   │       │       ├── ResetPassword/
│   │   │   │       │       │   └── V1/
│   │   │   │       │       │       └── AdminResetPasswordEndpointV1Tests.cs
│   │   │   │       │       ├── ChangePassword/
│   │   │   │       │       │   └── V1/
│   │   │   │       │       │       └── AdminChangePasswordEndpointV1Tests.cs
│   │   │   │       │       ├── SignOut/
│   │   │   │       │       │   └── V1/
│   │   │   │       │       │       └── AdminSignOutEndpointV1Tests.cs
│   │   │   │       │       ├── SignOutFromAllDevices/
│   │   │   │       │       │   └── V1/
│   │   │   │       │       │       └── AdminSignOutFromAllDevicesEndpointV1Tests.cs
│   │   │   │       │       ├── VerifyOtp/
│   │   │   │       │       │   └── V1/
│   │   │   │       │       │       └── AdminVerifyOtpEndpointV1Tests.cs
│   │   │   │       │       └── ResendOtp/
│   │   │   │       │           └── V1/
│   │   │   │       │               └── AdminResendOtpEndpointV1Tests.cs
│   │   │   │       └── Public/
│   │   │   │           └── Commands/
│   │   │   │               ├── Login/
│   │   │   │               │   └── V1/
│   │   │   │               │       └── PublicLoginEndpointV1Tests.cs
│   │   │   │               ├── SignUp/
│   │   │   │               │   └── V1/
│   │   │   │               │       └── PublicSignUpEndpointV1Tests.cs
│   │   │   │               ├── ForgotPassword/
│   │   │   │               │   └── V1/
│   │   │   │               │       └── PublicForgotPasswordEndpointV1Tests.cs
│   │   │   │               ├── ChangePassword/
│   │   │   │               │   └── V1/
│   │   │   │               │       └── PublicChangePasswordEndpointV1Tests.cs
│   │   │   │               ├── SetPassword/
│   │   │   │               │   └── V1/
│   │   │   │               │       └── PublicSetPasswordEndpointV1Tests.cs
│   │   │   │               ├── SignOut/
│   │   │   │               │   └── V1/
│   │   │   │               │       └── PublicSignOutEndpointV1Tests.cs
│   │   │   │               ├── SignOutFromAllDevices/
│   │   │   │               │   └── V1/
│   │   │   │               │       └── PublicSignOutFromAllDevicesEndpointV1Tests.cs
│   │   │   │               ├── SocialLogin/
│   │   │   │               │   └── V1/
│   │   │   │               │       └── PublicSocialLoginEndpointV1Tests.cs
│   │   │   │               ├── VerifyOtp/
│   │   │   │               │   └── V1/
│   │   │   │               │       └── PublicVerifyOtpEndpointV1Tests.cs
│   │   │   │               └── ResendOtp/
│   │   │   │                   └── V1/
│   │   │   │                       └── PublicResendOtpEndpointV1Tests.cs
│   │   │   │
│   │   │   ├── Roles/
│   │   │   │   └── UseCases/
│   │   │   │       ├── Admin/
│   │   │   │       │   ├── Commands/
│   │   │   │       │   │   ├── CreateRole/V1/
│   │   │   │       │   │   ├── UpdateRole/V1/
│   │   │   │       │   │   ├── ActivateRole/V1/
│   │   │   │       │   │   ├── DeactivateRole/V1/
│   │   │   │       │   │   ├── SoftDeleteRole/V1/
│   │   │   │       │   │   ├── HardDeleteRole/V1/
│   │   │   │       │   │   ├── RestoreRole/V1/
│   │   │   │       │   │   ├── CreatePermission/V1/
│   │   │   │       │   │   ├── UpdatePermission/V1/
│   │   │   │       │   │   ├── ActivatePermission/V1/
│   │   │   │       │   │   ├── DeactivatePermission/V1/
│   │   │   │       │   │   ├── SoftDeletePermission/V1/
│   │   │   │       │   │   ├── HardDeletePermission/V1/
│   │   │   │       │   │   ├── RestorePermission/V1/
│   │   │   │       │   │   ├── AssignPermissionToRole/V1/
│   │   │   │       │   │   ├── RemovePermissionFromRole/V1/
│   │   │   │       │   │   └── BulkUpdateRolePermissions/V1/
│   │   │   │       │   └── Queries/
│   │   │   │       │       ├── GetAllRoles/V1/
│   │   │   │       │       ├── GetRoleById/V1/
│   │   │   │       │       ├── GetOwnRoles/V1/
│   │   │   │       │       ├── GetAllPermissions/V1/
│   │   │   │       │       └── GetPermissionById/V1/
│   │   │   │       └── Public/
│   │   │   │           └── Queries/
│   │   │   │               └── GetOwnRoles/V1/
│   │   │   │
│   │   │   ├── Session/
│   │   │   │   └── UseCases/
│   │   │   │       ├── Admin/
│   │   │   │       │   ├── Commands/
│   │   │   │       │   │   ├── RefreshToken/V1/
│   │   │   │       │   │   ├── RevokeSession/V1/
│   │   │   │       │   │   ├── ForceLogoutUser/V1/
│   │   │   │       │   │   └── CleanupExpiredSessions/V1/
│   │   │   │       │   └── Queries/
│   │   │   │       │       ├── GetAllSessions/V1/
│   │   │   │       │       ├── GetOwnSessions/V1/
│   │   │   │       │       ├── GetOwnSessionById/V1/
│   │   │   │       │       ├── GetSessionMetrics/V1/
│   │   │   │       │       └── ExportSessionData/V1/
│   │   │   │       └── Public/
│   │   │   │           ├── Commands/
│   │   │   │           │   ├── RefreshToken/V1/
│   │   │   │           │   └── RevokeSession/V1/
│   │   │   │           └── Queries/
│   │   │   │               ├── GetOwnSessions/V1/
│   │   │   │               └── GetOwnSessionById/V1/
│   │   │   │
│   │   │   └── User/
│   │   │       └── UseCases/
│   │   │           ├── Admin/
│   │   │           │   ├── Commands/
│   │   │           │   │   ├── AssignRoleToUser/V1/
│   │   │           │   │   ├── RemoveRoleFromUser/V1/
│   │   │           │   │   ├── UpdateAvatar/V1/
│   │   │           │   │   └── UpdateOwnProfile/V1/
│   │   │           │   └── Queries/
│   │   │           │       ├── GetOwnProfile/V1/
│   │   │           │       └── GetUserRoles/V1/
│   │   │           └── Public/
│   │   │               ├── Commands/
│   │   │               │   ├── UpdateAvatar/V1/
│   │   │               │   └── UpdateOwnProfile/V1/
│   │   │               └── Queries/
│   │   │                   └── GetOwnProfile/V1/
│   │   │
│   │   └── Infrastructure/
│   │       ├── Repositories/
│   │       │   ├── AuthRepositoryTests.cs
│   │       │   ├── OtpRepositoryTests.cs
│   │       │   ├── PermissionRepositoryTests.cs
│   │       │   ├── RolePermissionRepositoryTests.cs
│   │       │   ├── RoleRepositoryTests.cs
│   │       │   ├── SessionRepositoryTests.cs
│   │       │   └── UserRoleRepositoryTests.cs
│   │       ├── Services/
│   │       │   ├── SessionExportServiceTests.cs
│   │       │   ├── SessionMetadataServiceTests.cs
│   │       │   ├── TokenDeliveryServiceTests.cs
│   │       │   └── UserLookupServiceTests.cs
│   │       └── Mappers/
│   │
│   ├── Core/                                  # Mirrors src/Modules/Core/Core/
│   │   └── Infrastructure/
│   │       ├── Repositories/
│   │       │   └── FileRepositoryTests.cs
│   │       └── Services/
│   │
│   └── Content/                               # Mirrors src/Modules/Content/Content/
│       ├── Application/
│       │   ├── Catalog/
│       │   │   └── UseCases/
│       │   │       ├── Admin/
│       │   │       │   ├── Commands/
│       │   │       │   │   ├── CreateCategory/V1/
│       │   │       │   │   ├── UpdateCategory/V1/
│       │   │       │   │   ├── ActivateCategory/V1/
│       │   │       │   │   ├── DeactivateCategory/V1/
│       │   │       │   │   ├── SetExclusiveCategory/V1/
│       │   │       │   │   ├── AddCategoryPricing/V1/
│       │   │       │   │   ├── UpdateCategoryPricing/V1/
│       │   │       │   │   ├── RemoveCategoryPricing/V1/
│       │   │       │   │   ├── UploadCategoryPoster/V1/
│       │   │       │   │   ├── CreateCustomer/V1/
│       │   │       │   │   ├── UpdateCustomer/V1/
│       │   │       │   │   ├── CreatePackage/V1/
│       │   │       │   │   ├── ActivatePackage/V1/
│       │   │       │   │   ├── DeactivatePackage/V1/
│       │   │       │   │   ├── AddPackageSlot/V1/
│       │   │       │   │   └── RemovePackageSlot/V1/
│       │   │       │   └── Queries/
│       │   │       │       ├── GetAllCategories/V1/
│       │   │       │       ├── GetCategoryById/V1/
│       │   │       │       ├── GetAllCustomers/V1/
│       │   │       │       ├── GetCustomerById/V1/
│       │   │       │       ├── GetAllPackages/V1/
│       │   │       │       └── GetPackageById/V1/
│       │   │       └── Public/
│       │   │           └── Queries/
│       │   │               ├── GetActiveCategories/V1/
│       │   │               └── GetExclusiveCategory/V1/
│       │   │
│       │   ├── Commerce/
│       │   │   └── UseCases/
│       │   │       └── Admin/
│       │   │           ├── Commands/
│       │   │           │   ├── CreateOrder/V1/
│       │   │           │   ├── EditOrder/V1/
│       │   │           │   ├── CancelOrder/V1/
│       │   │           │   ├── SubmitOrder/V1/
│       │   │           │   ├── AddOrderItem/V1/
│       │   │           │   ├── EditOrderItem/V1/
│       │   │           │   ├── RemoveOrderItem/V1/
│       │   │           │   ├── AddItemTier/V1/
│       │   │           │   ├── RemoveItemTier/V1/
│       │   │           │   ├── AttachPaymentProof/V1/
│       │   │           │   ├── VerifyPayment/V1/
│       │   │           │   └── RejectPayment/V1/
│       │   │           └── Queries/
│       │   │               ├── GetAllOrders/V1/
│       │   │               ├── GetOrderById/V1/
│       │   │               ├── GetCustomerOrders/V1/
│       │   │               ├── GetAllPayments/V1/
│       │   │               ├── GetOrderPayment/V1/
│       │   │               └── GetPendingPaymentOrders/V1/
│       │   │
│       │   ├── Editorial/
│       │   │   └── UseCases/...               # Same pattern as above
│       │   │
│       │   ├── Interactions/
│       │   │   └── UseCases/...               # Same pattern as above
│       │   │
│       │   └── Lookup/
│       │       └── UseCases/...               # Same pattern as above
│       │
│       └── Infrastructure/
│           ├── Repositories/
│           │   ├── ArticleRepositoryTests.cs
│           │   ├── CategoryRepositoryTests.cs
│           │   ├── ContentOrderRepositoryTests.cs
│           │   ├── CustomerRepositoryTests.cs
│           │   ├── LookupRepositoryTests.cs
│           │   ├── LyricsRepositoryTests.cs
│           │   ├── PackageRepositoryTests.cs
│           │   ├── PlaylistRepositoryTests.cs
│           │   ├── ShortVideoRepositoryTests.cs
│           │   └── VideoRepositoryTests.cs
│           ├── Mappers/
│           └── Seeds/
│
├── Workflows/
├── SmokeTest.cs
├── GlobalUsings.cs
└── _116.Integration.Tests.csproj
```

---

## Migration Map: Current Path to New Path

### Shared Tests

| Current | New |
|---------|-----|
| `Shared/Decorators/LoggingDecoratorTests.cs` | `Shared/Application/Decorators/LoggingDecoratorTests.cs` |
| `Shared/Decorators/ValidationDecoratorTests.cs` | `Shared/Application/Decorators/ValidationDecoratorTests.cs` |
| `Shared/ExceptionHandlers/ExceptionHandlerTests.cs` | `Shared/Exceptions/Handlers/ExceptionHandlerTests.cs` |
| `Shared/Interceptors/AuditableEntityInterceptorTests.cs` | `Shared/Infrastructure/Interceptors/AuditableEntityInterceptorTests.cs` |
| `Shared/Interceptors/DispatchDomainEventsInterceptorTests.cs` | `Shared/Infrastructure/Interceptors/DispatchDomainEventsInterceptorTests.cs` |
| `Shared/Middleware/ResourceNotFoundMiddlewareTests.cs` | `Shared/Infrastructure/Middleware/ResourceNotFoundMiddlewareTests.cs` |

### Identity — Infrastructure Tests

| Current | New |
|---------|-----|
| `Identity/Repositories/AuthRepositoryTests.cs` | `Modules/Identity/Infrastructure/Repositories/AuthRepositoryTests.cs` |
| `Identity/Repositories/OtpRepositoryTests.cs` | `Modules/Identity/Infrastructure/Repositories/OtpRepositoryTests.cs` |
| `Identity/Repositories/PermissionRepositoryTests.cs` | `Modules/Identity/Infrastructure/Repositories/PermissionRepositoryTests.cs` |
| `Identity/Repositories/RolePermissionRepositoryTests.cs` | `Modules/Identity/Infrastructure/Repositories/RolePermissionRepositoryTests.cs` |
| `Identity/Repositories/RoleRepositoryTests.cs` | `Modules/Identity/Infrastructure/Repositories/RoleRepositoryTests.cs` |
| `Identity/Repositories/SessionRepositoryTests.cs` | `Modules/Identity/Infrastructure/Repositories/SessionRepositoryTests.cs` |
| `Identity/Repositories/UserRoleRepositoryTests.cs` | `Modules/Identity/Infrastructure/Repositories/UserRoleRepositoryTests.cs` |
| `Identity/Services/SessionExportServiceTests.cs` | `Modules/Identity/Infrastructure/Services/SessionExportServiceTests.cs` |
| `Identity/Services/SessionMetadataServiceTests.cs` | `Modules/Identity/Infrastructure/Services/SessionMetadataServiceTests.cs` |
| `Identity/Services/TokenDeliveryServiceTests.cs` | `Modules/Identity/Infrastructure/Services/TokenDeliveryServiceTests.cs` |
| `Identity/Services/UserLookupServiceTests.cs` | `Modules/Identity/Infrastructure/Services/UserLookupServiceTests.cs` |

### Identity — Endpoint Tests (split from bundled files)

| Current | New (one file per use case) |
|---------|----------------------------|
| `Identity/Api/Auth/AdminAuthEndpointTests.cs` | `Modules/Identity/Application/Auth/UseCases/Admin/Commands/Login/V1/AdminLoginEndpointV1Tests.cs` |
| | `Modules/Identity/Application/Auth/UseCases/Admin/Commands/ForgotPassword/V1/AdminForgotPasswordEndpointV1Tests.cs` |
| | `Modules/Identity/Application/Auth/UseCases/Admin/Commands/ResetPassword/V1/AdminResetPasswordEndpointV1Tests.cs` |
| | `Modules/Identity/Application/Auth/UseCases/Admin/Commands/ChangePassword/V1/AdminChangePasswordEndpointV1Tests.cs` |
| | `Modules/Identity/Application/Auth/UseCases/Admin/Commands/SignOut/V1/AdminSignOutEndpointV1Tests.cs` |
| | `Modules/Identity/Application/Auth/UseCases/Admin/Commands/SignOutFromAllDevices/V1/AdminSignOutFromAllDevicesEndpointV1Tests.cs` |
| | `Modules/Identity/Application/Auth/UseCases/Admin/Commands/VerifyOtp/V1/AdminVerifyOtpEndpointV1Tests.cs` |
| | `Modules/Identity/Application/Auth/UseCases/Admin/Commands/ResendOtp/V1/AdminResendOtpEndpointV1Tests.cs` |
| `Identity/Api/Auth/PublicAuthEndpointTests.cs` | `Modules/Identity/Application/Auth/UseCases/Public/Commands/Login/V1/PublicLoginEndpointV1Tests.cs` |
| | `Modules/Identity/Application/Auth/UseCases/Public/Commands/SignUp/V1/PublicSignUpEndpointV1Tests.cs` |
| | `Modules/Identity/Application/Auth/UseCases/Public/Commands/ForgotPassword/V1/PublicForgotPasswordEndpointV1Tests.cs` |
| | `Modules/Identity/Application/Auth/UseCases/Public/Commands/ChangePassword/V1/PublicChangePasswordEndpointV1Tests.cs` |
| | `Modules/Identity/Application/Auth/UseCases/Public/Commands/SetPassword/V1/PublicSetPasswordEndpointV1Tests.cs` |
| | `Modules/Identity/Application/Auth/UseCases/Public/Commands/SignOut/V1/PublicSignOutEndpointV1Tests.cs` |
| | `Modules/Identity/Application/Auth/UseCases/Public/Commands/SignOutFromAllDevices/V1/PublicSignOutFromAllDevicesEndpointV1Tests.cs` |
| | `Modules/Identity/Application/Auth/UseCases/Public/Commands/SocialLogin/V1/PublicSocialLoginEndpointV1Tests.cs` |
| | `Modules/Identity/Application/Auth/UseCases/Public/Commands/VerifyOtp/V1/PublicVerifyOtpEndpointV1Tests.cs` |
| | `Modules/Identity/Application/Auth/UseCases/Public/Commands/ResendOtp/V1/PublicResendOtpEndpointV1Tests.cs` |
| `Identity/Api/Roles/AdminRoleCommandEndpointTests.cs` | Split into one file per use case under `Modules/Identity/Application/Roles/UseCases/Admin/Commands/{UseCase}/V1/` |
| `Identity/Api/Roles/AdminRoleQueryEndpointTests.cs` | Split into one file per use case under `Modules/Identity/Application/Roles/UseCases/Admin/Queries/{UseCase}/V1/` |
| `Identity/Api/Roles/AdminPermissionEndpointTests.cs` | Split into one file per use case under `Modules/Identity/Application/Roles/UseCases/Admin/Commands/{UseCase}/V1/` |
| `Identity/Api/Roles/AdminRolePermissionEndpointTests.cs` | Split into one file per use case under `Modules/Identity/Application/Roles/UseCases/Admin/Commands/{UseCase}/V1/` |
| `Identity/Api/Session/SessionEndpointTests.cs` | Split into one file per use case under `Modules/Identity/Application/Session/UseCases/{Scope}/{Type}/{UseCase}/V1/` |
| `Identity/Api/Users/UserEndpointTests.cs` | Split into one file per use case under `Modules/Identity/Application/User/UseCases/{Scope}/{Type}/{UseCase}/V1/` |

### Core — Infrastructure Tests

| Current | New |
|---------|-----|
| `Core/Repositories/FileRepositoryTests.cs` | `Modules/Core/Infrastructure/Repositories/FileRepositoryTests.cs` |

### Content — Infrastructure Tests

| Current | New |
|---------|-----|
| `Content/Repositories/ArticleRepositoryTests.cs` | `Modules/Content/Infrastructure/Repositories/ArticleRepositoryTests.cs` |
| `Content/Repositories/CategoryRepositoryTests.cs` | `Modules/Content/Infrastructure/Repositories/CategoryRepositoryTests.cs` |
| `Content/Repositories/ContentOrderRepositoryTests.cs` | `Modules/Content/Infrastructure/Repositories/ContentOrderRepositoryTests.cs` |
| `Content/Repositories/CustomerRepositoryTests.cs` | `Modules/Content/Infrastructure/Repositories/CustomerRepositoryTests.cs` |
| `Content/Repositories/LookupRepositoryTests.cs` | `Modules/Content/Infrastructure/Repositories/LookupRepositoryTests.cs` |
| `Content/Repositories/LyricsRepositoryTests.cs` | `Modules/Content/Infrastructure/Repositories/LyricsRepositoryTests.cs` |
| `Content/Repositories/PackageRepositoryTests.cs` | `Modules/Content/Infrastructure/Repositories/PackageRepositoryTests.cs` |
| `Content/Repositories/PlaylistRepositoryTests.cs` | `Modules/Content/Infrastructure/Repositories/PlaylistRepositoryTests.cs` |
| `Content/Repositories/ShortVideoRepositoryTests.cs` | `Modules/Content/Infrastructure/Repositories/ShortVideoRepositoryTests.cs` |
| `Content/Repositories/VideoRepositoryTests.cs` | `Modules/Content/Infrastructure/Repositories/VideoRepositoryTests.cs` |

### Content — Endpoint Tests (split from bundled files)

| Current | New (one file per use case) |
|---------|----------------------------|
| `Content/Api/Catalog/AdminCategoryCommandEndpointTests.cs` | Split into one file per use case under `Modules/Content/Application/Catalog/UseCases/Admin/Commands/{UseCase}/V1/` |
| `Content/Api/Catalog/CategoryQueryEndpointTests.cs` | Split into one file per use case under `Modules/Content/Application/Catalog/UseCases/{Scope}/Queries/{UseCase}/V1/` |
| `Content/Api/Catalog/AdminCustomerEndpointTests.cs` | Split into one file per use case under `Modules/Content/Application/Catalog/UseCases/Admin/{Type}/{UseCase}/V1/` |
| `Content/Api/Catalog/AdminPackageEndpointTests.cs` | Split into one file per use case under `Modules/Content/Application/Catalog/UseCases/Admin/{Type}/{UseCase}/V1/` |
| `Content/Api/Commerce/AdminOrderCommandEndpointTests.cs` | Split into one file per use case under `Modules/Content/Application/Commerce/UseCases/Admin/Commands/{UseCase}/V1/` |
| `Content/Api/Commerce/AdminOrderItemEndpointTests.cs` | Split into one file per use case under `Modules/Content/Application/Commerce/UseCases/Admin/Commands/{UseCase}/V1/` |
| `Content/Api/Commerce/AdminPaymentCommandEndpointTests.cs` | Split into one file per use case under `Modules/Content/Application/Commerce/UseCases/Admin/Commands/{UseCase}/V1/` |
| `Content/Api/Commerce/CommerceQueryEndpointTests.cs` | Split into one file per use case under `Modules/Content/Application/Commerce/UseCases/Admin/Queries/{UseCase}/V1/` |

---

## Namespace Convention

Namespaces must match the folder path exactly, rooted at `_116.Integration.Tests`:

```csharp
// File: tests/Integration/Modules/Identity/Infrastructure/Repositories/RoleRepositoryTests.cs
namespace _116.Integration.Tests.Modules.Identity.Infrastructure.Repositories;

// File: tests/Integration/Modules/Identity/Application/Auth/UseCases/Admin/Commands/Login/V1/AdminLoginEndpointV1Tests.cs
namespace _116.Integration.Tests.Modules.Identity.Application.Auth.UseCases.Admin.Commands.Login.V1;

// File: tests/Integration/Shared/Application/Decorators/LoggingDecoratorTests.cs
namespace _116.Integration.Tests.Shared.Application.Decorators;
```

---

## Comparison: Three-Way Alignment

This table shows how a single feature maps across all three trees:

| Layer | `src/` | `tests/Unit/` | `tests/Integration/` |
|-------|--------|---------------|----------------------|
| Endpoint | `src/Modules/Identity/Identity/Application/Auth/UseCases/Admin/Commands/Login/V1/AdminLoginEndpointV1.cs` | `tests/Unit/Modules/Identity/Application/Auth/UseCases/Admin/Commands/Login/V1/AdminLoginEndpointV1Tests.cs` | `tests/Integration/Modules/Identity/Application/Auth/UseCases/Admin/Commands/Login/V1/AdminLoginEndpointV1Tests.cs` |
| Repository | `src/Modules/Identity/Identity/Infrastructure/Repositories/AuthRepository.cs` | `tests/Unit/Modules/Identity/Infrastructure/Repositories/AuthRepositoryTests.cs` | `tests/Integration/Modules/Identity/Infrastructure/Repositories/AuthRepositoryTests.cs` |
| Decorator | `src/Shared/Shared/Application/Decorators/LoggingDecorator.cs` | `tests/Unit/Shared/Application/Decorators/LoggingDecoratorTests.cs` | `tests/Integration/Shared/Application/Decorators/LoggingDecoratorTests.cs` |
| Interceptor | `src/Shared/Shared/Infrastructure/interceptors/AuditableEntityInterceptor.cs` | `tests/Unit/Shared/Infrastructure/Interceptors/AuditableEntityInterceptorTests.cs` | `tests/Integration/Shared/Infrastructure/Interceptors/AuditableEntityInterceptorTests.cs` |

---

## What Changes During Migration

1. **Moves without code changes** — repository, service, and infrastructure tests move into
   `Modules/{Module}/Infrastructure/` and update their namespace. No test logic changes.

2. **Splits with code changes** — bundled endpoint test files (e.g., `AdminAuthEndpointTests.cs`
   with 8 use cases) are split into one file per use case. Each new file gets its own class,
   namespace, and the subset of tests from the original file that belong to that use case.

3. **New `.gitkeep` files** — empty directories that represent future test locations get a
   `.gitkeep` so the structure is visible in git.

4. **Old directories removed** — the flat `Api/`, `Repositories/`, `Services/` directories
   at the module root are deleted after all files are moved.

5. **Old `.gitkeep` files removed** — `.gitkeep` files in the old flat directories are deleted.

---

## Rules for New Tests

When adding a new integration test, follow this checklist:

1. Identify the source file being tested
2. Apply the path transformation rule to determine the test file location
3. Create the folder structure if it does not exist
4. Name the test class `{SourceClassName}Tests`
5. Set the namespace to match the folder path
6. For endpoint tests: one test class per use case, in the `V1/` folder
7. For repository tests: one test class per repository, in `Infrastructure/Repositories/`
8. For service tests: one test class per service, in `Infrastructure/Services/`
