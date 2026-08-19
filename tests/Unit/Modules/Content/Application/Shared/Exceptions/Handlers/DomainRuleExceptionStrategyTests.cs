using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Application.Shared.Exceptions.Handlers;
using _116.Content.Domain.StateMachines;
using _116.Shared.Domain.Exceptions;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Shared.Exceptions.Handlers;

/// <summary>
/// Unit tests for <see cref="DomainRuleExceptionStrategy"/>: the domain's culture-free
/// codes come out as the same localized details the handlers produced before the guards moved,
/// with the code and args carried as extensions.
/// </summary>
public class DomainRuleExceptionStrategyTests
{
    private readonly DomainRuleExceptionStrategy _strategy = new();

    private static DefaultHttpContext CreateContext()
    {
        ServiceProvider provider = new ServiceCollection()
            .AddLogging()
            .AddLocalization()
            .AddScoped<ArticleErrorMessage>()
            .AddScoped<VideoErrorMessage>()
            .AddScoped<LyricsErrorMessage>()
            .BuildServiceProvider();

        return new DefaultHttpContext
        {
            RequestServices = provider,
            Request = { Path = "/api/test" },
            TraceIdentifier = "test-trace-id",
        };
    }

    [Fact]
    public void ExceptionType_ShouldReturnDomainRuleExceptionType()
    {
        // Act & Assert
        _strategy.ExceptionType.Should().Be(typeof(DomainRuleException));
    }

    [Theory]
    [InlineData("Article")]
    [InlineData("Video")]
    [InlineData("Lyrics")]
    public void CreateProblemDetails_ForAnInvalidTransition_ShouldLocalizeThroughTheThrowingContentType(
        string contentType
    )
    {
        // Arrange
        DefaultHttpContext context = CreateContext();
        string expected = contentType switch
        {
            "Video" => context
                .RequestServices.GetRequiredService<VideoErrorMessage>()
                .InvalidStatusTransition("Draft", "Published"),
            "Lyrics" => context
                .RequestServices.GetRequiredService<LyricsErrorMessage>()
                .InvalidStatusTransition("Draft", "Published"),
            _ => context
                .RequestServices.GetRequiredService<ArticleErrorMessage>()
                .InvalidStatusTransition("Draft", "Published"),
        };
        var exception = new DomainRuleException(
            ContentRuleCodes.InvalidStatusTransition,
            contentType,
            "Draft",
            "Published"
        );

        // Act
        ProblemDetails problem = _strategy.CreateProblemDetails(exception, context);

        // Assert
        problem.Status.Should().Be(StatusCodes.Status400BadRequest);
        problem.Detail.Should().Be(expected);
        problem.Extensions["code"].Should().Be(ContentRuleCodes.InvalidStatusTransition);
        problem.Extensions["args"].Should().BeEquivalentTo(new[] { contentType, "Draft", "Published" });
    }

    [Fact]
    public void CreateProblemDetails_ForTheNotEditableRule_ShouldPhraseItAsTheUpdateGuardsAlwaysDid()
    {
        // Arrange
        DefaultHttpContext context = CreateContext();
        string expected = context
            .RequestServices.GetRequiredService<ArticleErrorMessage>()
            .InvalidStatusTransition("Published", "Draft/PendingPayment/PendingReview/Rejected (editable)");
        var exception = new DomainRuleException(ContentRuleCodes.NotEditable, "Article", "Published");

        // Act
        ProblemDetails problem = _strategy.CreateProblemDetails(exception, context);

        // Assert
        problem.Status.Should().Be(StatusCodes.Status400BadRequest);
        problem.Detail.Should().Be(expected);
    }

    [Fact]
    public void CreateProblemDetails_ForTheYoutubeUrlRule_ShouldLocalizeTheVideoMessage()
    {
        // Arrange
        DefaultHttpContext context = CreateContext();
        string expected = context
            .RequestServices.GetRequiredService<VideoErrorMessage>()
            .CannotPublishWithoutYoutubeUrl();
        var exception = new DomainRuleException(ContentRuleCodes.PublicationRequiresYoutubeUrl);

        // Act
        ProblemDetails problem = _strategy.CreateProblemDetails(exception, context);

        // Assert
        problem.Status.Should().Be(StatusCodes.Status400BadRequest);
        problem.Detail.Should().Be(expected);
    }

    [Fact]
    public void CreateProblemDetails_ForAnUnmappedCode_ShouldDegradeToTheCodeAsA400()
    {
        // Arrange — a rule added before its strategy arm must stay a refusal, never a 500
        DefaultHttpContext context = CreateContext();
        var exception = new DomainRuleException("content.some-future-rule", "x");

        // Act
        ProblemDetails problem = _strategy.CreateProblemDetails(exception, context);

        // Assert
        problem.Status.Should().Be(StatusCodes.Status400BadRequest);
        problem.Detail.Should().Be("content.some-future-rule");
    }
}
