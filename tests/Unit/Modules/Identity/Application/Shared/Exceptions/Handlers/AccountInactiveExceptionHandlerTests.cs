using _116.Identity.Application.Shared.Exceptions;
using _116.Identity.Application.Shared.Exceptions.Handlers;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Shared.Exceptions.Handlers;

public class AccountInactiveExceptionHandlerTests
{
    private readonly AccountInactiveExceptionHandler _handler = new();

    [Fact]
    public void ExceptionType_ShouldReturnAccountInactiveExceptionType()
    {
        _handler.ExceptionType.Should().Be(typeof(AccountInactiveException));
    }

    [Fact]
    public void CreateProblemDetails_ShouldReturn423StatusCode()
    {
        var exception = new AccountInactiveException("Account inactive");
        var context = new DefaultHttpContext { Request = { Path = "/api/test" } };

        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        problemDetails.Status.Should().Be(StatusCodes.Status423Locked);
    }
}
