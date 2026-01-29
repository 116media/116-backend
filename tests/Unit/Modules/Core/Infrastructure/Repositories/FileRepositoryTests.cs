using _116.Core.Application.Shared.Services;
using _116.Core.Domain.Entities;
using _116.Core.Infrastructure.Persistence;
using _116.Core.Infrastructure.Repositories;
using _116.Unit.Tests.Common.Builders.Entities;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Core.Infrastructure.Repositories;

/// <summary>
/// Unit tests for <see cref="FileRepository"/>.
/// </summary>
public class FileRepositoryTests : IDisposable
{
    private readonly CoreDbContext _context;
    private readonly Mock<IFileService> _mockFileService;
    private readonly FileRepository _repository;

    public FileRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<CoreDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new CoreDbContext(options);
        _mockFileService = new Mock<IFileService>();
        _repository = new FileRepository(_context, _mockFileService.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenFileExists_ShouldReturnFile()
    {
        // Arrange
        var file = new FileBuilder().Build();
        _context.Files.Add(file);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(file.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(file.Id);
        result.FileName.Should().Be(file.FileName);
    }

    [Fact]
    public async Task GetByIdAsync_WhenFileDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var fileId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(fileId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WhenFileIsDeleted_ShouldReturnNull()
    {
        // Arrange
        var file = new FileBuilder().AsDeleted().Build();
        _context.Files.Add(file);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(file.Id);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_ShouldAddFileToContext()
    {
        // Arrange
        var file = new FileBuilder().Build();

        // Act
        await _repository.AddAsync(file);
        await _context.SaveChangesAsync();

        // Assert
        var savedFile = await _context.Files.FirstOrDefaultAsync(f => f.Id == file.Id);
        savedFile.Should().NotBeNull();
        savedFile!.Id.Should().Be(file.Id);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_ShouldMarkFileAsModified()
    {
        // Arrange
        var file = new FileBuilder().Build();
        _context.Files.Add(file);
        await _context.SaveChangesAsync();

        var newUrl = "https://new-storage-url.com/file.jpg";
        file.UpdateStorageUrl(newUrl);

        // Act
        await _repository.UpdateAsync(file);
        await _context.SaveChangesAsync();

        // Assert
        var updatedFile = await _context.Files.FirstOrDefaultAsync(f => f.Id == file.Id);
        updatedFile.Should().NotBeNull();
        updatedFile!.StorageUrl.Should().Be(newUrl);
    }

    #endregion

    #region Remove Tests

    [Fact]
    public void Remove_ShouldRemoveFileFromContext()
    {
        // Arrange
        var file = new FileBuilder().Build();
        _context.Files.Add(file);
        _context.SaveChanges();

        // Act
        _repository.Remove(file);
        _context.SaveChanges();

        // Assert
        var removedFile = _context.Files.FirstOrDefault(f => f.Id == file.Id);
        removedFile.Should().BeNull();
    }

    #endregion

    #region GetAvatarFileAsync Tests

    [Fact]
    public async Task GetAvatarFileAsync_WhenAvatarFileIdHasValueAndExists_ShouldReturnFile()
    {
        // Arrange
        var file = new FileBuilder().AsPngImage().Build();
        _context.Files.Add(file);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAvatarFileAsync(file.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(file.Id);
    }

    [Fact]
    public async Task GetAvatarFileAsync_WhenAvatarFileIdIsNull_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetAvatarFileAsync(null);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAvatarFileAsync_WhenAvatarFileIdHasValueButDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var fileId = Guid.NewGuid();

        // Act
        var result = await _repository.GetAvatarFileAsync(fileId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region SaveChangesAsync Tests

    [Fact]
    public async Task SaveChangesAsync_ShouldPersistChanges()
    {
        // Arrange
        var file = new FileBuilder().Build();
        _context.Files.Add(file);

        // Act
        await _repository.SaveChangesAsync();

        // Assert
        var savedFile = await _context.Files.FirstOrDefaultAsync(f => f.Id == file.Id);
        savedFile.Should().NotBeNull();
    }

    #endregion

    #region UploadAndStoreAvatarAsync Tests

    [Fact]
    public async Task UploadAndStoreAvatarAsync_ShouldUploadFileAndStoreMetadata()
    {
        // Arrange
        var mockFormFile = new Mock<IFormFile>();
        var userId = Guid.NewGuid().ToString();
        var originalFileName = "avatar.jpg";
        var mimeType = "image/jpeg";

        var uploadResult = new FileUploadResult(
            FileId: Guid.NewGuid(),
            SecureUrl: "https://cloudinary.com/secure/avatar.jpg",
            Format: "jpg",
            Width: 300,
            Height: 300,
            Bytes: 50000
        );

        _mockFileService
            .Setup(s => s.UploadFileAsync(mockFormFile.Object, userId, "avatars", It.IsAny<CancellationToken>()))
            .ReturnsAsync(uploadResult);

        // Act
        var result = await _repository.UploadAndStoreAvatarAsync(
            mockFormFile.Object,
            userId,
            originalFileName,
            mimeType
        );

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(uploadResult.FileId);
        result.FileName.Should().Be(userId);
        result.OriginalFileName.Should().Be(originalFileName);
        result.MimeType.Should().Be(mimeType);
        result.StorageUrl.Should().Be(uploadResult.SecureUrl);
        result.SizeInBytes.Should().Be(uploadResult.Bytes);

        var savedFile = await _context.Files.FirstOrDefaultAsync(f => f.Id == uploadResult.FileId);
        savedFile.Should().NotBeNull();

        _mockFileService.Verify(
            s => s.UploadFileAsync(mockFormFile.Object, userId, "avatars", It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    #endregion

    #region DownloadAndStoreAvatarFromUrlAsync Tests

    [Fact]
    public async Task DownloadAndStoreAvatarFromUrlAsync_ShouldDownloadAndStoreMetadata()
    {
        // Arrange
        var avatarUrl = "https://social-provider.com/avatar.jpg";
        var userId = Guid.NewGuid().ToString();

        var downloadResult = new FileDownloadResult(
            FileId: Guid.NewGuid(),
            FileName: $"{Guid.NewGuid()}.jpg",
            OriginalFileName: "social-avatar.jpg",
            MimeType: "image/jpeg",
            StorageUrl: avatarUrl,
            SizeInBytes: 45000
        );

        _mockFileService
            .Setup(s => s.DownloadFileAsync(avatarUrl, It.IsAny<CancellationToken>()))
            .ReturnsAsync(downloadResult);

        // Act
        var result = await _repository.DownloadAndStoreAvatarFromUrlAsync(avatarUrl, userId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(downloadResult.FileId);
        result.FileName.Should().Be(downloadResult.FileName);
        result.OriginalFileName.Should().Be(downloadResult.OriginalFileName);
        result.MimeType.Should().Be(downloadResult.MimeType);
        result.StorageUrl.Should().Be(downloadResult.StorageUrl);
        result.SizeInBytes.Should().Be(downloadResult.SizeInBytes);

        var savedFile = await _context.Files.FirstOrDefaultAsync(f => f.Id == downloadResult.FileId);
        savedFile.Should().NotBeNull();

        _mockFileService.Verify(s => s.DownloadFileAsync(avatarUrl, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region UpdateAvatarFromUrlAsync Tests

    [Fact]
    public async Task UpdateAvatarFromUrlAsync_WhenNoCurrentAvatar_ShouldDownloadAndStoreNewAvatar()
    {
        // Arrange
        var newAvatarUrl = "https://social-provider.com/new-avatar.jpg";
        var userId = Guid.NewGuid().ToString();

        var downloadResult = new FileDownloadResult(
            FileId: Guid.NewGuid(),
            FileName: $"{Guid.NewGuid()}.jpg",
            OriginalFileName: "new-avatar.jpg",
            MimeType: "image/jpeg",
            StorageUrl: newAvatarUrl,
            SizeInBytes: 50000
        );

        _mockFileService
            .Setup(s => s.DownloadFileAsync(newAvatarUrl, It.IsAny<CancellationToken>()))
            .ReturnsAsync(downloadResult);

        // Act
        var result = await _repository.UpdateAvatarFromUrlAsync(null, newAvatarUrl, userId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(downloadResult.FileId);
        result.StorageUrl.Should().Be(newAvatarUrl);

        _mockFileService.Verify(s => s.DownloadFileAsync(newAvatarUrl, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAvatarFromUrlAsync_WhenCurrentAvatarHasSameUrl_ShouldReturnNull()
    {
        // Arrange
        var avatarUrl = "https://social-provider.com/avatar.jpg";
        var userId = Guid.NewGuid().ToString();

        var existingFile = new FileBuilder().WithStorageUrl(avatarUrl).Build();

        _context.Files.Add(existingFile);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.UpdateAvatarFromUrlAsync(existingFile.Id, avatarUrl, userId);

        // Assert
        result.Should().BeNull();

        _mockFileService.Verify(
            s => s.DownloadFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task UpdateAvatarFromUrlAsync_WhenCurrentAvatarHasDifferentUrl_ShouldDeleteOldAndDownloadNew()
    {
        // Arrange
        var oldAvatarUrl = "https://social-provider.com/old-avatar.jpg";
        var newAvatarUrl = "https://social-provider.com/new-avatar.jpg";
        var userId = Guid.NewGuid().ToString();

        var oldFile = new FileBuilder().WithStorageUrl(oldAvatarUrl).Build();

        _context.Files.Add(oldFile);
        await _context.SaveChangesAsync();

        var downloadResult = new FileDownloadResult(
            FileId: Guid.NewGuid(),
            FileName: $"{Guid.NewGuid()}.jpg",
            OriginalFileName: "new-avatar.jpg",
            MimeType: "image/jpeg",
            StorageUrl: newAvatarUrl,
            SizeInBytes: 60000
        );

        _mockFileService
            .Setup(s => s.DownloadFileAsync(newAvatarUrl, It.IsAny<CancellationToken>()))
            .ReturnsAsync(downloadResult);

        // Act
        var result = await _repository.UpdateAvatarFromUrlAsync(oldFile.Id, newAvatarUrl, userId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(downloadResult.FileId);
        result.StorageUrl.Should().Be(newAvatarUrl);

        var deletedFile = await _context.Files.FirstOrDefaultAsync(f => f.Id == oldFile.Id);
        deletedFile.Should().BeNull();

        _mockFileService.Verify(s => s.DownloadFileAsync(newAvatarUrl, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAvatarFromUrlAsync_WhenCurrentAvatarDoesNotExist_ShouldDownloadNew()
    {
        // Arrange
        var nonExistentFileId = Guid.NewGuid();
        var newAvatarUrl = "https://social-provider.com/new-avatar.jpg";
        var userId = Guid.NewGuid().ToString();

        var downloadResult = new FileDownloadResult(
            FileId: Guid.NewGuid(),
            FileName: $"{Guid.NewGuid()}.jpg",
            OriginalFileName: "new-avatar.jpg",
            MimeType: "image/jpeg",
            StorageUrl: newAvatarUrl,
            SizeInBytes: 55000
        );

        _mockFileService
            .Setup(s => s.DownloadFileAsync(newAvatarUrl, It.IsAny<CancellationToken>()))
            .ReturnsAsync(downloadResult);

        // Act
        var result = await _repository.UpdateAvatarFromUrlAsync(nonExistentFileId, newAvatarUrl, userId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(downloadResult.FileId);

        _mockFileService.Verify(s => s.DownloadFileAsync(newAvatarUrl, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region UpdateAvatarFromFileAsync Tests

    [Fact]
    public async Task UpdateAvatarFromFileAsync_WhenNoCurrentAvatar_ShouldUploadAndStoreNewAvatar()
    {
        // Arrange
        var mockFormFile = new Mock<IFormFile>();
        var userId = Guid.NewGuid().ToString();
        var originalFileName = "new-avatar.png";
        var mimeType = "image/png";

        var uploadResult = new FileUploadResult(
            FileId: Guid.NewGuid(),
            SecureUrl: "https://cloudinary.com/secure/new-avatar.png",
            Format: "png",
            Width: 400,
            Height: 400,
            Bytes: 70000
        );

        _mockFileService
            .Setup(s => s.UploadFileAsync(mockFormFile.Object, userId, "avatars", It.IsAny<CancellationToken>()))
            .ReturnsAsync(uploadResult);

        // Act
        var result = await _repository.UpdateAvatarFromFileAsync(
            null,
            mockFormFile.Object,
            userId,
            originalFileName,
            mimeType
        );

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(uploadResult.FileId);
        result.StorageUrl.Should().Be(uploadResult.SecureUrl);

        _mockFileService.Verify(
            s => s.UploadFileAsync(mockFormFile.Object, userId, "avatars", It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task UpdateAvatarFromFileAsync_WhenCurrentAvatarExists_ShouldDeleteOldAndUploadNew()
    {
        // Arrange
        var oldFile = new FileBuilder().Build();
        _context.Files.Add(oldFile);
        await _context.SaveChangesAsync();

        var mockFormFile = new Mock<IFormFile>();
        var userId = Guid.NewGuid().ToString();
        var originalFileName = "updated-avatar.jpg";
        var mimeType = "image/jpeg";

        var uploadResult = new FileUploadResult(
            FileId: Guid.NewGuid(),
            SecureUrl: "https://cloudinary.com/secure/updated-avatar.jpg",
            Format: "jpg",
            Width: 500,
            Height: 500,
            Bytes: 80000
        );

        _mockFileService
            .Setup(s => s.UploadFileAsync(mockFormFile.Object, userId, "avatars", It.IsAny<CancellationToken>()))
            .ReturnsAsync(uploadResult);

        // Act
        var result = await _repository.UpdateAvatarFromFileAsync(
            oldFile.Id,
            mockFormFile.Object,
            userId,
            originalFileName,
            mimeType
        );

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(uploadResult.FileId);

        var deletedFile = await _context.Files.FirstOrDefaultAsync(f => f.Id == oldFile.Id);
        deletedFile.Should().BeNull();

        _mockFileService.Verify(
            s => s.UploadFileAsync(mockFormFile.Object, userId, "avatars", It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task UpdateAvatarFromFileAsync_WhenCurrentAvatarDoesNotExist_ShouldUploadNew()
    {
        // Arrange
        var nonExistentFileId = Guid.NewGuid();
        var mockFormFile = new Mock<IFormFile>();
        var userId = Guid.NewGuid().ToString();
        var originalFileName = "avatar.jpg";
        var mimeType = "image/jpeg";

        var uploadResult = new FileUploadResult(
            FileId: Guid.NewGuid(),
            SecureUrl: "https://cloudinary.com/secure/avatar.jpg",
            Format: "jpg",
            Width: 350,
            Height: 350,
            Bytes: 65000
        );

        _mockFileService
            .Setup(s => s.UploadFileAsync(mockFormFile.Object, userId, "avatars", It.IsAny<CancellationToken>()))
            .ReturnsAsync(uploadResult);

        // Act
        var result = await _repository.UpdateAvatarFromFileAsync(
            nonExistentFileId,
            mockFormFile.Object,
            userId,
            originalFileName,
            mimeType
        );

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(uploadResult.FileId);

        _mockFileService.Verify(
            s => s.UploadFileAsync(mockFormFile.Object, userId, "avatars", It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    #endregion

    #region UpdateAvatarUrlFromSourceAsync Tests

    [Fact]
    public async Task UpdateAvatarUrlFromSourceAsync_WhenAvatarUrlIsNull_ShouldReturnNull()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();

        // Act
        var result = await _repository.UpdateAvatarUrlFromSourceAsync(null, null, userId, isAvatarSourceManual: false);

        // Assert
        result.Should().BeNull();

        _mockFileService.Verify(
            s => s.DownloadFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task UpdateAvatarUrlFromSourceAsync_WhenAvatarUrlIsEmpty_ShouldReturnNull()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();

        // Act
        var result = await _repository.UpdateAvatarUrlFromSourceAsync(null, "", userId, isAvatarSourceManual: false);

        // Assert
        result.Should().BeNull();

        _mockFileService.Verify(
            s => s.DownloadFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task UpdateAvatarUrlFromSourceAsync_WhenAvatarUrlIsWhitespace_ShouldReturnNull()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();

        // Act
        var result = await _repository.UpdateAvatarUrlFromSourceAsync(null, "   ", userId, isAvatarSourceManual: false);

        // Assert
        result.Should().BeNull();

        _mockFileService.Verify(
            s => s.DownloadFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task UpdateAvatarUrlFromSourceAsync_WhenIsAvatarSourceManualIsTrue_ShouldReturnNull()
    {
        // Arrange
        var avatarUrl = "https://social-provider.com/avatar.jpg";
        var userId = Guid.NewGuid().ToString();

        // Act
        var result = await _repository.UpdateAvatarUrlFromSourceAsync(
            null,
            avatarUrl,
            userId,
            isAvatarSourceManual: true
        );

        // Assert
        result.Should().BeNull();

        _mockFileService.Verify(
            s => s.DownloadFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task UpdateAvatarUrlFromSourceAsync_WhenConditionsAreMet_ShouldCallUpdateAvatarFromUrlAsync()
    {
        // Arrange
        var avatarUrl = "https://social-provider.com/avatar.jpg";
        var userId = Guid.NewGuid().ToString();

        var downloadResult = new FileDownloadResult(
            FileId: Guid.NewGuid(),
            FileName: $"{Guid.NewGuid()}.jpg",
            OriginalFileName: "avatar.jpg",
            MimeType: "image/jpeg",
            StorageUrl: avatarUrl,
            SizeInBytes: 48000
        );

        _mockFileService
            .Setup(s => s.DownloadFileAsync(avatarUrl, It.IsAny<CancellationToken>()))
            .ReturnsAsync(downloadResult);

        // Act
        var result = await _repository.UpdateAvatarUrlFromSourceAsync(
            null,
            avatarUrl,
            userId,
            isAvatarSourceManual: false
        );

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(downloadResult.FileId);
        result.StorageUrl.Should().Be(avatarUrl);

        _mockFileService.Verify(s => s.DownloadFileAsync(avatarUrl, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion
}
