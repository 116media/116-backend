using _116.Core.Application.Shared.Specifications;
using _116.Core.Domain.Entities;
using _116.Unit.Tests.Common.Builders.Entities;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Core.Application.Shared.Specifications;

/// <summary>
/// Unit tests for File identification specifications.
/// </summary>
public class FileIdentificationSpecificationsTests
{
    #region FileByIdSpecification Tests

    [Fact]
    public void FileByIdSpecification_WithMatchingId_ShouldReturnTrue()
    {
        // Arrange
        Guid fileId = Guid.NewGuid();
        FileEntity file = new FileBuilder().WithId(fileId).Build();
        FileByIdSpecification spec = new(fileId);

        // Act
        bool result = spec.IsSatisfiedBy(file);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void FileByIdSpecification_WithDifferentId_ShouldReturnFalse()
    {
        // Arrange
        FileEntity file = new FileBuilder().WithId(Guid.NewGuid()).Build();
        FileByIdSpecification spec = new(Guid.NewGuid());

        // Act
        bool result = spec.IsSatisfiedBy(file);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region FileByOriginalFileNameSpecification Tests

    [Fact]
    public void FileByOriginalFileNameSpecification_WithMatchingName_ShouldReturnTrue()
    {
        // Arrange
        string fileName = "document.pdf";
        FileEntity file = new FileBuilder().WithOriginalFileName(fileName).Build();
        FileByOriginalFileNameSpecification spec = new(fileName);

        // Act
        bool result = spec.IsSatisfiedBy(file);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void FileByOriginalFileNameSpecification_WithDifferentName_ShouldReturnFalse()
    {
        // Arrange
        FileEntity file = new FileBuilder().WithOriginalFileName("document.pdf").Build();
        FileByOriginalFileNameSpecification spec = new("image.jpg");

        // Act
        bool result = spec.IsSatisfiedBy(file);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void FileByOriginalFileNameSpecification_IsCaseSensitive()
    {
        // Arrange
        FileEntity file = new FileBuilder().WithOriginalFileName("Document.PDF").Build();
        FileByOriginalFileNameSpecification spec = new("document.pdf");

        // Act
        bool result = spec.IsSatisfiedBy(file);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region FileByFileNameSpecification Tests

    [Fact]
    public void FileByFileNameSpecification_WithMatchingName_ShouldReturnTrue()
    {
        // Arrange
        string storedFileName = "abc123-def456.pdf";
        FileEntity file = new FileBuilder().WithFileName(storedFileName).Build();
        FileByFileNameSpecification spec = new(storedFileName);

        // Act
        bool result = spec.IsSatisfiedBy(file);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void FileByFileNameSpecification_WithDifferentName_ShouldReturnFalse()
    {
        // Arrange
        FileEntity file = new FileBuilder().WithFileName("abc123-def456.pdf").Build();
        FileByFileNameSpecification spec = new("xyz789-uvw012.pdf");

        // Act
        bool result = spec.IsSatisfiedBy(file);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region FileByMimeTypeSpecification Tests

    [Fact]
    public void FileByMimeTypeSpecification_WithMatchingMimeType_ShouldReturnTrue()
    {
        // Arrange
        string mimeType = "image/jpeg";
        FileEntity file = new FileBuilder().WithMimeType(mimeType).Build();
        FileByMimeTypeSpecification spec = new(mimeType);

        // Act
        bool result = spec.IsSatisfiedBy(file);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void FileByMimeTypeSpecification_WithDifferentMimeType_ShouldReturnFalse()
    {
        // Arrange
        FileEntity file = new FileBuilder().WithMimeType("image/jpeg").Build();
        FileByMimeTypeSpecification spec = new("application/pdf");

        // Act
        bool result = spec.IsSatisfiedBy(file);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void FileByMimeTypeSpecification_WithJpegImage_ShouldReturnTrue()
    {
        // Arrange
        FileEntity file = new FileBuilder().AsJpegImage().Build();
        FileByMimeTypeSpecification spec = new("image/jpeg");

        // Act
        bool result = spec.IsSatisfiedBy(file);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void FileByMimeTypeSpecification_WithPdfDocument_ShouldReturnTrue()
    {
        // Arrange
        FileEntity file = new FileBuilder().AsPdfDocument().Build();
        FileByMimeTypeSpecification spec = new("application/pdf");

        // Act
        bool result = spec.IsSatisfiedBy(file);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region FileByIdNotDeletedSpecification Tests

    [Fact]
    public void FileByIdNotDeletedSpecification_WithMatchingIdNotDeleted_ShouldReturnTrue()
    {
        // Arrange
        Guid fileId = Guid.NewGuid();
        FileEntity file = new FileBuilder().WithId(fileId).Build();
        FileByIdNotDeletedSpecification spec = new(fileId);

        // Act
        bool result = spec.IsSatisfiedBy(file);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void FileByIdNotDeletedSpecification_WithMatchingIdButDeleted_ShouldReturnFalse()
    {
        // Arrange
        Guid fileId = Guid.NewGuid();
        FileEntity file = new FileBuilder().WithId(fileId).AsDeleted().Build();
        FileByIdNotDeletedSpecification spec = new(fileId);

        // Act
        bool result = spec.IsSatisfiedBy(file);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void FileByIdNotDeletedSpecification_WithDifferentIdNotDeleted_ShouldReturnFalse()
    {
        // Arrange
        FileEntity file = new FileBuilder().WithId(Guid.NewGuid()).Build();
        FileByIdNotDeletedSpecification spec = new(Guid.NewGuid());

        // Act
        bool result = spec.IsSatisfiedBy(file);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void FileByIdNotDeletedSpecification_WithDifferentIdAndDeleted_ShouldReturnFalse()
    {
        // Arrange
        FileEntity file = new FileBuilder().WithId(Guid.NewGuid()).AsDeleted().Build();
        FileByIdNotDeletedSpecification spec = new(Guid.NewGuid());

        // Act
        bool result = spec.IsSatisfiedBy(file);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region LINQ Integration Tests

    [Fact]
    public void FileByIdSpecification_WithLinq_ShouldFilterCorrectly()
    {
        // Arrange
        Guid targetId = Guid.NewGuid();
        List<FileEntity> files =
        [
            new FileBuilder().WithId(targetId).Build(),
            new FileBuilder().WithId(Guid.NewGuid()).Build(),
            new FileBuilder().WithId(Guid.NewGuid()).Build(),
        ];

        FileByIdSpecification spec = new(targetId);

        // Act
        List<FileEntity> filtered = files.Where(spec.ToExpression().Compile()).ToList();

        // Assert
        filtered.Should().HaveCount(1);
        filtered[0].Id.Should().Be(targetId);
    }

    [Fact]
    public void FileByMimeTypeSpecification_WithLinq_ShouldFilterCorrectly()
    {
        // Arrange
        List<FileEntity> files =
        [
            new FileBuilder().AsJpegImage().Build(),
            new FileBuilder().AsJpegImage().Build(),
            new FileBuilder().AsPngImage().Build(),
            new FileBuilder().AsPdfDocument().Build(),
        ];

        FileByMimeTypeSpecification spec = new("image/jpeg");

        // Act
        List<FileEntity> filtered = files.Where(spec.ToExpression().Compile()).ToList();

        // Assert
        filtered.Should().HaveCount(2);
        filtered.All(f => f.MimeType == "image/jpeg").Should().BeTrue();
    }

    [Fact]
    public void FileByIdNotDeletedSpecification_WithLinq_ShouldFilterCorrectly()
    {
        // Arrange
        Guid targetId = Guid.NewGuid();
        List<FileEntity> files =
        [
            new FileBuilder().WithId(targetId).Build(), // Match
            new FileBuilder().WithId(targetId).AsDeleted().Build(), // Wrong - deleted
            new FileBuilder().WithId(Guid.NewGuid()).Build(), // Wrong - different ID
            new FileBuilder().WithId(Guid.NewGuid()).AsDeleted().Build(), // Wrong - both
        ];

        FileByIdNotDeletedSpecification spec = new(targetId);

        // Act
        List<FileEntity> filtered = files.Where(spec.ToExpression().Compile()).ToList();

        // Assert
        filtered.Should().HaveCount(1);
        filtered[0].Id.Should().Be(targetId);
        filtered[0].IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void FileByOriginalFileNameSpecification_WithLinq_ShouldFilterCorrectly()
    {
        // Arrange
        string targetName = "important-document.pdf";
        List<FileEntity> files =
        [
            new FileBuilder().WithOriginalFileName(targetName).Build(),
            new FileBuilder().WithOriginalFileName(targetName).Build(),
            new FileBuilder().WithOriginalFileName("other-file.pdf").Build(),
        ];

        FileByOriginalFileNameSpecification spec = new(targetName);

        // Act
        List<FileEntity> filtered = files.Where(spec.ToExpression().Compile()).ToList();

        // Assert
        filtered.Should().HaveCount(2);
        filtered.All(f => f.OriginalFileName == targetName).Should().BeTrue();
    }

    #endregion
}
