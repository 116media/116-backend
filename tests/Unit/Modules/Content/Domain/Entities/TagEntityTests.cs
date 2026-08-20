using _116.Content.Domain.Entities;
using _116.Content.Domain.Events;
using _116.Content.Domain.Exceptions;
using _116.Content.Domain.StateMachines;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
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
        var id = Guid.NewGuid();
        string name = TestConstants.Tag.ValidName;
        string slug = TestConstants.Tag.ValidSlug;

        // Act
        var entity = TagEntity.Create(id, name, slug);

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
        Action act = () => TagEntity.Create(Guid.NewGuid(), invalidName!, TestConstants.Tag.ValidSlug);

        // Assert
        act.Should().Throw<ContentRuleException>().Which.Code.Should().Be(ContentRuleCodes.TagNameRequired);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidSlug_ShouldThrowBadRequestException(string? invalidSlug)
    {
        // Act
        Action act = () => TagEntity.Create(Guid.NewGuid(), TestConstants.Tag.ValidName, invalidSlug!);

        // Assert
        act.Should().Throw<ContentRuleException>().Which.Code.Should().Be(ContentRuleCodes.TagSlugRequired);
    }

    #endregion

    #region Update Tests

    [Fact]
    public void Update_WithValidValues_ShouldUpdateNameAndSlug()
    {
        // Arrange
        var tag = TagEntity.Create(Guid.NewGuid(), TestConstants.Tag.ValidName, TestConstants.Tag.ValidSlug);

        // Act
        tag.Update(name: "New Name", slug: "new-slug");

        // Assert
        tag.Name.Should().Be("New Name");
        tag.Slug.Should().Be("new-slug");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithInvalidName_ShouldThrowBadRequestException(string? invalidName)
    {
        // Arrange
        var tag = TagEntity.Create(Guid.NewGuid(), TestConstants.Tag.ValidName, TestConstants.Tag.ValidSlug);

        // Act
        Action act = () => tag.Update(name: invalidName!, slug: "valid-slug");

        // Assert
        act.Should().Throw<ContentRuleException>().Which.Code.Should().Be(ContentRuleCodes.TagNameRequired);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithInvalidSlug_ShouldThrowBadRequestException(string? invalidSlug)
    {
        // Arrange
        var tag = TagEntity.Create(Guid.NewGuid(), TestConstants.Tag.ValidName, TestConstants.Tag.ValidSlug);

        // Act
        Action act = () => tag.Update(name: "Valid Name", slug: invalidSlug!);

        // Assert
        act.Should().Throw<ContentRuleException>().Which.Code.Should().Be(ContentRuleCodes.TagSlugRequired);
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
        tag.Update("Gombe", "gombe");

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
