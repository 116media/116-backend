using _116.Core.Application.Shared.Specifications;
using _116.Core.Domain.Entities;
using _116.Unit.Tests.Common.Factories;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Core.Application.Shared.Specifications;

/// <summary>
/// Unit tests for File status specifications.
/// </summary>
public class FileStatusSpecificationsTests
{
    #region FileIsNotDeletedSpecification Tests

    [Fact]
    public void FileIsNotDeletedSpecification_WithNonDeletedFile_ShouldReturnTrue()
    {
        // Arrange
        FileEntity file = FileFactory.Create();
        FileIsNotDeletedSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(file);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void FileIsNotDeletedSpecification_WithDeletedFile_ShouldReturnFalse()
    {
        // Arrange
        FileEntity file = FileFactory.CreateDeleted();
        FileIsNotDeletedSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(file);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region FileIsDeletedSpecification Tests

    [Fact]
    public void FileIsDeletedSpecification_WithDeletedFile_ShouldReturnTrue()
    {
        // Arrange
        FileEntity file = FileFactory.CreateDeleted();
        FileIsDeletedSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(file);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void FileIsDeletedSpecification_WithNonDeletedFile_ShouldReturnFalse()
    {
        // Arrange
        FileEntity file = FileFactory.Create();
        FileIsDeletedSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(file);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region FileBySizeRangeSpecification Tests

    [Fact]
    public void FileBySizeRangeSpecification_WithFileInRange_ShouldReturnTrue()
    {
        // Arrange
        long fileSize = 5000;
        FileEntity file = FileFactory.CreateWithSize(fileSize);
        FileBySizeRangeSpecification spec = new(minSize: 1000, maxSize: 10000);

        // Act
        bool result = spec.IsSatisfiedBy(file);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void FileBySizeRangeSpecification_WithFileAtMinimum_ShouldReturnTrue()
    {
        // Arrange
        long fileSize = 1000;
        FileEntity file = FileFactory.CreateWithSize(fileSize);
        FileBySizeRangeSpecification spec = new(minSize: 1000, maxSize: 10000);

        // Act
        bool result = spec.IsSatisfiedBy(file);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void FileBySizeRangeSpecification_WithFileAtMaximum_ShouldReturnTrue()
    {
        // Arrange
        long fileSize = 10000;
        FileEntity file = FileFactory.CreateWithSize(fileSize);
        FileBySizeRangeSpecification spec = new(minSize: 1000, maxSize: 10000);

        // Act
        bool result = spec.IsSatisfiedBy(file);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void FileBySizeRangeSpecification_WithFileBelowMinimum_ShouldReturnFalse()
    {
        // Arrange
        long fileSize = 500;
        FileEntity file = FileFactory.CreateWithSize(fileSize);
        FileBySizeRangeSpecification spec = new(minSize: 1000, maxSize: 10000);

        // Act
        bool result = spec.IsSatisfiedBy(file);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void FileBySizeRangeSpecification_WithFileAboveMaximum_ShouldReturnFalse()
    {
        // Arrange
        long fileSize = 15000;
        FileEntity file = FileFactory.CreateWithSize(fileSize);
        FileBySizeRangeSpecification spec = new(minSize: 1000, maxSize: 10000);

        // Act
        bool result = spec.IsSatisfiedBy(file);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region LINQ Integration Tests

    [Fact]
    public void FileIsNotDeletedSpecification_WithLinq_ShouldFilterCorrectly()
    {
        // Arrange
        List<FileEntity> files = [FileFactory.Create(), FileFactory.Create(), FileFactory.CreateDeleted()];

        FileIsNotDeletedSpecification spec = new();

        // Act
        List<FileEntity> filtered = files.Where(spec.ToExpression().Compile()).ToList();

        // Assert
        filtered.Should().HaveCount(2);
        filtered.Should().OnlyContain(f => !f.IsDeleted);
    }

    [Fact]
    public void FileBySizeRangeSpecification_WithLinq_ShouldFilterCorrectly()
    {
        // Arrange
        List<FileEntity> files =
        [
            FileFactory.CreateWithSize(500),
            FileFactory.CreateWithSize(5000),
            FileFactory.CreateWithSize(8000),
            FileFactory.CreateWithSize(15000),
        ];

        FileBySizeRangeSpecification spec = new(minSize: 1000, maxSize: 10000);

        // Act
        List<FileEntity> filtered = files.Where(spec.ToExpression().Compile()).ToList();

        // Assert
        filtered.Should().HaveCount(2);
        filtered.Should().OnlyContain(f => f.SizeInBytes >= 1000 && f.SizeInBytes <= 10000);
    }

    #endregion
}
