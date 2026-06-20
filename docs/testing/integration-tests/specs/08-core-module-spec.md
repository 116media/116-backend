# Phase 6: Core Module Tests Spec

## Tasks

### FileRepository
- [ ] `FileRepositoryTests.cs`
  - [ ] CreateAsync_ValidFile_ShouldPersist
  - [ ] GetByIdOrThrowAsync_Existing_ShouldReturn
  - [ ] GetByIdOrThrowAsync_NonExistent_ShouldThrow
  - [ ] GetByPublicIdAsync_ShouldReturnCorrectFile
  - [ ] DeleteAsync_ShouldRemoveFromDatabase
  - [ ] UpdateAsync_ShouldUpdateFields

### CloudinaryService (stubbed in integration)
- [ ] `CloudinaryServiceIntegrationTests.cs`
  - [ ] UploadAsync_ShouldCallStubAndReturnFakeUrl
  - [ ] DeleteAsync_ShouldCallStubWithoutError

### FileService
- [ ] `FileServiceIntegrationTests.cs`
  - [ ] UploadAsync_ShouldPersistFileRecordAndCallCloudinary
  - [ ] DeleteAsync_ShouldRemoveRecordAndCallCloudinary
  - [ ] GetByIdAsync_ShouldReturnFileWithUrl

## Test Approach

Core module is small (1 entity, 1 repository, 2 services). Tests verify that:
- FileRepository correctly persists to the `core` schema
- FileService orchestrates between CloudinaryService and FileRepository
- StubCloudinaryService returns predictable values

```csharp
[Collection("Database")]
public class FileRepositoryTests(PostgresFixture postgres) : BaseRepositoryTest(postgres)
{
    [Fact]
    public async Task CreateAsync_ValidFile_ShouldPersist()
    {
        await using var context = CreateDbContext<CoreDbContext>();
        var file = FileEntity.Create(/* ... */);
        context.Files.Add(file);
        await context.SaveChangesAsync();

        await using var verifyContext = CreateDbContext<CoreDbContext>();
        var saved = await verifyContext.Files.FindAsync(file.Id);
        saved.Should().NotBeNull();
        saved!.PublicId.Should().Be(file.PublicId);
    }
}
```

## File Locations

```
tests/_116.Integration.Tests/Core/
├── Repositories/
│   └── FileRepositoryTests.cs
└── Services/
    ├── CloudinaryServiceIntegrationTests.cs
    └── FileServiceIntegrationTests.cs
```

## Acceptance Criteria

1. FileRepository CRUD operations verified against real PostgreSQL
2. FileService integration with stub Cloudinary verified
3. `./scripts/run-tests-with-coverage.sh integration` passes
