using _116.Core.Application.Shared.Specifications;
using _116.Core.Domain.Entities;
using _116.Unit.Tests.Common.Factories;
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
        FileEntity file = FileFactory.CreateWithId(fileId);
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
        FileEntity file = FileFactory.CreateWithId(Guid.NewGuid());
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
        FileEntity file = FileFactory.Create(fileName);
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
        FileEntity file = FileFactory.Create("document.pdf");
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
        FileEntity file = FileFactory.Create("Document.PDF");
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
        FileEntity file = FileFactory.CreateWithFileName(storedFileName);
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
        FileEntity file = FileFactory.CreateWithFileName("abc123-def456.pdf");
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
        FileEntity file = FileFactory.CreateWithMimeType(mimeType);
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
        FileEntity file = FileFactory.CreateWithMimeType("image/jpeg");
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
        FileEntity file = FileFactory.CreateJpeg();
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
        FileEntity file = FileFactory.CreatePdf();
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
        FileEntity file = FileFactory.CreateWithId(fileId);
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
        FileEntity file = FileFactory.CreateDeletedWithId(fileId);
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
        FileEntity file = FileFactory.CreateWithId(Guid.NewGuid());
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
        FileEntity file = FileFactory.CreateDeletedWithId(Guid.NewGuid());
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
            FileFactory.CreateWithId(targetId),
            FileFactory.CreateWithId(Guid.NewGuid()),
            FileFactory.CreateWithId(Guid.NewGuid()),
        ];

        FileByIdSpecification spec = new(targetId);

        // Act
        List<FileEntity> filtered = files.Where(spec.ToExpression().Compile()).ToList();

        // Assert
        filtered.Should().ContainSingle();
        filtered[0].Id.Should().Be(targetId);
    }

    [Fact]
    public void FileByMimeTypeSpecification_WithLinq_ShouldFilterCorrectly()
    {
        // Arrange
        List<FileEntity> files =
        [
            FileFactory.CreateJpeg(),
            FileFactory.CreateJpeg(),
            FileFactory.CreatePng(),
            FileFactory.CreatePdf(),
        ];

        FileByMimeTypeSpecification spec = new("image/jpeg");

        // Act
        List<FileEntity> filtered = files.Where(spec.ToExpression().Compile()).ToList();

        // Assert
        filtered.Should().HaveCount(2);
        filtered.Should().OnlyContain(f => f.MimeType == "image/jpeg");
    }

    [Fact]
    public void FileByIdNotDeletedSpecification_WithLinq_ShouldFilterCorrectly()
    {
        // Arrange
        Guid targetId = Guid.NewGuid();
        List<FileEntity> files =
        [
            FileFactory.CreateWithId(targetId), // Match
            FileFactory.CreateDeletedWithId(targetId), // Wrong - deleted
            FileFactory.CreateWithId(Guid.NewGuid()), // Wrong - different ID
            FileFactory.CreateDeletedWithId(Guid.NewGuid()), // Wrong - both
        ];

        FileByIdNotDeletedSpecification spec = new(targetId);

        // Act
        List<FileEntity> filtered = files.Where(spec.ToExpression().Compile()).ToList();

        // Assert
        filtered.Should().ContainSingle();
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
            FileFactory.Create(targetName),
            FileFactory.Create(targetName),
            FileFactory.Create("other-file.pdf"),
        ];

        FileByOriginalFileNameSpecification spec = new(targetName);

        // Act
        List<FileEntity> filtered = files.Where(spec.ToExpression().Compile()).ToList();

        // Assert
        filtered.Should().HaveCount(2);
        filtered.Should().OnlyContain(f => f.OriginalFileName == targetName);
    }

    #endregion
}
