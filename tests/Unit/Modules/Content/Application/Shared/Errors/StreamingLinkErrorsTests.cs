using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Shared.Errors;

/// <summary>
/// Unit tests for <see cref="StreamingLinkErrors"/>.
/// </summary>
public class StreamingLinkErrorsTests
{
    private readonly StreamingLinkErrors _errors = TestErrorsFactory.CreateStreamingLinkErrors();
    private readonly StreamingLinkErrorMessage _message = LocalizerFactory.CreateMessage<StreamingLinkErrorMessage>();

    [Fact]
    public void ResolutionFailed_ShouldReturnBadGatewayException()
    {
        BadGatewayException exception = _errors.ResolutionFailed();

        exception.Should().NotBeNull();
        exception.Message.Should().Contain(_message.ResolutionFailed());
    }

    [Fact]
    public void ResolutionRateLimited_ShouldReturnRateLimitExceededException()
    {
        RateLimitExceededException exception = _errors.ResolutionRateLimited();

        exception.Should().NotBeNull();
        exception.Message.Should().Contain(_message.ResolutionRateLimited());
    }

    [Fact]
    public void NothingResolved_ShouldReturnNotFoundException()
    {
        NotFoundException exception = _errors.NothingResolved();

        exception.Should().NotBeNull();
        exception.Message.Should().Contain(_message.NothingResolved());
    }

    [Fact]
    public void Messages_ShouldResolveToLocalizedTextNotTheResourceKeys()
    {
        _message.ResolutionFailed().Should().NotBeNullOrWhiteSpace().And.NotBe("ResolutionFailed");
        _message.NothingResolved().Should().NotBeNullOrWhiteSpace().And.NotBe("NothingResolved");
        _message.ResolutionRateLimited().Should().NotBeNullOrWhiteSpace().And.NotBe("ResolutionRateLimited");
    }

    [Fact]
    public void UnresolvableSourceUrl_ShouldResolveToLocalizedText()
    {
        _message.UnresolvableSourceUrl().Should().NotBeNullOrWhiteSpace().And.NotBe("UnresolvableSourceUrl");
    }

    [Fact]
    public void Localizer_ShouldBeExposedForValidationExtensions()
    {
        _errors.Msg.Should().NotBeNull();
        _message.Localizer.Should().NotBeNull();
    }
}
