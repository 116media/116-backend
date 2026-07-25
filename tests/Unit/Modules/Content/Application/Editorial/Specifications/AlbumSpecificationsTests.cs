using _116.Content.Application.Editorial.Specifications;
using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Factories.Content;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.Specifications;

/// <summary>
/// Unit tests for album specification classes.
/// Note: Specifications using EF.Functions.ILike require a real PostgreSQL provider —
/// those are covered via ToExpression().Compile() only.
/// </summary>
public class AlbumSpecificationsTests
{
    #region AlbumByIdSpecification

    [Fact]
    public void AlbumByIdSpecification_WithMatchingId_ShouldReturnTrue()
    {
        // Arrange
        AlbumEntity album = AlbumFactory.Create();
        var spec = new AlbumByIdSpecification(album.Id);

        // Act
        bool result = spec.IsSatisfiedBy(album);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void AlbumByIdSpecification_WithDifferentId_ShouldReturnFalse()
    {
        // Arrange
        AlbumEntity album = AlbumFactory.Create();
        var spec = new AlbumByIdSpecification(Guid.NewGuid());

        // Act
        bool result = spec.IsSatisfiedBy(album);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region AlbumSearchSpecification

    // ILike: requires PostgreSQL provider — compile-only
    [Fact]
    public void AlbumSearchSpecification_ShouldCompileExpression()
    {
        // Arrange
        var spec = new AlbumSearchSpecification("control");

        // Act
        Func<AlbumEntity, bool> predicate = spec.ToExpression().Compile();

        // Assert
        predicate.Should().NotBeNull();
    }

    #endregion
}
