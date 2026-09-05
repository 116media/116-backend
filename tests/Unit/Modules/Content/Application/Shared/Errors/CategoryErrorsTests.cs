using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Shared.Errors;

/// <summary>
/// Unit tests for <see cref="CategoryErrors"/>.
/// </summary>
public class CategoryErrorsTests
{
    private readonly CategoryErrors _errors = TestErrorsFactory.CreateCategoryErrors();
    private readonly CategoryErrorMessage _message = LocalizerFactory.CreateMessage<CategoryErrorMessage>();

    [Fact]
    public void AlreadyExists_WithSlug_ShouldReturnConflictException()
    {
        // Arrange
        string slug = "artist-profile";

        // Act
        ConflictException exception = _errors.AlreadyExists(slug);

        // Assert
        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().Contain(slug);
    }

    [Fact]
    public void NotFound_WithId_ShouldReturnNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        NotFoundException exception = _errors.NotFound(id);

        // Assert
        exception.Should().BeOfType<NotFoundException>();
        exception.Message.Should().Contain(id.ToString());
    }

    [Fact]
    public void AlreadyActive_ShouldReturnConflictException()
    {
        ConflictException exception = _errors.AlreadyActive();

        exception.Should().NotBeNull();
        exception.Message.Should().Contain(_message.AlreadyActive());
    }

    [Fact]
    public void AlreadyInactive_ShouldReturnConflictException()
    {
        ConflictException exception = _errors.AlreadyInactive();

        exception.Should().NotBeNull();
        exception.Message.Should().Contain(_message.AlreadyInactive());
    }

    [Fact]
    public void PricingAlreadyExists_ShouldReturnConflictException()
    {
        ConflictException exception = _errors.PricingAlreadyExists();

        exception.Should().NotBeNull();
        exception.Message.Should().Contain(_message.PricingAlreadyExists());
    }

    [Fact]
    public void PricingNotFound_WithIds_ShouldReturnNotFoundException()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var tierId = Guid.NewGuid();

        // Act
        NotFoundException exception = _errors.PricingNotFound(categoryId, tierId);

        // Assert
        exception.Should().BeOfType<NotFoundException>();
        exception.Message.Should().NotBeNullOrEmpty();
    }
}
