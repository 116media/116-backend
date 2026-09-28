using _116.Storage.Application.Shared.Specifications;
using _116.Storage.Domain.Entities;
using _116.Storage.TestData.Factories;
using AwesomeAssertions;
using Xunit;

namespace _116.Storage.Unit.Tests.Application.Shared.Specifications;

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

    #endregion
}
