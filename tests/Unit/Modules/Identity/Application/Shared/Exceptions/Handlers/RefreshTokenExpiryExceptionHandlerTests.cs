using _116.Identity.Application.Shared.Exceptions;
using _116.Identity.Application.Shared.Exceptions.Handlers;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Shared.Exceptions.Handlers;

public class RefreshTokenExpiryExceptionHandlerTests
{
    private readonly RefreshTokenExpiryExceptionHandler _handler = new();

    [Fact]
    public void ExceptionType_ShouldReturnRefreshTokenExpiryExceptionType()
    {
        _handler.ExceptionType.Should().Be(typeof(RefreshTokenExpiryException));
    }

    [Fact]
    public void CreateProblemDetails_ShouldReturn403StatusCode()
    {
        var exception = new RefreshTokenExpiryException("Refresh token expired");
        var context = new DefaultHttpContext { Request = { Path = "/api/test" } };

        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        problemDetails.Status.Should().Be(StatusCodes.Status403Forbidden);
    }
}
