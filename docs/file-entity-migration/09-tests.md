# Phase 6 — Test Plan

All tests follow the existing conventions documented in `docs/how-to-tests/`.

---

## Test Infrastructure Updates

### 1. Update `FileBuilder`

**File:** `tests/Fixtures/Builders/Entities/FileBuilder.cs`

Add support for `StorageKey`:

```csharp
private string? _storageKey;

public FileBuilder WithStorageKey(string storageKey)
{
    _storageKey = storageKey;
    return this;
}
```

Update `Build()` to pass `storageKey` to `FileEntity.Create()`.

### 2. Update `FileFactory`

**File:** `tests/Fixtures/Factories/FileFactory.cs`

Add new factory methods:

```csharp
FileFactory.CreateWithStorageKey(string storageKey)
FileFactory.CreateImage()                              // image/jpeg, with storage key
FileFactory.CreateVideo()                              // video/mp4, with storage key
```

### 3. Update `MockFileRepository`

**File:** `tests/Unit/Common/Mocks/Repositories/MockFileRepository.cs`

Add setup and verify methods for the new `IFileRepository` methods:

```csharp
// Setup
mock.SetupUploadAndStoreImageFile(FileEntity file)
mock.SetupUploadAndStoreVideoFile(FileEntity file)
mock.SetupReplaceImageFile(FileEntity file)
mock.SetupSoftDeleteById(bool result = true)
mock.SetupSoftDeleteByIdReturnsFalse()

// Verify
mock.VerifyUploadAndStoreImageFileCalled()
mock.VerifyUploadAndStoreVideoFileCalled()
mock.VerifyReplaceImageFileCalled()
mock.VerifySoftDeleteByIdCalled(Guid fileId)
mock.VerifySoftDeleteByIdCalled()                // any ID
mock.VerifySoftDeleteByIdNotCalled()
```

### 4. Update `ShortVideoBuilder`

**File:** `tests/Fixtures/Builders/Entities/Content/ShortVideoBuilder.cs`

Add:

```csharp
private Guid _videoFileId = Guid.NewGuid();
private Guid? _thumbnailFileId;

public ShortVideoBuilder WithVideoFileId(Guid fileId) { _videoFileId = fileId; return this; }
public ShortVideoBuilder WithThumbnailFileId(Guid fileId) { _thumbnailFileId = fileId; return this; }
```

Update `Build()` to pass `videoFileId` to `CreateStandalone()` / `CreateTeaser()`.

### 5. Update `ShortVideoFactory`

**File:** `tests/Fixtures/Factories/Content/ShortVideoFactory.cs`

Update all factory methods to pass a `videoFileId` to the builder. Add:

```csharp
ShortVideoFactory.CreateWithVideoFileId(Guid videoFileId)
ShortVideoFactory.CreateWithThumbnailFileId(Guid thumbnailFileId)
```

### 6. Update `ArticleBuilder`

**File:** `tests/Fixtures/Builders/Entities/Content/ArticleBuilder.cs`

Add:

```csharp
private Guid? _coverImageFileId;

public ArticleBuilder WithCoverImageFileId(Guid fileId) { _coverImageFileId = fileId; return this; }
```

After `Build()`, call `article.UpdateCoverImage(_coverImageFileId, _coverImageUrl)` if set.

### 7. Update `VideoBuilder`

**File:** `tests/Fixtures/Builders/Entities/Content/VideoBuilder.cs`

Add:

```csharp
private Guid? _thumbnailFileId;

public VideoBuilder WithThumbnailFileId(Guid fileId) { _thumbnailFileId = fileId; return this; }
```

### 8. Update `TestConstants`

**File:** `tests/Fixtures/Constants/TestConstants.cs`

Add to `TestConstants.File`:

```csharp
public const string ValidStorageKey = "content/test-images/test-image-id";
public const string ValidVideoStorageKey = "content/short-videos/test-video-id";
```

Add to `TestConstants.Content.Editorial.Cloudinary`:

```csharp
public static readonly CloudinaryUploadResult DefaultVideoUploadResult = new(
    PublicId: "content/short-videos/test-id",
    SecureUrl: "https://res.cloudinary.com/test/video/upload/test.mp4",
    Format: "mp4",
    Width: 1080,
    Height: 1920,
    Bytes: 5_000_000,
    ResourceType: "video"
);
```

---

## Domain Entity Tests

### `FileEntityTests` (update existing)

**File:** `tests/Unit/Modules/Core/Domain/FileEntityTests.cs`

Add tests:

```csharp
[Fact]
public void Create_WithStorageKey_ShouldSetStorageKey()
{
    FileEntity file = FileEntity.Create(
        id: Guid.NewGuid(),
        fileName: "test",
        originalFileName: "test.jpg",
        mimeType: "image/jpeg",
        storageUrl: TestConstants.File.ValidStorageUrl,
        sizeInBytes: 1000,
        i18n: _i18n,
        storageKey: TestConstants.File.ValidStorageKey
    );

    file.StorageKey.Should().Be(TestConstants.File.ValidStorageKey);
}

[Fact]
public void Create_WithoutStorageKey_ShouldHaveNullStorageKey()
{
    FileEntity file = FileFactory.Create();
    file.StorageKey.Should().BeNull();
}

[Fact]
public void UpdateStorageKey_ShouldSetStorageKey()
{
    FileEntity file = FileFactory.Create();
    file.UpdateStorageKey("new-storage-key");
    file.StorageKey.Should().Be("new-storage-key");
}
```

### `ArticleEntityTests` (update existing)

**File:** `tests/Unit/Modules/Content/Domain/Entities/ArticleEntityTests.cs`

Add tests:

```csharp
[Fact]
public void UpdateCoverImage_ShouldSetFileIdAndUrl()
{
    ArticleEntity article = ArticleFactory.Create(CategoryId);
    Guid fileId = Guid.NewGuid();
    const string url = "https://res.cloudinary.com/test/image/upload/cover.jpg";

    article.UpdateCoverImage(coverImageFileId: fileId, coverImageUrl: url);

    article.CoverImageFileId.Should().Be(fileId);
    article.CoverImageUrl.Should().Be(url);
}

[Fact]
public void UpdateCoverImage_WithNull_ShouldClearBoth()
{
    ArticleEntity article = ArticleFactory.Create(CategoryId);
    article.UpdateCoverImage(coverImageFileId: Guid.NewGuid(), coverImageUrl: "https://example.com/img.jpg");

    article.UpdateCoverImage(coverImageFileId: null, coverImageUrl: null);

    article.CoverImageFileId.Should().BeNull();
    article.CoverImageUrl.Should().BeNull();
}
```

### `VideoEntityTests` (update existing)

Add tests:

```csharp
[Fact]
public void UpdateThumbnail_WithFileId_ShouldSetFileIdAndUrl()
{
    VideoEntity video = VideoFactory.Create(CategoryId);
    Guid fileId = Guid.NewGuid();
    const string url = "https://res.cloudinary.com/test/image/upload/thumb.jpg";

    video.UpdateThumbnail(thumbnailFileId: fileId, thumbnailUrl: url);

    video.ThumbnailFileId.Should().Be(fileId);
    video.ThumbnailUrl.Should().Be(url);
}

[Fact]
public void UpdateThumbnail_WithNullFileId_ShouldSetUrlOnly()
{
    VideoEntity video = VideoFactory.Create(CategoryId);
    const string url = "https://res.cloudinary.com/test/image/upload/auto-thumb.jpg";

    video.UpdateThumbnail(thumbnailFileId: null, thumbnailUrl: url);

    video.ThumbnailFileId.Should().BeNull();
    video.ThumbnailUrl.Should().Be(url);
}
```

### `ShortVideoEntityTests` (update existing)

**File:** `tests/Unit/Modules/Content/Domain/Entities/ShortVideoEntityTests.cs`

Update existing tests and add new ones:

```csharp
[Fact]
public void CreateStandalone_ShouldSetVideoFileId()
{
    Guid videoFileId = Guid.NewGuid();

    ShortVideoEntity shortVideo = ShortVideoEntity.CreateStandalone(
        id: Guid.NewGuid(),
        title: TestConstants.Content.Editorial.ShortVideo.ValidTitle,
        slug: TestConstants.Content.Editorial.ShortVideo.ValidSlug,
        videoFileId: videoFileId,
        videoUrl: TestConstants.File.ValidStorageUrl,
        authorId: Guid.NewGuid(),
        errors: _i18n.ShortVideo
    );

    shortVideo.VideoFileId.Should().Be(videoFileId);
    shortVideo.VideoUrl.Should().Be(TestConstants.File.ValidStorageUrl);
}

[Fact]
public void CreateTeaser_ShouldSetVideoFileId()
{
    Guid videoFileId = Guid.NewGuid();
    Guid videoId = Guid.NewGuid();

    ShortVideoEntity shortVideo = ShortVideoEntity.CreateTeaser(
        id: Guid.NewGuid(),
        title: TestConstants.Content.Editorial.ShortVideo.ValidTitle,
        slug: TestConstants.Content.Editorial.ShortVideo.ValidSlug,
        videoFileId: videoFileId,
        videoUrl: TestConstants.File.ValidStorageUrl,
        videoId: videoId,
        authorId: Guid.NewGuid(),
        errors: _i18n.ShortVideo
    );

    shortVideo.VideoFileId.Should().Be(videoFileId);
    shortVideo.VideoId.Should().Be(videoId);
}

[Fact]
public void UpdateThumbnail_WithFileId_ShouldSetFileIdAndUrl()
{
    ShortVideoEntity shortVideo = ShortVideoFactory.Create();
    Guid fileId = Guid.NewGuid();
    const string url = "https://res.cloudinary.com/test/image/upload/thumb.jpg";

    shortVideo.UpdateThumbnail(thumbnailFileId: fileId, thumbnailUrl: url);

    shortVideo.ThumbnailFileId.Should().Be(fileId);
    shortVideo.ThumbnailUrl.Should().Be(url);
}

[Fact]
public void UpdateThumbnail_WithNullFileId_ShouldSetAutoGeneratedUrl()
{
    ShortVideoEntity shortVideo = ShortVideoFactory.Create();
    const string url = "https://res.cloudinary.com/test/video/upload/so_1/thumb.jpg";

    shortVideo.UpdateThumbnail(thumbnailFileId: null, thumbnailUrl: url);

    shortVideo.ThumbnailFileId.Should().BeNull();
    shortVideo.ThumbnailUrl.Should().Be(url);
}
```

---

## Handler Tests

### `AdminUploadArticleImageHandlerTests` (update existing or create)

**File:** `tests/Unit/Modules/Content/Application/Editorial/UseCases/Admin/Commands/UploadArticleImage/AdminUploadArticleImageHandlerTests.cs`

```csharp
public class AdminUploadArticleImageHandlerTests : BaseContentHandlerTest
{
    private static readonly Guid CategoryId = Guid.NewGuid();

    private readonly Mock<IArticleRepository> _articleRepositoryMock;
    private readonly Mock<IFileRepository> _fileRepositoryMock;
    private readonly Mock<ICloudinaryService> _cloudinaryServiceMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminUploadArticleImageHandler _handler;

    public AdminUploadArticleImageHandlerTests()
    {
        _articleRepositoryMock = MockArticleRepository.Create();
        _fileRepositoryMock = MockFileRepository.Create();
        _cloudinaryServiceMock = MockCloudinaryService.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();

        _handler = new AdminUploadArticleImageHandler(
            _articleRepositoryMock.Object,
            _fileRepositoryMock.Object,
            _cloudinaryServiceMock.Object,
            _unitOfWorkMock.Object,
            Mapper
        );
    }

    [Fact]
    public async Task Handle_WhenCoverImage_ShouldUseFileRepository()
    {
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        FileEntity fileEntity = FileFactory.CreateWithStorageKey("content/article-images/cover");
        var command = new AdminUploadArticleImageCommand(
            ArticleId: article.Id.ToString(),
            File: FileTestHelpers.CreateMockFormFile(),
            ImageType: EnumArticleImageType.Cover
        );

        _articleRepositoryMock.SetupGetByIdOrThrow(article);
        _articleRepositoryMock.SetupGetImagesByArticleId(article.Id, []);
        _fileRepositoryMock.SetupReplaceImageFile(fileEntity);

        AdminUploadArticleImageResult result = await _handler.Handle(command, CancellationToken.None);

        result.Image.Should().NotBeNull();
        _fileRepositoryMock.VerifyReplaceImageFileCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenCoverImageWithExistingCover_ShouldRemoveOldCover()
    {
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        ArticleImageEntity oldCover = ArticleImageFactory.CreateCover(article.Id);
        FileEntity fileEntity = FileFactory.CreateWithStorageKey("content/article-images/cover");
        var command = new AdminUploadArticleImageCommand(
            ArticleId: article.Id.ToString(),
            File: FileTestHelpers.CreateMockFormFile(),
            ImageType: EnumArticleImageType.Cover
        );

        _articleRepositoryMock.SetupGetByIdOrThrow(article);
        _articleRepositoryMock.SetupGetImagesByArticleId(article.Id, [oldCover]);
        _fileRepositoryMock.SetupReplaceImageFile(fileEntity);

        await _handler.Handle(command, CancellationToken.None);

        _articleRepositoryMock.VerifyRemoveImagesCalled();
        _fileRepositoryMock.VerifyReplaceImageFileCalled();
    }

    [Fact]
    public async Task Handle_WhenBodyImage_ShouldUseCloudinaryServiceDirectly()
    {
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        var command = new AdminUploadArticleImageCommand(
            ArticleId: article.Id.ToString(),
            File: FileTestHelpers.CreateMockFormFile(),
            ImageType: EnumArticleImageType.Body
        );

        _articleRepositoryMock.SetupGetByIdOrThrow(article);
        _cloudinaryServiceMock.SetupUploadImage();

        await _handler.Handle(command, CancellationToken.None);

        // Body images still use Cloudinary directly
        _cloudinaryServiceMock.VerifyUploadCalled();
        // FileRepository should NOT be called for body images
        _fileRepositoryMock.Verify(
            x => x.ReplaceImageFileAsync(
                It.IsAny<Guid?>(),
                It.IsAny<IFormFile>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_WhenArticleNotFound_ShouldThrowNotFoundException()
    {
        Guid nonExistentId = Guid.NewGuid();
        var command = new AdminUploadArticleImageCommand(
            ArticleId: nonExistentId.ToString(),
            File: FileTestHelpers.CreateMockFormFile(),
            ImageType: EnumArticleImageType.Cover
        );

        _articleRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
```

### `AdminUploadVideoThumbnailHandlerTests`

**File:** `tests/Unit/Modules/Content/Application/Editorial/UseCases/Admin/Commands/UploadVideoThumbnail/AdminUploadVideoThumbnailHandlerTests.cs`

```csharp
public class AdminUploadVideoThumbnailHandlerTests
{
    private static readonly Guid CategoryId = Guid.NewGuid();

    private readonly Mock<IVideoRepository> _videoRepositoryMock;
    private readonly Mock<IFileRepository> _fileRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminUploadVideoThumbnailHandler _handler;

    public AdminUploadVideoThumbnailHandlerTests()
    {
        _videoRepositoryMock = MockVideoRepository.Create();
        _fileRepositoryMock = MockFileRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();

        _handler = new AdminUploadVideoThumbnailHandler(
            _videoRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _fileRepositoryMock.Object
        );
    }

    [Fact]
    public async Task Handle_WhenVideoFound_ShouldUploadAndSetThumbnailFileId()
    {
        VideoEntity video = VideoFactory.Create(CategoryId);
        FileEntity fileEntity = FileFactory.CreateWithStorageKey("content/video-thumbnails/thumb");
        var command = new AdminUploadVideoThumbnailCommand(
            VideoId: video.Id.ToString(),
            File: FileTestHelpers.CreateMockFormFile()
        );

        _videoRepositoryMock.SetupGetByIdOrThrow(video);
        _fileRepositoryMock.SetupReplaceImageFile(fileEntity);

        AdminUploadVideoThumbnailResult result = await _handler.Handle(command, CancellationToken.None);

        result.ThumbnailUrl.Should().Be(fileEntity.StorageUrl);
        _fileRepositoryMock.VerifyReplaceImageFileCalled();
        _videoRepositoryMock.VerifyUpdateCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenVideoNotFound_ShouldThrowNotFoundException()
    {
        Guid nonExistentId = Guid.NewGuid();
        var command = new AdminUploadVideoThumbnailCommand(
            VideoId: nonExistentId.ToString(),
            File: FileTestHelpers.CreateMockFormFile()
        );

        _videoRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenVideoHasExistingThumbnail_ShouldReplaceFile()
    {
        Guid existingFileId = Guid.NewGuid();
        VideoEntity video = VideoFactory.Create(CategoryId);
        // Set existing thumbnail via reflection or builder
        video.UpdateThumbnail(thumbnailFileId: existingFileId, thumbnailUrl: "https://old-url.com/thumb.jpg");

        FileEntity newFile = FileFactory.CreateWithStorageKey("content/video-thumbnails/new-thumb");
        var command = new AdminUploadVideoThumbnailCommand(
            VideoId: video.Id.ToString(),
            File: FileTestHelpers.CreateMockFormFile()
        );

        _videoRepositoryMock.SetupGetByIdOrThrow(video);
        _fileRepositoryMock.SetupReplaceImageFile(newFile);

        await _handler.Handle(command, CancellationToken.None);

        _fileRepositoryMock.VerifyReplaceImageFileCalled();
    }
}
```

### `AdminCreateShortVideoHandlerTests` (update existing)

**File:** `tests/Unit/Modules/Content/Application/Editorial/UseCases/Admin/Commands/CreateShortVideo/AdminCreateShortVideoHandlerTests.cs`

Update existing tests:
- Replace `MockCloudinaryService` with `MockFileRepository`
- Use `SetupUploadAndStoreVideoFile()` instead of `SetupUploadImage()`
- Verify `VerifyUploadAndStoreVideoFileCalled()`

```csharp
[Fact]
public async Task Handle_WhenValidStandalone_ShouldCreateWithVideoFileId()
{
    FileEntity videoFile = FileFactory.CreateVideo();
    var command = new AdminCreateShortVideoCommand(
        Title: TestConstants.Content.Editorial.ShortVideo.ValidTitle,
        Slug: "unique-slug",
        VideoFile: FileTestHelpers.CreateMockFormFile(),
        VideoId: null,
        AuthorId: Guid.NewGuid()
    );

    _shortVideoRepositoryMock.SetupGetBySlug("unique-slug", null);
    _fileRepositoryMock.SetupUploadAndStoreVideoFile(videoFile);

    AdminCreateShortVideoResult result = await _handler.Handle(command, CancellationToken.None);

    result.ShortVideo.Should().NotBeNull();
    _fileRepositoryMock.VerifyUploadAndStoreVideoFileCalled();
    _unitOfWorkMock.VerifyCommitCalled();
}
```

### `AdminUploadShortVideoThumbnailHandlerTests`

Same pattern as `AdminUploadVideoThumbnailHandlerTests` — replace `ICloudinaryService` with `IFileRepository`.

### `AdminDeleteShortVideoHandlerTests` (update existing)

Update to verify `SoftDeleteByIdCalled()` instead of `VerifyDeleteImageCalled()`:

```csharp
[Fact]
public async Task Handle_WhenShortVideoFound_ShouldSoftDeleteVideoAndThumbnailFiles()
{
    Guid videoFileId = Guid.NewGuid();
    Guid thumbnailFileId = Guid.NewGuid();
    ShortVideoEntity shortVideo = ShortVideoFactory.CreateWithVideoFileId(videoFileId);
    // Set thumbnail file ID via builder/reflection
    shortVideo.UpdateThumbnail(thumbnailFileId: thumbnailFileId, thumbnailUrl: "https://thumb.jpg");

    var command = new AdminDeleteShortVideoCommand(Id: shortVideo.Id.ToString());

    _shortVideoRepositoryMock.SetupGetByIdOrThrow(shortVideo);
    _fileRepositoryMock.SetupSoftDeleteById();

    await _handler.Handle(command, CancellationToken.None);

    // Both files should be soft-deleted
    _fileRepositoryMock.VerifySoftDeleteByIdCalled(videoFileId);
    _fileRepositoryMock.VerifySoftDeleteByIdCalled(thumbnailFileId);
    _shortVideoRepositoryMock.VerifyRemoveCalled(shortVideo);
    _unitOfWorkMock.VerifyCommitCalled();
}

[Fact]
public async Task Handle_WhenNoThumbnailFile_ShouldOnlySoftDeleteVideoFile()
{
    Guid videoFileId = Guid.NewGuid();
    ShortVideoEntity shortVideo = ShortVideoFactory.CreateWithVideoFileId(videoFileId);
    // ThumbnailFileId is null (auto-generated thumbnail)

    var command = new AdminDeleteShortVideoCommand(Id: shortVideo.Id.ToString());

    _shortVideoRepositoryMock.SetupGetByIdOrThrow(shortVideo);
    _fileRepositoryMock.SetupSoftDeleteById();

    await _handler.Handle(command, CancellationToken.None);

    _fileRepositoryMock.VerifySoftDeleteByIdCalled(videoFileId);
    // Verify soft delete was called exactly once (only video, not thumbnail)
    _fileRepositoryMock.Verify(
        x => x.SoftDeleteByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
        Times.Once
    );
}
```

### `AdminDeleteArticleHandlerTests` (update existing)

Add test for cover image FileEntity soft-deletion:

```csharp
[Fact]
public async Task Handle_WhenArticleHasCoverImage_ShouldSoftDeleteCoverFile()
{
    Guid coverFileId = Guid.NewGuid();
    ArticleEntity article = ArticleFactory.Create(CategoryId);
    article.UpdateCoverImage(coverImageFileId: coverFileId, coverImageUrl: "https://cover.jpg");

    var command = new AdminDeleteArticleCommand(Id: article.Id.ToString());

    _articleRepositoryMock.SetupGetByIdOrThrow(article);
    _articleRepositoryMock.SetupGetImagesByArticleId(article.Id, []);
    _fileRepositoryMock.SetupSoftDeleteById();

    await _handler.Handle(command, CancellationToken.None);

    _fileRepositoryMock.VerifySoftDeleteByIdCalled(coverFileId);
}

[Fact]
public async Task Handle_WhenArticleHasNoCoverImage_ShouldNotCallSoftDelete()
{
    ArticleEntity article = ArticleFactory.Create(CategoryId);
    // CoverImageFileId is null

    var command = new AdminDeleteArticleCommand(Id: article.Id.ToString());

    _articleRepositoryMock.SetupGetByIdOrThrow(article);
    _articleRepositoryMock.SetupGetImagesByArticleId(article.Id, []);

    await _handler.Handle(command, CancellationToken.None);

    _fileRepositoryMock.VerifySoftDeleteByIdNotCalled();
}
```

---

## Test Count Summary

| Category | New Tests | Updated Tests |
|----------|-----------|---------------|
| `FileEntityTests` | 3 | 0 |
| `ArticleEntityTests` | 2 | 0 |
| `VideoEntityTests` | 2 | 0 |
| `ShortVideoEntityTests` | 4 | ~3 (existing create/update tests) |
| `AdminUploadArticleImageHandlerTests` | 4 | 0 |
| `AdminUploadVideoThumbnailHandlerTests` | 3 | 0 |
| `AdminUploadShortVideoThumbnailHandlerTests` | 3 | 0 |
| `AdminCreateShortVideoHandlerTests` | 0 | ~4 (existing tests) |
| `AdminDeleteShortVideoHandlerTests` | 2 | ~2 (existing tests) |
| `AdminDeleteArticleHandlerTests` | 2 | 0 |
| **Total** | **~25 new** | **~9 updated** |

---

## Test Infrastructure Files Changed

| File | Change |
|------|--------|
| `tests/Fixtures/Builders/Entities/FileBuilder.cs` | Add `WithStorageKey()` |
| `tests/Fixtures/Builders/Entities/Content/ShortVideoBuilder.cs` | Add `WithVideoFileId()`, `WithThumbnailFileId()` |
| `tests/Fixtures/Builders/Entities/Content/ArticleBuilder.cs` | Add `WithCoverImageFileId()` |
| `tests/Fixtures/Builders/Entities/Content/VideoBuilder.cs` | Add `WithThumbnailFileId()` |
| `tests/Fixtures/Factories/FileFactory.cs` | Add `CreateWithStorageKey()`, `CreateImage()`, `CreateVideo()` |
| `tests/Fixtures/Factories/Content/ShortVideoFactory.cs` | Update all methods with `videoFileId`, add new methods |
| `tests/Fixtures/Constants/TestConstants.cs` | Add storage key constants, video upload result |
| `tests/Unit/Common/Mocks/Repositories/MockFileRepository.cs` | Add 6 new setup/verify methods |
