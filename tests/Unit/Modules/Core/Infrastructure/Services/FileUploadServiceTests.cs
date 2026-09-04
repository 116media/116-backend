using _116.Core.Application.Shared.Services;
using _116.Core.Domain.Entities;
using _116.Core.Domain.Enums;
using _116.Core.Domain.Exceptions;
using _116.Core.Domain.StateMachines;
using _116.Core.Infrastructure.Persistence;
using _116.Core.Infrastructure.Repositories;
using _116.Core.Infrastructure.Services;
using _116.Shared.Application.Services;
using _116.Shared.Infrastructure.interceptors;
using _116.Tests.Fixtures.Factories.Core;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Core.Infrastructure.Services;

/// <summary>
/// Unit tests for <see cref="FileUploadService" />: uploads describe what reached storage without
/// touching the database, and recording stages the row without committing it.
/// </summary>
public class FileUploadServiceTests : IDisposable
{
    private static readonly DateTime StartInstant = new(2026, 9, 11, 12, 0, 0, DateTimeKind.Utc);

    private readonly FakeTimeProvider _time = new(new DateTimeOffset(StartInstant));
    private readonly CoreDbContext _context;
    private readonly Mock<IFileService> _fileServiceMock = new();
    private readonly Mock<IImageColorService> _imageColorServiceMock = new();
    private readonly FileUploadService _service;

    public FileUploadServiceTests()
    {
        // The audit interceptor is what stamps CreatedAt, which is how a recorded file is told
        // from an unrecorded one; without it a seeded row would not look persisted.
        DbContextOptions<CoreDbContext> options = new DbContextOptionsBuilder<CoreDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(
                new AuditableEntityInterceptor(
                    Mock.Of<ICurrentActor>(actor =>
                        actor.UserId == null && actor.IsAuthenticated == false && actor.HasHttpContext == false
                    ),
                    _time
                )
            )
            .Options;

        _context = new CoreDbContext(options);

        _service = new FileUploadService(
            new FileRepository(_context, _time),
            _fileServiceMock.Object,
            _imageColorServiceMock.Object,
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
    public async Task UploadImageAsync_ShouldCarryTheExtractedColorsOntoTheFile()
    {
        // Arrange
        FileUploadResult upload = SetupUpload();
        _imageColorServiceMock
            .Setup(x => x.ExtractAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ImageColors("#112233", "#FFFFFF"));

        // Act
        FileEntity uploaded = await _service.UploadImageAsync(
            FakeFile(),
            "poster",
            "posters",
            "poster.jpg",
            "image/jpeg"
        );

        // Assert
        uploaded.Id.Should().Be(upload.FileId);
        uploaded.StorageUrl.Should().Be(upload.SecureUrl);
        uploaded.StorageKey.Should().Be(upload.PublicId);
        uploaded.DominantColorHex.Should().Be("#112233");
        uploaded.ForegroundColorHex.Should().Be("#FFFFFF");
    }

    [Fact]
    public async Task UploadImageAsync_WhenColorExtractionYieldsNothing_ShouldStillDescribeTheUpload()
    {
        // Arrange
        FileUploadResult upload = SetupUpload();
        _imageColorServiceMock
            .Setup(x => x.ExtractAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ImageColors?)null);

        // Act
        FileEntity uploaded = await _service.UploadImageAsync(
            FakeFile(),
            "poster",
            "posters",
            "poster.jpg",
            "image/jpeg"
        );

        // Assert
        uploaded.Id.Should().Be(upload.FileId);
        uploaded.DominantColorHex.Should().BeNull();
        uploaded.ForegroundColorHex.Should().BeNull();
    }

    [Fact]
    public async Task UploadImageAsync_ShouldWriteNothingToTheDatabase()
    {
        // Arrange
        SetupUpload();

        // Act
        FileEntity uploaded = await _service.UploadImageAsync(
            FakeFile(),
            "poster",
            "posters",
            "poster.jpg",
            "image/jpeg"
        );

        // Assert
        (await ReadIgnoringFiltersAsync(uploaded.Id))
            .Should()
            .BeNull();
        _context.ChangeTracker.Entries<FileEntity>().Should().BeEmpty();
    }

    [Fact]
    public async Task UploadVideoAsync_ShouldDescribeTheVideoUpload()
    {
        // Arrange
        FileUploadResult upload = SetupUpload();

        // Act
        FileEntity uploaded = await _service.UploadVideoAsync(FakeFile(), "clip", "clips", "clip.mp4", "video/mp4");

        // Assert
        uploaded.Id.Should().Be(upload.FileId);
        uploaded.MimeType.Should().Be("video/mp4");
        _fileServiceMock.Verify(
            x => x.UploadVideoFileAsync(It.IsAny<IFormFile>(), "clip", "clips", It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task UploadRawAsync_ShouldDescribeTheRawUpload()
    {
        // Arrange
        FileUploadResult upload = SetupUpload();

        // Act
        FileEntity uploaded = await _service.UploadRawAsync(
            FakeFile(),
            "proof",
            "proofs",
            "proof.pdf",
            "application/pdf"
        );

        // Assert
        uploaded.Id.Should().Be(upload.FileId);
        uploaded.MimeType.Should().Be("application/pdf");
        _fileServiceMock.Verify(
            x => x.UploadRawFileAsync(It.IsAny<IFormFile>(), "proof", "proofs", It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task UploadAvatarAsync_ShouldUploadUnderTheOwnersIdInTheAvatarsFolder()
    {
        // Arrange
        FileUploadResult upload = SetupUpload();

        // Act
        FileEntity uploaded = await _service.UploadAvatarAsync(FakeFile(), "user-1", "avatar.jpg", "image/jpeg");

        // Assert
        uploaded.Id.Should().Be(upload.FileId);
        uploaded.FileName.Should().Be("user-1");
        _fileServiceMock.Verify(
            x => x.UploadFileAsync(It.IsAny<IFormFile>(), "user-1", "avatars", It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task UploadAvatarFromUrlAsync_WithNoCurrentAvatar_ShouldDescribeTheDownload()
    {
        // Arrange
        const string avatarUrl = "https://provider.test/avatar.jpg";
        FileDownloadResult download = SetupDownload(avatarUrl);

        // Act
        FileEntity? avatar = await _service.UploadAvatarFromUrlAsync(null, avatarUrl);

        // Assert
        avatar.Should().NotBeNull();
        avatar!.Id.Should().Be(download.FileId);
        avatar.StorageUrl.Should().Be(avatarUrl);
    }

    [Fact]
    public async Task UploadAvatarFromUrlAsync_WhenTheCurrentAvatarHasTheSameUrl_ShouldDoNothing()
    {
        // Arrange
        FileEntity existing = await SeedStoredFileAsync();

        // Act
        FileEntity? avatar = await _service.UploadAvatarFromUrlAsync(existing.Id, existing.StorageUrl);

        // Assert
        avatar.Should().BeNull();
        _fileServiceMock.Verify(
            x => x.DownloadFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task UploadAvatarFromUrlAsync_WithADifferentUrl_ShouldDescribeTheDownload()
    {
        // Arrange
        FileEntity existing = await SeedStoredFileAsync();
        const string avatarUrl = "https://provider.test/changed.jpg";
        FileDownloadResult download = SetupDownload(avatarUrl);

        // Act
        FileEntity? avatar = await _service.UploadAvatarFromUrlAsync(existing.Id, avatarUrl);

        // Assert
        avatar.Should().NotBeNull();
        avatar!.Id.Should().Be(download.FileId);
    }

    [Fact]
    public async Task RecordAsync_ShouldStageTheRowWithoutCommittingIt()
    {
        // Arrange
        SetupUpload();
        FileEntity uploaded = await _service.UploadImageAsync(
            FakeFile(),
            "poster",
            "posters",
            "poster.jpg",
            "image/jpeg"
        );

        // Act
        FileEntity recorded = await _service.RecordAsync(uploaded);

        // Assert
        recorded.Id.Should().Be(uploaded.Id);
        _context.Entry(recorded).State.Should().Be(EntityState.Added);
        (await ReadIgnoringFiltersAsync(uploaded.Id)).Should().BeNull();
    }

    [Fact]
    public async Task RecordAsync_WithASupersededFile_ShouldMarkTheOldRowReplaced()
    {
        // Arrange
        FileEntity existing = await SeedStoredFileAsync();
        SetupUpload();
        FileEntity uploaded = await _service.UploadImageAsync(
            FakeFile(),
            "poster",
            "posters",
            "poster.jpg",
            "image/jpeg"
        );

        // Act
        await _service.RecordAsync(uploaded, existing.Id);
        await _context.SaveChangesAsync();

        // Assert
        FileEntity? replaced = await ReadIgnoringFiltersAsync(existing.Id);
        replaced.Should().NotBeNull();
        replaced!.State.Should().Be(EnumFileState.Replaced);
        replaced.DeletedAt.Should().Be(StartInstant);
        (await ReadIgnoringFiltersAsync(uploaded.Id)).Should().NotBeNull();
    }

    [Fact]
    public async Task RecordAsync_WithAFileThatAlreadyHasARow_ShouldRefuseToRecordItTwice()
    {
        // Arrange
        FileEntity recorded = await SeedStoredFileAsync();

        // Act
        Func<Task> act = async () => await _service.RecordAsync(recorded);

        // Assert
        await act.Should()
            .ThrowAsync<CoreRuleException>()
            .Where(exception => exception.Code == CoreRuleCodes.FileAlreadyRecorded);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task UploadImageAsync_WithoutAMimeType_ShouldRejectBeforeReachingStorage(string mimeType)
    {
        // Arrange
        SetupUpload();

        // Act
        Func<Task> act = async () =>
            await _service.UploadImageAsync(FakeFile(), "poster", "posters", "poster.jpg", mimeType);

        // Assert
        await act.Should()
            .ThrowAsync<CoreRuleException>()
            .Where(exception => exception.Code == CoreRuleCodes.MimeTypeRequired);
        _fileServiceMock.Verify(
            x =>
                x.UploadFileAsync(
                    It.IsAny<IFormFile>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task UploadImageAsync_WithoutAnOriginalFileName_ShouldRejectBeforeReachingStorage()
    {
        // Arrange
        SetupUpload();

        // Act
        Func<Task> act = async () =>
            await _service.UploadImageAsync(FakeFile(), "poster", "posters", " ", "image/jpeg");

        // Assert
        await act.Should()
            .ThrowAsync<CoreRuleException>()
            .Where(exception => exception.Code == CoreRuleCodes.OriginalFileNameRequired);
        _fileServiceMock.Verify(
            x =>
                x.UploadFileAsync(
                    It.IsAny<IFormFile>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }
}
