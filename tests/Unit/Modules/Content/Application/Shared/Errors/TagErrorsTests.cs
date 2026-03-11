using _116.Content.Application.Shared.Errors;
using _116.Shared.Application.Exceptions;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Shared.Errors;

/// <summary>
/// Unit tests for <see cref="TagErrors"/>.
/// </summary>
public class TagErrorsTests
{
    [Fact]
    public void SlugAlreadyExists_WithSlug_ShouldReturnConflictException()
    {
        // Arrange
        string slug = "fally-ipupa";

        // Act
        ConflictException exception = TagErrors.SlugAlreadyExists(slug);

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
        NotFoundException exception = TagErrors.NotFound(id);

        // Assert
        exception.Should().BeOfType<NotFoundException>();
        exception.Message.Should().Contain(id.ToString());
    }

    [Fact]
    public void NameRequired_ShouldReturnBadRequestException()
    {
        // Act
        BadRequestException exception = TagErrors.NameRequired();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void SlugRequired_ShouldReturnBadRequestException()
    {
        // Act
        BadRequestException exception = TagErrors.SlugRequired();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().NotBeNullOrEmpty();
    }
}
