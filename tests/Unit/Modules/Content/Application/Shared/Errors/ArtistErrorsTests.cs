using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Shared.Errors;

/// <summary>
/// Unit tests for <see cref="ArtistErrors" /> factory methods and the
/// <see cref="ArtistErrorMessage" /> localized strings backing them.
/// </summary>
public class ArtistErrorsTests
{
    private const string Slug = "fally-ipupa";

    private readonly ArtistErrors _errors = TestErrorsFactory.CreateArtistErrors();
    private readonly ArtistErrorMessage _message = LocalizerFactory.CreateMessage<ArtistErrorMessage>();

    #region ArtistErrors

    [Fact]
    public void Msg_ShouldExposeUsableMessageProvider()
    {
        // Arrange & Act
        ArtistErrorMessage msg = _errors.Msg;

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
        ex.EntityName.Should().Be("Artist");
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

    [Fact]
    public void SlugRequired_ShouldReturnBadRequestExceptionWithLocalizedMessage()
    {
        // Arrange & Act
        BadRequestException ex = _errors.SlugRequired();

        // Assert
        ex.Should().BeOfType<BadRequestException>();
        ex.Message.Should().NotBeNullOrWhiteSpace().And.Contain(_message.SlugRequired());
    }

    [Fact]
    public void SlugAlreadyExists_ShouldReturnConflictExceptionInterpolatingTheSlug()
    {
        // Arrange & Act
        ConflictException ex = _errors.SlugAlreadyExists(Slug);

        // Assert
        ex.Should().BeOfType<ConflictException>();
        ex.Message.Should().NotBeNullOrWhiteSpace().And.Contain(Slug);
    }

    [Fact]
    public void AlreadyClaimed_ShouldReturnConflictExceptionWithLocalizedMessage()
    {
        // Arrange & Act
        ConflictException ex = _errors.AlreadyClaimed();

        // Assert
        ex.Should().BeOfType<ConflictException>();
        ex.Message.Should().NotBeNullOrWhiteSpace().And.Contain(_message.AlreadyClaimed());
    }

    [Fact]
    public void ClaimRequestAlreadyExists_ShouldReturnConflictExceptionWithLocalizedMessage()
    {
        // Arrange & Act
        ConflictException ex = _errors.ClaimRequestAlreadyExists();

        // Assert
        ex.Should().BeOfType<ConflictException>();
        ex.Message.Should().NotBeNullOrWhiteSpace().And.Contain(_message.ClaimRequestAlreadyExists());
    }

    #endregion

    #region ArtistErrorMessage

    [Fact]
    public void NameRequiredMessage_ShouldResolveToLocalizedTextNotTheResourceKey()
    {
        // Arrange & Act
        string result = _message.NameRequired();

        // Assert
        result.Should().NotBeNullOrWhiteSpace().And.NotBe("NameRequired");
    }

    [Fact]
    public void SlugRequiredMessage_ShouldResolveToLocalizedTextNotTheResourceKey()
    {
        // Arrange & Act
        string result = _message.SlugRequired();

        // Assert
        result.Should().NotBeNullOrWhiteSpace().And.NotBe("SlugRequired");
    }

    [Fact]
    public void SlugAlreadyExistsMessage_ShouldInterpolateTheSlugArgument()
    {
        // Arrange & Act
        string result = _message.SlugAlreadyExists(Slug);

        // Assert
        result.Should().NotBeNullOrWhiteSpace().And.NotBe("SlugAlreadyExists").And.Contain(Slug);
        result.Should().NotContain("{0}");
    }

    [Fact]
    public void AlreadyClaimedMessage_ShouldResolveToLocalizedTextNotTheResourceKey()
    {
        // Arrange & Act
        string result = _message.AlreadyClaimed();

        // Assert
        result.Should().NotBeNullOrWhiteSpace().And.NotBe("AlreadyClaimed");
    }

    [Fact]
    public void ClaimRequestAlreadyExistsMessage_ShouldResolveToLocalizedTextNotTheResourceKey()
    {
        // Arrange & Act
        string result = _message.ClaimRequestAlreadyExists();

        // Assert
        result.Should().NotBeNullOrWhiteSpace().And.NotBe("ClaimRequestAlreadyExists");
    }

    #endregion

    #region Identity, Social Link and Directory Errors

    [Fact]
    public void TooManyAliases_ShouldReturnBadRequestException()
    {
        BadRequestException exception = _errors.TooManyAliases();

        exception.Should().NotBeNull();
        exception.Message.Should().Contain(_message.TooManyAliases());
    }

    [Fact]
    public void AliasTooLong_ShouldReturnBadRequestException()
    {
        BadRequestException exception = _errors.AliasTooLong();

        exception.Should().NotBeNull();
        exception.Message.Should().Contain(_message.AliasTooLong());
    }

    [Fact]
    public void BirthdateInFuture_ShouldReturnBadRequestException()
    {
        BadRequestException exception = _errors.BirthdateInFuture();

        exception.Should().NotBeNull();
        exception.Message.Should().Contain(_message.BirthdateInFuture());
    }

    [Fact]
    public void SocialLinkNotFound_ShouldReturnNotFoundExceptionNamingThePlatform()
    {
        NotFoundException exception = _errors.SocialLinkNotFound("Instagram");

        exception.Should().BeOfType<NotFoundException>();
        exception.Message.Should().Contain("Instagram");
    }

    #endregion
}
