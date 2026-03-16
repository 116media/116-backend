using _116.Content.Application.Shared.Errors;
using _116.Shared.Application.Exceptions;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Shared.Errors;

/// <summary>
/// Unit tests for <see cref="ShortVideoErrors"/>.
/// </summary>
public class ShortVideoErrorsTests
{
    [Fact]
    public void NotFound_WithId_ShouldReturnNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        NotFoundException exception = ShortVideoErrors.NotFound(id);

        // Assert
        exception.Should().BeOfType<NotFoundException>();
        exception.Message.Should().Contain(id.ToString());
    }

    [Fact]
    public void AlreadyActive_ShouldReturnConflictException()
    {
        // Act
        ConflictException exception = ShortVideoErrors.AlreadyActive();

        // Assert
        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void AlreadyInactive_ShouldReturnConflictException()
    {
        // Act
        ConflictException exception = ShortVideoErrors.AlreadyInactive();

        // Assert
        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void TitleRequired_ShouldReturnBadRequestException()
    {
        // Act
        BadRequestException exception = ShortVideoErrors.TitleRequired();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void SlugAlreadyExists_WithSlug_ShouldReturnConflictException()
    {
        // Arrange
        const string slug = "teaser-fally-focus";

        // Act
        ConflictException exception = ShortVideoErrors.SlugAlreadyExists(slug);

        // Assert
        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().Contain(slug);
    }
}
