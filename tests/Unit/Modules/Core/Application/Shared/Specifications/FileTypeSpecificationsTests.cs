using _116.Core.Application.Shared.Specifications;
using _116.Core.Domain.Entities;
using _116.Unit.Tests.Common.Builders.Entities;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Core.Application.Shared.Specifications;

/// <summary>
/// Unit tests for File type specifications.
/// </summary>
public class FileTypeSpecificationsTests
{
    #region FileIsImageSpecification Tests

    [Theory]
    [InlineData("image/jpeg")]
    [InlineData("image/png")]
    [InlineData("image/gif")]
    [InlineData("image/bmp")]
    [InlineData("image/webp")]
    public void FileIsImageSpecification_WithValidImageMimeType_ShouldReturnTrue(string mimeType)
    {
        // Arrange
        FileEntity file = new FileBuilder().WithMimeType(mimeType).Build();
        FileIsImageSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(file);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("application/pdf")]
    [InlineData("application/json")]
    [InlineData("text/plain")]
    [InlineData("video/mp4")]
    public void FileIsImageSpecification_WithNonImageMimeType_ShouldReturnFalse(string mimeType)
    {
        // Arrange
        FileEntity file = new FileBuilder().WithMimeType(mimeType).Build();
        FileIsImageSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(file);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void FileIsImageSpecification_WithUnsupportedImageFormat_ShouldReturnFalse()
    {
        // Arrange - image/ prefix but unsupported format
        FileEntity file = new FileBuilder().WithMimeType("image/tiff").Build();
        FileIsImageSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(file);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void FileIsImageSpecification_WithJpegImage_ShouldReturnTrue()
    {
        // Arrange
        FileEntity file = new FileBuilder().AsJpegImage().Build();
        FileIsImageSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(file);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void FileIsImageSpecification_WithPngImage_ShouldReturnTrue()
    {
        // Arrange
        FileEntity file = new FileBuilder().AsPngImage().Build();
        FileIsImageSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(file);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void FileIsImageSpecification_WithPdfDocument_ShouldReturnFalse()
    {
        // Arrange
        FileEntity file = new FileBuilder().AsPdfDocument().Build();
        FileIsImageSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(file);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region FileIsValidAvatarSpecification Tests

    [Fact]
    public void FileIsValidAvatarSpecification_WithValidImageNotDeleted_ShouldReturnTrue()
    {
        // Arrange
        FileEntity file = new FileBuilder().AsJpegImage().Build();
        FileIsValidAvatarSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(file);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void FileIsValidAvatarSpecification_WithValidImageButDeleted_ShouldReturnFalse()
    {
        // Arrange
        FileEntity file = new FileBuilder().AsJpegImage().AsDeleted().Build();
        FileIsValidAvatarSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(file);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void FileIsValidAvatarSpecification_WithNonImageNotDeleted_ShouldReturnFalse()
    {
        // Arrange
        FileEntity file = new FileBuilder().AsPdfDocument().Build();
        FileIsValidAvatarSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(file);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void FileIsValidAvatarSpecification_WithNonImageAndDeleted_ShouldReturnFalse()
    {
        // Arrange
        FileEntity file = new FileBuilder().AsPdfDocument().AsDeleted().Build();
        FileIsValidAvatarSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(file);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void FileIsValidAvatarSpecification_WithPngImageNotDeleted_ShouldReturnTrue()
    {
        // Arrange
        FileEntity file = new FileBuilder().AsPngImage().Build();
        FileIsValidAvatarSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(file);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region LINQ Integration Tests

    [Fact]
    public void FileIsImageSpecification_WithLinq_ShouldFilterCorrectly()
    {
        // Arrange
        List<FileEntity> files =
        [
            new FileBuilder().AsJpegImage().Build(),
            new FileBuilder().AsPngImage().Build(),
            new FileBuilder().AsPdfDocument().Build(),
            new FileBuilder().WithMimeType("text/plain").Build(),
        ];

        FileIsImageSpecification spec = new();

        // Act
        List<FileEntity> filtered = files.Where(spec.ToExpression().Compile()).ToList();

        // Assert
        filtered.Should().HaveCount(2);
        filtered.All(f => f.MimeType.StartsWith("image/")).Should().BeTrue();
    }

    [Fact]
    public void FileIsValidAvatarSpecification_WithLinq_ShouldFilterCorrectly()
    {
        // Arrange
        List<FileEntity> files =
        [
            new FileBuilder().AsJpegImage().Build(), // Valid avatar
            new FileBuilder().AsPngImage().Build(), // Valid avatar
            new FileBuilder().AsJpegImage().AsDeleted().Build(), // Deleted
            new FileBuilder().AsPdfDocument().Build(), // Not image
        ];

        FileIsValidAvatarSpecification spec = new();

        // Act
        List<FileEntity> filtered = files.Where(spec.ToExpression().Compile()).ToList();

        // Assert
        filtered.Should().HaveCount(2);
        filtered.All(f => !f.IsDeleted && f.MimeType.StartsWith("image/")).Should().BeTrue();
    }

    #endregion
}
