using _116.Content.Application.Shared.Errors;
using _116.Shared.Application.Exceptions;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Shared.Errors;

/// <summary>
/// Unit tests for <see cref="ContentTypeErrors"/>.
/// </summary>
public class ContentTypeErrorsTests
{
    [Fact]
    public void AlreadyExists_WithName_ShouldReturnConflictException()
    {
        // Arrange
        string name = "Article";

        // Act
        ConflictException exception = ContentTypeErrors.AlreadyExists(name);

        // Assert
        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().Contain(name);
    }

    [Fact]
    public void NotFound_WithId_ShouldReturnNotFoundException()
    {
        // Arrange
        Guid id = Guid.NewGuid();

        // Act
        NotFoundException exception = ContentTypeErrors.NotFound(id);

        // Assert
        exception.Should().BeOfType<NotFoundException>();
        exception.Message.Should().Contain(id.ToString());
    }

    [Fact]
    public void AlreadyActive_ShouldReturnConflictException()
    {
        // Act
        ConflictException exception = ContentTypeErrors.AlreadyActive();

        // Assert
        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void AlreadyInactive_ShouldReturnConflictException()
    {
        // Act
        ConflictException exception = ContentTypeErrors.AlreadyInactive();

        // Assert
        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void NameRequired_ShouldReturnBadRequestException()
    {
        // Act
        BadRequestException exception = ContentTypeErrors.NameRequired();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().NotBeNullOrEmpty();
    }
}
