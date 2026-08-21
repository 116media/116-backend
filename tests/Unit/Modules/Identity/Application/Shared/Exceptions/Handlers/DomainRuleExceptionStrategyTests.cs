using _116.Identity.Application.Shared.Errors.Messages;
using _116.Identity.Application.Shared.Exceptions.Handlers;
using _116.Identity.Domain.Exceptions;
using _116.Identity.Domain.StateMachines;
using _116.Shared.Application.Exceptions;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Shared.Exceptions.Handlers;

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
            .AddScoped<ConflictErrorMessage>()
            .AddScoped<AuthorizationErrorMessage>()
            .BuildServiceProvider();

        return new DefaultHttpContext
        {
            RequestServices = provider,
            Request = { Path = "/api/test" },
            TraceIdentifier = "test-trace-id",
        };
    }

    [Fact]
    public void ExceptionType_ShouldReturnIdentityRuleExceptionType()
    {
        // Act & Assert
        _strategy.ExceptionType.Should().Be(typeof(IdentityRuleException));
    }

    [Fact]
    public void CreateProblemDetails_ForARequiredFieldRule_ShouldAnswerAsTheOldBadRequest()
    {
        // Arrange
        DefaultHttpContext context = CreateContext();
        string expected = context.RequestServices.GetRequiredService<ValidationErrorMessage>().RoleNameRequired();
        var exception = new IdentityRuleException(IdentityRuleCodes.RoleNameRequired);

        // Act
        ProblemDetails problem = _strategy.CreateProblemDetails(exception, context);

        // Assert
        problem.Status.Should().Be(StatusCodes.Status400BadRequest);
        problem.Title.Should().Be(nameof(BadRequestException));
        problem.Detail.Should().Be(expected);
        problem.Extensions["code"].Should().Be(IdentityRuleCodes.RoleNameRequired);
    }

    [Fact]
    public void CreateProblemDetails_ForProviderMismatch_ShouldAnswerAsTheOldConflict()
    {
        // Arrange
        DefaultHttpContext context = CreateContext();
        string expected = context.RequestServices.GetRequiredService<ConflictErrorMessage>().ProviderMismatch();
        var exception = new IdentityRuleException(IdentityRuleCodes.ProviderMismatch);

        // Act
        ProblemDetails problem = _strategy.CreateProblemDetails(exception, context);

        // Assert
        problem.Status.Should().Be(StatusCodes.Status409Conflict);
        problem.Title.Should().Be(nameof(ConflictException));
        problem.Detail.Should().Be(expected);
    }

    [Fact]
    public void CreateProblemDetails_ForAccountInactive_ShouldStayLockedAndCarryTheEmail()
    {
        // Arrange
        DefaultHttpContext context = CreateContext();
        string expected = context
            .RequestServices.GetRequiredService<AuthorizationErrorMessage>()
            .AccountInactive(email: "user@example.com");
        var exception = new IdentityRuleException(IdentityRuleCodes.AccountInactive, "user@example.com");

        // Act
        ProblemDetails problem = _strategy.CreateProblemDetails(exception, context);

        // Assert
        problem.Status.Should().Be(StatusCodes.Status423Locked);
        problem.Title.Should().Be("AccountInactiveException");
        problem.Detail.Should().Be(expected);
        problem.Extensions["args"].Should().BeEquivalentTo(new[] { "user@example.com" });
    }

    [Fact]
    public void CreateProblemDetails_ForAccountNotVerified_ShouldStayForbidden()
    {
        // Arrange
        DefaultHttpContext context = CreateContext();
        var exception = new IdentityRuleException(IdentityRuleCodes.AccountNotVerified, "user@example.com");

        // Act
        ProblemDetails problem = _strategy.CreateProblemDetails(exception, context);

        // Assert
        problem.Status.Should().Be(StatusCodes.Status403Forbidden);
        problem.Title.Should().Be("AccountNotVerifiedException");
    }

    [Fact]
    public void CreateProblemDetails_ForTheEmailFormatRule_ShouldStayUnauthorized()
    {
        // Arrange
        DefaultHttpContext context = CreateContext();
        string expected = context
            .RequestServices.GetRequiredService<ValidationErrorMessage>()
            .InvalidEmailFormat(email: "bad@@example");
        var exception = new IdentityRuleException(IdentityRuleCodes.InvalidEmailFormat, "bad@@example");

        // Act
        ProblemDetails problem = _strategy.CreateProblemDetails(exception, context);

        // Assert
        problem.Status.Should().Be(StatusCodes.Status401Unauthorized);
        problem.Title.Should().Be(nameof(AuthenticationException));
        problem.Detail.Should().Be(expected);
    }

    [Fact]
    public void CreateProblemDetails_ForAnUnmappedCode_ShouldDegradeToTheCodeAsA400()
    {
        // Arrange — a rule added before its strategy arm must stay a refusal, never a 500
        DefaultHttpContext context = CreateContext();
        var exception = new IdentityRuleException("identity.some-future-rule");

        // Act
        ProblemDetails problem = _strategy.CreateProblemDetails(exception, context);

        // Assert
        problem.Status.Should().Be(StatusCodes.Status400BadRequest);
        problem.Detail.Should().Be("identity.some-future-rule");
    }
}
