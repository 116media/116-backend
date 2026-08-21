using _116.Core.Application.Shared.Errors.Messages;
using _116.Core.Application.Shared.Exceptions.Handlers;
using _116.Core.Domain.Exceptions;
using _116.Core.Domain.StateMachines;
using _116.Shared.Application.Exceptions;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace _116.Unit.Tests.Modules.Core.Application.Shared.Exceptions.Handlers;

/// <summary>
/// Unit tests for <see cref="DomainRuleExceptionStrategy"/>: the domain's culture-free codes come
/// out with the same status, title and localized detail the retired exceptions produced, with the
/// code and args carried as extensions.
/// </summary>
public class DomainRuleExceptionStrategyTests
{
    private readonly DomainRuleExceptionStrategy _strategy = new();

    private static DefaultHttpContext CreateContext()
    {
        ServiceProvider provider = new ServiceCollection()
            .AddLogging()
            .AddLocalization()
            .AddScoped<ValidationErrorMessage>()
            .BuildServiceProvider();

        return new DefaultHttpContext
        {
            RequestServices = provider,
            Request = { Path = "/api/test" },
            TraceIdentifier = "test-trace-id",
        };
    }

    [Fact]
    public void ExceptionType_ShouldReturnCoreRuleExceptionType()
    {
        // Act & Assert
        _strategy.ExceptionType.Should().Be(typeof(CoreRuleException));
    }

    [Fact]
    public void CreateProblemDetails_ForAFileGuard_ShouldAnswerAsTheOldBadRequest()
    {
        // Arrange
        DefaultHttpContext context = CreateContext();
        string expected = context.RequestServices.GetRequiredService<ValidationErrorMessage>().FileNameRequired();
        var exception = new CoreRuleException(CoreRuleCodes.FileNameRequired);

        // Act
        ProblemDetails problem = _strategy.CreateProblemDetails(exception, context);

        // Assert
        problem.Status.Should().Be(StatusCodes.Status400BadRequest);
        problem.Title.Should().Be(nameof(BadRequestException));
        problem.Detail.Should().Be(expected);
        problem.Extensions["code"].Should().Be(CoreRuleCodes.FileNameRequired);
    }

    [Fact]
    public void CreateProblemDetails_ForAnUnmappedCode_ShouldDegradeToTheCodeAsA400()
    {
        // Arrange — a rule added before its strategy arm must stay a refusal, never a 500
        DefaultHttpContext context = CreateContext();
        var exception = new CoreRuleException("core.some-future-rule");

        // Act
        ProblemDetails problem = _strategy.CreateProblemDetails(exception, context);

        // Assert
        problem.Status.Should().Be(StatusCodes.Status400BadRequest);
        problem.Detail.Should().Be("core.some-future-rule");
    }
}
