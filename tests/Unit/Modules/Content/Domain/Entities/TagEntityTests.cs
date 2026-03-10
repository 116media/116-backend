using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Domain.Entities;

/// <summary>
/// Unit tests for <see cref="TagEntity"/>.
/// </summary>
public class TagEntityTests
{
    #region Create Tests

    [Fact]
    public void Create_WithValidValues_ShouldCreateTag()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        string name = TestConstants.Content.Tag.ValidName;
        string slug = TestConstants.Content.Tag.ValidSlug;

        // Act
        TagEntity entity = TagEntity.Create(id, name, slug);

        // Assert
        entity.Id.Should().Be(id);
        entity.Name.Should().Be(name);
        entity.Slug.Should().Be(slug);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidName_ShouldThrowBadRequestException(string? invalidName)
    {
        // Act
        Action act = () => TagEntity.Create(Guid.NewGuid(), invalidName!, TestConstants.Content.Tag.ValidSlug);

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidSlug_ShouldThrowBadRequestException(string? invalidSlug)
    {
        // Act
        Action act = () => TagEntity.Create(Guid.NewGuid(), TestConstants.Content.Tag.ValidName, invalidSlug!);

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    #endregion
}
