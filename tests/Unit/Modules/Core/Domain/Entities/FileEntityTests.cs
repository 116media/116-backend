using _116.Core.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Core.Domain.Entities;

/// <summary>
/// Unit tests for <see cref="FileEntity"/>.
/// </summary>
public class FileEntityTests
{
    #region Create Tests

    [Fact]
    public void Create_WithValidParameters_ShouldCreateFile()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        string fileName = TestConstants.File.ValidFileName;
        string originalFileName = TestConstants.File.ValidOriginalFileName;
        string mimeType = TestConstants.File.ValidMimeType;
        string storageUrl = TestConstants.File.ValidStorageUrl;
        long sizeInBytes = TestConstants.File.ValidSizeInBytes;

        // Act
        FileEntity file = FileEntity.Create(id, fileName, originalFileName, mimeType, storageUrl, sizeInBytes);

        // Assert
        file.Id.Should().Be(id);
        file.FileName.Should().Be(fileName);
        file.OriginalFileName.Should().Be(originalFileName);
        file.MimeType.Should().Be(mimeType);
        file.StorageUrl.Should().Be(storageUrl);
        file.SizeInBytes.Should().Be(sizeInBytes);
        file.IsDeleted.Should().BeFalse();
        file.DeletedAt.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidFileName_ShouldThrowBadRequestException(string? invalidFileName)
    {
        // Arrange
        Guid id = Guid.NewGuid();

        // Act
        Action act = () =>
            FileEntity.Create(
                id,
                invalidFileName!,
                TestConstants.File.ValidOriginalFileName,
                TestConstants.File.ValidMimeType,
                TestConstants.File.ValidStorageUrl,
                TestConstants.File.ValidSizeInBytes
            );

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidOriginalFileName_ShouldThrowBadRequestException(string? invalidOriginalFileName)
    {
        // Arrange
        Guid id = Guid.NewGuid();

        // Act
        Action act = () =>
            FileEntity.Create(
                id,
                TestConstants.File.ValidFileName,
                invalidOriginalFileName!,
                TestConstants.File.ValidMimeType,
                TestConstants.File.ValidStorageUrl,
                TestConstants.File.ValidSizeInBytes
            );

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidMimeType_ShouldThrowBadRequestException(string? invalidMimeType)
    {
        // Arrange
        Guid id = Guid.NewGuid();

        // Act
        Action act = () =>
            FileEntity.Create(
                id,
                TestConstants.File.ValidFileName,
                TestConstants.File.ValidOriginalFileName,
                invalidMimeType!,
                TestConstants.File.ValidStorageUrl,
                TestConstants.File.ValidSizeInBytes
            );

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidStorageUrl_ShouldThrowBadRequestException(string? invalidStorageUrl)
    {
        // Arrange
        Guid id = Guid.NewGuid();

        // Act
        Action act = () =>
            FileEntity.Create(
                id,
                TestConstants.File.ValidFileName,
                TestConstants.File.ValidOriginalFileName,
                TestConstants.File.ValidMimeType,
                invalidStorageUrl!,
                TestConstants.File.ValidSizeInBytes
            );

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_WithInvalidSizeInBytes_ShouldThrowBadRequestException(long invalidSize)
    {
        // Arrange
        Guid id = Guid.NewGuid();

        // Act
        Action act = () =>
            FileEntity.Create(
                id,
                TestConstants.File.ValidFileName,
                TestConstants.File.ValidOriginalFileName,
                TestConstants.File.ValidMimeType,
                TestConstants.File.ValidStorageUrl,
                invalidSize
            );

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    #endregion

    #region UpdateStorageUrl Tests

    [Fact]
    public void UpdateStorageUrl_WithValidUrl_ShouldUpdateStorageUrl()
    {
        // Arrange
        FileEntity file = FileFactory.Create();
        string newStorageUrl = "https://newcloud.example.com/files/new-file.jpg";

        // Act
        file.UpdateStorageUrl(newStorageUrl);

        // Assert
        file.StorageUrl.Should().Be(newStorageUrl);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateStorageUrl_WithInvalidUrl_ShouldThrowBadRequestException(string? invalidUrl)
    {
        // Arrange
        FileEntity file = FileFactory.Create();

        // Act
        Action act = () => file.UpdateStorageUrl(invalidUrl!);

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    #endregion

    #region Delete Tests

    [Fact]
    public void Delete_WhenNotDeleted_ShouldMarkAsDeletedAndReturnTrue()
    {
        // Arrange
        FileEntity file = FileFactory.Create();

        // Act
        bool result = file.Delete();

        // Assert
        result.Should().BeTrue();
        file.IsDeleted.Should().BeTrue();
        file.DeletedAt.Should().NotBeNull();
        file.DeletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Delete_WhenAlreadyDeleted_ShouldReturnFalse()
    {
        // Arrange
        FileEntity file = FileFactory.CreateDeleted();

        // Act
        bool result = file.Delete();

        // Assert
        result.Should().BeFalse();
        file.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void Delete_WhenAlreadyDeleted_ShouldNotUpdateDeletedAt()
    {
        // Arrange
        FileEntity file = FileFactory.CreateDeleted();
        DateTime? originalDeletedAt = file.DeletedAt;

        // Act
        file.Delete();

        // Assert
        file.DeletedAt.Should().Be(originalDeletedAt);
    }

    #endregion

    #region Builder Tests

    [Fact]
    public void Factory_CreateWithId_ShouldCreateFileWithSpecifiedId()
    {
        // Arrange
        Guid id = Guid.NewGuid();

        // Act
        FileEntity file = FileFactory.CreateWithId(id);

        // Assert
        file.Id.Should().Be(id);
        file.FileName.Should().NotBeNullOrEmpty();
        file.OriginalFileName.Should().NotBeNullOrEmpty();
        file.MimeType.Should().NotBeNullOrEmpty();
        file.StorageUrl.Should().NotBeNullOrEmpty();
        file.SizeInBytes.Should().BePositive();
    }

    [Fact]
    public void Factory_Create_ShouldCreateValidFile()
    {
        // Arrange & Act
        FileEntity file = FileFactory.Create();

        // Assert
        file.Id.Should().NotBeEmpty();
        file.FileName.Should().NotBeNullOrEmpty();
        file.OriginalFileName.Should().NotBeNullOrEmpty();
        file.MimeType.Should().NotBeNullOrEmpty();
        file.StorageUrl.Should().NotBeNullOrEmpty();
        file.SizeInBytes.Should().BePositive();
        file.IsDeleted.Should().BeFalse();
    }

    #endregion
}
