using System.Net;
using _116.Mailer.Application.Shared.Exceptions;
using _116.Mailer.Infrastructure.Services;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Mailer.Infrastructure.Services;

/// <summary>
/// Unit tests for the adapters' failure classification and the delivery
/// exception's transient default.
/// </summary>
public class EmailSenderClassificationTests
{
    [Fact]
    public void EmailDeliveryException_ShouldDefaultToTransient()
    {
        var exception = new EmailDeliveryException("timeout");

        exception.IsTransient.Should().BeTrue();
    }

    [Fact]
    public void EmailDeliveryException_Permanent_ShouldCarryTheFlag()
    {
        var exception = new EmailDeliveryException("bad recipient", isTransient: false);

        exception.IsTransient.Should().BeFalse();
    }

    [Theory]
    [InlineData(HttpStatusCode.TooManyRequests, true)]
    [InlineData(HttpStatusCode.InternalServerError, true)]
    [InlineData(HttpStatusCode.BadGateway, true)]
    [InlineData(HttpStatusCode.ServiceUnavailable, true)]
    [InlineData(HttpStatusCode.BadRequest, false)]
    [InlineData(HttpStatusCode.UnprocessableEntity, false)]
    [InlineData(HttpStatusCode.Unauthorized, false)]
    [InlineData(HttpStatusCode.Forbidden, false)]
    public void ResendIsTransient_ShouldClassifyStatusCodes(HttpStatusCode statusCode, bool expected)
    {
        ResendEmailSender.IsTransient(statusCode).Should().Be(expected);
    }
}
