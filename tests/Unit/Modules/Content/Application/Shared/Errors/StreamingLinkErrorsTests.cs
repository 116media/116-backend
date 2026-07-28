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
    private readonly StreamingLinkErrorMessage _message = LocalizerFactory.CreateMessage<StreamingLinkErrorMessage>(
        "en"
    );

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
    public void UnresolvableSourceUrl_ShouldReturnBadRequestException()
    {
        BadRequestException exception = _errors.UnresolvableSourceUrl();

        exception.Should().NotBeNull();
        exception.Message.Should().Contain(_message.UnresolvableSourceUrl());
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
    }
}
