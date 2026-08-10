using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Shared.Errors;

/// <summary>
/// Unit tests for <see cref="ContentTypeErrors"/>.
/// </summary>
public class ContentTypeErrorsTests
{
    private readonly ContentTypeErrors _errors = TestErrorsFactory.CreateContentTypeErrors();
    private readonly ContentTypeErrorMessage _message = LocalizerFactory.CreateMessage<ContentTypeErrorMessage>();

    [Fact]
    public void AlreadyExists_WithName_ShouldReturnConflictException()
    {
        // Arrange
        string name = "Article";

        // Act
        ConflictException exception = _errors.AlreadyExists(name);

        // Assert
        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().Contain(name);
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
    public void NameRequired_ShouldReturnBadRequestException()
    {
        BadRequestException exception = _errors.NameRequired();

        exception.Should().NotBeNull();
        exception.Message.Should().Contain(_message.NameRequired());
    }
}
