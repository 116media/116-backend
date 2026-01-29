using _116.Identity.Application.Auth.Exceptions;
using _116.Identity.Application.Auth.Exceptions.Handlers;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.Exceptions.Handlers;

public class OtpAttemptsLimitExceptionHandlerTests
{
    private readonly OtpAttemptsLimitExceptionHandler _handler = new();

    [Fact]
    public void ExceptionType_ShouldReturnOtpAttemptsLimitExceptionType()
    {
        _handler.ExceptionType.Should().Be(typeof(OtpAttemptsLimitException));
    }

    [Fact]
    public void CreateProblemDetails_ShouldReturn429StatusCode()
    {
        var exception = new OtpAttemptsLimitException("Too many OTP attempts");
        var context = new DefaultHttpContext { Request = { Path = "/api/test" } };

        var problemDetails = _handler.CreateProblemDetails(exception, context);

        problemDetails.Status.Should().Be(StatusCodes.Status429TooManyRequests);
    }
}
