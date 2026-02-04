using _116.Identity.Application.Shared.Exceptions;
using _116.Identity.Application.Shared.Exceptions.Handlers;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Shared.Exceptions.Handlers;

public class AccountNotVerifiedExceptionHandlerTests
{
    private readonly AccountNotVerifiedExceptionHandler _handler = new();

    [Fact]
    public void ExceptionType_ShouldReturnAccountNotVerifiedExceptionType()
    {
        _handler.ExceptionType.Should().Be(typeof(AccountNotVerifiedException));
    }

    [Fact]
    public void CreateProblemDetails_ShouldReturn403StatusCode()
    {
        var exception = new AccountNotVerifiedException("Account not verified");
        var context = new DefaultHttpContext { Request = { Path = "/api/test" } };

        var problemDetails = _handler.CreateProblemDetails(exception, context);

        problemDetails.Status.Should().Be(StatusCodes.Status403Forbidden);
    }
}
