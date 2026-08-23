using _116.Mailer.Application.Shared.Errors;
using _116.Mailer.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Mailer.Application.Shared.Errors;

/// <summary>
/// Unit tests for <see cref="NewsletterErrors" /> factory methods and the
/// <see cref="NewsletterErrorMessage" /> localized strings backing them.
/// </summary>
public class NewsletterErrorsTests
{
    private readonly NewsletterErrors _errors = new(LocalizerFactory.CreateMessage<NewsletterErrorMessage>());
    private readonly NewsletterErrorMessage _message = LocalizerFactory.CreateMessage<NewsletterErrorMessage>();

    [Fact]
    public void Msg_ShouldExposeUsableMessageProvider()
    {
        // Arrange & Act
        NewsletterErrorMessage msg = _errors.Msg;

        // Assert
        msg.Should().NotBeNull();
        msg.TokenInvalid().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void TokenNotFound_ShouldReturnNotFoundExceptionWithLocalizedMessage()
    {
        // Arrange & Act
        NotFoundException ex = _errors.TokenNotFound();

        // Assert
        ex.Should().BeOfType<NotFoundException>();
        ex.Message.Should().NotBeNullOrWhiteSpace().And.Contain(_message.TokenInvalid());
    }
}
