using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Shared.Errors;

/// <summary>
/// Unit tests for <see cref="ShortVideoErrors"/>.
/// </summary>
public class ShortVideoErrorsTests
{
    private readonly ShortVideoErrors _errors = TestErrorsFactory.CreateShortVideoErrors();
    private readonly ShortVideoErrorMessage _message = LocalizerFactory.CreateMessage<ShortVideoErrorMessage>();

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
    public void TitleRequired_ShouldReturnBadRequestException()
    {
        BadRequestException exception = _errors.TitleRequired();

        exception.Should().NotBeNull();
        exception.Message.Should().Contain(_message.TitleRequired());
    }

    [Fact]
    public void SlugAlreadyExists_WithSlug_ShouldReturnConflictException()
    {
        // Arrange
        const string slug = "teaser-fally-focus";

        // Act
        ConflictException exception = _errors.SlugAlreadyExists(slug);

        // Assert
        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().Contain(slug);
    }
}
