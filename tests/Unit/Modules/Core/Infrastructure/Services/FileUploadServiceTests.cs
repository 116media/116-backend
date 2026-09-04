using _116.Core.Application.Shared.Persistence;
using _116.Core.Application.Shared.Services;
using _116.Core.Domain.Entities;
using _116.Core.Infrastructure.Persistence;
using _116.Core.Infrastructure.Repositories;
using _116.Core.Infrastructure.Services;
using _116.Tests.Fixtures.Factories.Core;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Core.Infrastructure.Services;

/// <summary>
/// Unit tests for <see cref="FileUploadService" />: each upload reaches storage, is recorded,
/// and the superseded row is marked replaced before the new one lands.
/// </summary>
public class FileUploadServiceTests : IDisposable
{
    private static readonly DateTime StartInstant = new(2026, 9, 11, 12, 0, 0, DateTimeKind.Utc);

    private readonly FakeTimeProvider _time = new(new DateTimeOffset(StartInstant));
    private readonly CoreDbContext _context;
    private readonly Mock<IFileService> _fileServiceMock = new();
    private readonly Mock<IImageColorService> _imageColorServiceMock = new();
    private readonly Mock<ICoreUnitOfWork> _unitOfWorkMock = new();
    private readonly FileUploadService _service;

    public FileUploadServiceTests()
    {
        DbContextOptions<CoreDbContext> options = new DbContextOptionsBuilder<CoreDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new CoreDbContext(options);

        _unitOfWorkMock
            .Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(async (CancellationToken token) => await _context.SaveChangesAsync(token));

        _service = new FileUploadService(
            new FileRepository(_context, _time),
            _fileServiceMock.Object,
            _imageColorServiceMock.Object,
            _unitOfWorkMock.Object,
            _time
        );
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Points the storage gateway at an upload result for any file.
    /// </summary>
    /// <param name="storageUrl">The URL the upload should report.</param>
    /// <returns>The result the gateway returns.</returns>
    private FileUploadResult SetupUpload(string storageUrl = "https://cdn.test/new.jpg")
    {
        var result = new FileUploadResult(
            FileId: Guid.NewGuid(),
            SecureUrl: storageUrl,
            Format: "jpg",
            Width: 800,
            Height: 600,
            Bytes: 1024,
            PublicId: $"public-{Guid.NewGuid():N}"
        );

        _fileServiceMock
            .Setup(x =>
                x.UploadFileAsync(
                    It.IsAny<IFormFile>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(result);
        _fileServiceMock
            .Setup(x =>
                x.UploadVideoFileAsync(
                    It.IsAny<IFormFile>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(result);
        _fileServiceMock
            .Setup(x =>
                x.UploadRawFileAsync(
                    It.IsAny<IFormFile>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(result);

        return result;
    }

    /// <summary>
    /// Points the storage gateway at a download result for the supplied URL.
    /// </summary>
    /// <param name="avatarUrl">The URL being fetched.</param>
    /// <returns>The result the gateway returns.</returns>
    private FileDownloadResult SetupDownload(string avatarUrl)
    {
        var result = new FileDownloadResult(
            FileId: Guid.NewGuid(),
            FileName: $"{Guid.NewGuid()}.jpg",
            OriginalFileName: "avatar.jpg",
            MimeType: "image/jpeg",
            StorageUrl: avatarUrl,
            SizeInBytes: 2048
        );

        _fileServiceMock.Setup(x => x.DownloadFileAsync(avatarUrl, It.IsAny<CancellationToken>())).ReturnsAsync(result);

        return result;
    }

    /// <summary>
    /// Writes a stored file so a replacement has something to supersede.
    /// </summary>
    /// <returns>The persisted file.</returns>
    private async Task<FileEntity> SeedStoredFileAsync()
    {
        FileEntity file = FileFactory.Create();
        _context.Files.Add(file);
        await _context.SaveChangesAsync();

        return file;
    }

    /// <summary>
    /// Reads a file row ignoring the soft-delete filter.
    /// </summary>
    /// <param name="fileId">The file to read.</param>
    /// <returns>The row, or null.</returns>
    private Task<FileEntity?> ReadIgnoringFiltersAsync(Guid fileId)
    {
        return _context.Files.IgnoreQueryFilters().FirstOrDefaultAsync(file => file.Id == fileId);
    }

    private static IFormFile FakeFile()
    {
        var mock = new Mock<IFormFile>();
        mock.Setup(x => x.FileName).Returns("upload.jpg");
        mock.Setup(x => x.ContentType).Returns("image/jpeg");

        return mock.Object;
    }

    [Fact]
    public async Task UpdateAvatarFromUrlAsync_WithNoCurrentAvatar_ShouldStoreTheDownloadedFile()
    {
        // Arrange
        const string avatarUrl = "https://provider.test/avatar.jpg";
        FileDownloadResult download = SetupDownload(avatarUrl);

        // Act
        FileEntity? result = await _service.UpdateAvatarFromUrlAsync(null, avatarUrl, "user-1");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(download.FileId);
        result.StorageUrl.Should().Be(avatarUrl);
        (await ReadIgnoringFiltersAsync(download.FileId)).Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAvatarFromUrlAsync_WhenTheCurrentAvatarHasTheSameUrl_ShouldDoNothing()
    {
        // Arrange
        FileEntity existing = await SeedStoredFileAsync();

        // Act
        FileEntity? result = await _service.UpdateAvatarFromUrlAsync(existing.Id, existing.StorageUrl, "user-1");

        // Assert
        result.Should().BeNull();
        _fileServiceMock.Verify(
            x => x.DownloadFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task UpdateAvatarFromUrlAsync_WithADifferentUrl_ShouldReplaceTheOldRow()
    {
        // Arrange
        FileEntity existing = await SeedStoredFileAsync();
        const string avatarUrl = "https://provider.test/changed.jpg";
        SetupDownload(avatarUrl);

        // Act
        FileEntity? result = await _service.UpdateAvatarFromUrlAsync(existing.Id, avatarUrl, "user-1");

        // Assert
        result.Should().NotBeNull();
        FileEntity? replaced = await ReadIgnoringFiltersAsync(existing.Id);
        replaced.Should().NotBeNull();
        replaced!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAvatarFromFileAsync_WithNoCurrentAvatar_ShouldStoreTheUpload()
    {
        // Arrange
        FileUploadResult upload = SetupUpload();

        // Act
        FileEntity result = await _service.UpdateAvatarFromFileAsync(
            null,
            FakeFile(),
            "user-1",
            "avatar.jpg",
            "image/jpeg"
        );

        // Assert
        result.Id.Should().Be(upload.FileId);
        (await ReadIgnoringFiltersAsync(upload.FileId)).Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAvatarFromFileAsync_WithACurrentAvatar_ShouldReplaceTheOldRow()
    {
        // Arrange
        FileEntity existing = await SeedStoredFileAsync();
        SetupUpload();

        // Act
        await _service.UpdateAvatarFromFileAsync(existing.Id, FakeFile(), "user-1", "avatar.jpg", "image/jpeg");

        // Assert
        FileEntity? replaced = await ReadIgnoringFiltersAsync(existing.Id);
        replaced!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task ReplaceImageFileAsync_ShouldCarryTheExtractedColorsOntoTheStoredRow()
    {
        // Arrange
        FileUploadResult upload = SetupUpload();
        _imageColorServiceMock
            .Setup(x => x.ExtractAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ImageColors("#112233", "#FFFFFF"));

        // Act
        FileEntity result = await _service.ReplaceImageFileAsync(
            null,
            FakeFile(),
            "poster",
            "posters",
            "poster.jpg",
            "image/jpeg"
        );

        // Assert
        result.Id.Should().Be(upload.FileId);
        result.DominantColorHex.Should().Be("#112233");
        result.ForegroundColorHex.Should().Be("#FFFFFF");
    }

    [Fact]
    public async Task ReplaceImageFileAsync_WhenColorExtractionYieldsNothing_ShouldStillStoreTheFile()
    {
        // Arrange
        FileUploadResult upload = SetupUpload();
        _imageColorServiceMock
            .Setup(x => x.ExtractAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ImageColors?)null);

        // Act
        FileEntity result = await _service.ReplaceImageFileAsync(
            null,
            FakeFile(),
            "poster",
            "posters",
            "poster.jpg",
            "image/jpeg"
        );

        // Assert
        result.Id.Should().Be(upload.FileId);
        result.DominantColorHex.Should().BeNull();
        result.ForegroundColorHex.Should().BeNull();
    }

    [Fact]
    public async Task ReplaceVideoFileAsync_WithACurrentFile_ShouldReplaceTheOldRow()
    {
        // Arrange
        FileEntity existing = await SeedStoredFileAsync();
        SetupUpload();

        // Act
        await _service.ReplaceVideoFileAsync(existing.Id, FakeFile(), "clip", "clips", "clip.mp4", "video/mp4");

        // Assert
        FileEntity? replaced = await ReadIgnoringFiltersAsync(existing.Id);
        replaced!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task UploadAndStoreRawFileAsync_ShouldStoreTheUpload()
    {
        // Arrange
        FileUploadResult upload = SetupUpload();

        // Act
        FileEntity result = await _service.UploadAndStoreRawFileAsync(
            FakeFile(),
            "proof",
            "proofs",
            "proof.pdf",
            "application/pdf"
        );

        // Assert
        result.Id.Should().Be(upload.FileId);
        (await ReadIgnoringFiltersAsync(upload.FileId)).Should().NotBeNull();
    }

    [Fact]
    public async Task ReplaceImageFileAsync_WithACurrentFile_ShouldCommitTheReplacementBeforeTheNewUpload()
    {
        // Arrange
        FileEntity existing = await SeedStoredFileAsync();
        SetupUpload();

        // Act
        await _service.ReplaceImageFileAsync(existing.Id, FakeFile(), "poster", "posters", "p.jpg", "image/jpeg");

        // Assert
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task UploadAndStoreRawFileAsync_ShouldCommitExactlyOnce()
    {
        // Arrange
        SetupUpload();

        // Act
        await _service.UploadAndStoreRawFileAsync(FakeFile(), "proof", "proofs", "proof.pdf", "application/pdf");

        // Assert
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
