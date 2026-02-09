using _116.Core.Application.Shared.DTOs;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Core.Application.Shared.DTOs;

/// <summary>
/// Unit tests for <see cref="FileDto"/>.
/// </summary>
public class FileDtoTests
{
    [Fact]
    public void FileDto_WithAllProperties_ShouldCreateSuccessfully()
    {
        // Arrange
        var id = Guid.NewGuid();
        string fileName = "abc123.jpg";
        string originalFileName = "avatar.jpg";
        string mimeType = "image/jpeg";
        string storageUrl = "https://cloudinary.com/image.jpg";
        long sizeInBytes = 1024;
        bool isDeleted = false;

        // Act
        var dto = new FileDto(id, fileName, originalFileName, mimeType, storageUrl, sizeInBytes, isDeleted);

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(id);
        dto.FileName.Should().Be(fileName);
        dto.OriginalFileName.Should().Be(originalFileName);
        dto.MimeType.Should().Be(mimeType);
        dto.StorageUrl.Should().Be(storageUrl);
        dto.SizeInBytes.Should().Be(sizeInBytes);
        dto.IsDeleted.Should().Be(isDeleted);
    }

    [Fact]
    public void FileDto_WithDeletedStatus_ShouldReflectStatus()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var dto = new FileDto(
            id,
            "deleted.jpg",
            "removed.jpg",
            "image/jpeg",
            "https://storage.url/deleted.jpg",
            2048,
            true
        );

        // Assert
        dto.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void FileDto_Equality_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto1 = new FileDto(id, "file.jpg", "original.jpg", "image/jpeg", "https://url.com/file.jpg", 1024, false);
        var dto2 = new FileDto(id, "file.jpg", "original.jpg", "image/jpeg", "https://url.com/file.jpg", 1024, false);

        // Act & Assert
        dto1.Should().Be(dto2);
    }

    [Fact]
    public void FileDto_Equality_WithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var dto1 = new FileDto(
            Guid.NewGuid(),
            "file1.jpg",
            "original1.jpg",
            "image/jpeg",
            "https://url.com/file1.jpg",
            1024,
            false
        );
        var dto2 = new FileDto(
            Guid.NewGuid(),
            "file2.jpg",
            "original2.jpg",
            "image/png",
            "https://url.com/file2.jpg",
            2048,
            true
        );

        // Act & Assert
        dto1.Should().NotBe(dto2);
    }
}
