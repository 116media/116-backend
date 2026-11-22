using _116.Content.Domain.Entities;
using _116.Content.Domain.Events;
using _116.Content.Domain.Exceptions;
using _116.Content.Domain.StateMachines;
using _116.Tests.TestData.Constants;
using AwesomeAssertions;
using Xunit;

namespace _116.Content.Unit.Tests.Domain.Entities;

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
        var id = Guid.NewGuid();
        string name = TestConstants.Tag.ValidName;
        string slug = TestConstants.Tag.ValidSlug;

        // Act
        var entity = TagEntity.Create(id, name, slug);

        // Assert
        entity.Id.Should().Be(id);
        entity.Name.Should().Be(name);
        entity.Slug.Value.Should().Be(slug);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidName_ShouldThrowBadRequestException(string? invalidName)
    {
        // Act
        Action act = () => TagEntity.Create(Guid.NewGuid(), invalidName!, TestConstants.Tag.ValidSlug);

        // Assert
        act.Should().Throw<ContentRuleException>().Which.Code.Should().Be(ContentRuleCodes.TagNameRequired);
    }
    #endregion

    #region Update Tests
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithInvalidName_ShouldThrowBadRequestException(string? invalidName)
    {
        // Arrange
        var tag = TagEntity.Create(Guid.NewGuid(), TestConstants.Tag.ValidName, TestConstants.Tag.ValidSlug);

        // Act
        Action act = () => tag.Update(name: invalidName!);

        // Assert
        act.Should().Throw<ContentRuleException>().Which.Code.Should().Be(ContentRuleCodes.TagNameRequired);
    }
    #endregion

    [Fact]
    public void Create_ShouldRaiseTagGraphChangedEvent()
    {
        // Act
        TagEntity tag = TagEntity.Create(Guid.NewGuid(), "Kinshasa", "kinshasa");

        // Assert
        tag.DomainEvents.OfType<TagGraphChangedEvent>()
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(new TagGraphChangedEvent(tag.Id));
    }

    [Fact]
    public void Update_ShouldRaiseTagGraphChangedEvent()
    {
        // Arrange
        TagEntity tag = TagEntity.Create(Guid.NewGuid(), "Kinshasa", "kinshasa");
        tag.ClearDomainEvents();

        // Act
        tag.Update("Gombe");

        // Assert
        tag.DomainEvents.OfType<TagGraphChangedEvent>()
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(new TagGraphChangedEvent(tag.Id));
    }

    [Fact]
    public void MarkDeleted_ShouldRaiseTagGraphChangedEvent()
    {
        // Arrange
        TagEntity tag = TagEntity.Create(Guid.NewGuid(), "Kinshasa", "kinshasa");
        tag.ClearDomainEvents();

        // Act
        tag.MarkDeleted();

        // Assert
        tag.DomainEvents.OfType<TagGraphChangedEvent>()
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(new TagGraphChangedEvent(tag.Id));
    }
}
