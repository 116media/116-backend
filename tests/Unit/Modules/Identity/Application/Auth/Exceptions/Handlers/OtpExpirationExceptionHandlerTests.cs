using _116.Identity.Application.Auth.Exceptions;
using _116.Identity.Application.Auth.Exceptions.Handlers;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.Exceptions.Handlers;

public class OtpExpirationExceptionHandlerTests
{
    private readonly OtpExpirationExceptionHandler _handler = new();

    [Fact]
    public void ExceptionType_ShouldReturnOtpExpirationExceptionType()
    {
        _handler.ExceptionType.Should().Be(typeof(OtpExpirationException));
    }

    [Fact]
    public void CreateProblemDetails_ShouldReturn410StatusCode()
    {
        var exception = new OtpExpirationException("OTP expired");
        var context = new DefaultHttpContext { Request = { Path = "/api/test" } };

        var problemDetails = _handler.CreateProblemDetails(exception, context);

        problemDetails.Status.Should().Be(StatusCodes.Status410Gone);
    }
}
