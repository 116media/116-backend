using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Shared.Errors;

/// <summary>
/// Unit tests for <see cref="AlbumErrors" /> factory methods and the
/// <see cref="AlbumErrorMessage" /> localized strings backing them.
/// </summary>
public class AlbumErrorsTests
{
    private readonly AlbumErrors _errors = TestErrorsFactory.CreateAlbumErrors();
    private readonly AlbumErrorMessage _message = LocalizerFactory.CreateMessage<AlbumErrorMessage>("en");

    #region AlbumErrors

    [Fact]
    public void Msg_ShouldExposeUsableMessageProvider()
    {
        // Arrange & Act
        AlbumErrorMessage msg = _errors.Msg;

        // Assert
        msg.Should().NotBeNull();
        msg.NameRequired().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void NotFound_ShouldReturnNotFoundExceptionCarryingTheIdentifier()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        NotFoundException ex = _errors.NotFound(id);

        // Assert
        ex.Should().BeOfType<NotFoundException>();
        ex.Message.Should().NotBeNullOrWhiteSpace().And.Contain(id.ToString());
        ex.EntityName.Should().Be("Album");
    }

    [Fact]
    public void NameRequired_ShouldReturnBadRequestExceptionWithLocalizedMessage()
    {
        // Arrange & Act
        BadRequestException ex = _errors.NameRequired();

        // Assert
        ex.Should().BeOfType<BadRequestException>();
        ex.Message.Should().NotBeNullOrWhiteSpace().And.Contain(_message.NameRequired());
    }

    #endregion

    #region AlbumErrorMessage

    [Fact]
    public void Localizer_ShouldBeUsableForValidationExtensions()
    {
        // Arrange & Act
        string resolved = _message.Localizer["NameRequired"];

        // Assert
        _message.Localizer.Should().NotBeNull();
        resolved.Should().NotBeNullOrWhiteSpace().And.NotBe("NameRequired");
    }

    [Fact]
    public void NameRequiredMessage_ShouldResolveToLocalizedTextNotTheResourceKey()
    {
        // Arrange & Act
        string result = _message.NameRequired();

        // Assert
        result.Should().NotBeNullOrWhiteSpace().And.NotBe("NameRequired");
    }

    #endregion
}
