using _116.BuildingBlocks.Presentation.Logging;
using AwesomeAssertions;
using Xunit;

namespace _116.Shared.Unit.Tests.Shared.Application.Logging;

/// <summary>
/// Unit tests for <see cref="SearchQueryRedaction" />. The reader's term is the thing being
/// protected, so every case asserts the term is absent rather than that the output matches a
/// shape.
/// </summary>
public class SearchQueryRedactionTests
{
    private const string Term = "fally-ipupa-secret";

    [Theory]
    [InlineData("/api/v1/public/search?q=fally-ipupa-secret")]
    [InlineData("/api/v1/public/search?scope=All&q=fally-ipupa-secret")]
    [InlineData("/api/v1/public/search?q=fally-ipupa-secret&scope=All")]
    [InlineData("/api/v1/public/search?scope=All&q=fally-ipupa-secret&pageIndex=2")]
    [InlineData("/api/v1/public/search?Q=fally-ipupa-secret")]
    [InlineData("/api/v1/public/search?q=fally-ipupa-secret&q=fally-ipupa-secret")]
    public void Redact_WhereverTheTermSits_ShouldRemoveIt(string path)
    {
        // Act
        string redacted = SearchQueryRedaction.Redact(path);

        // Assert
        redacted.Should().NotContain(Term);
        redacted.Should().Contain("q=[redacted]");
    }

    [Fact]
    public void Redact_ShouldKeepTheRouteAndTheOtherParameters()
    {
        // Act
        string redacted = SearchQueryRedaction.Redact("/api/v1/public/search?scope=Articles&q=damso&pageIndex=3");

        // Assert
        redacted.Should().Be("/api/v1/public/search?scope=Articles&q=[redacted]&pageIndex=3");
    }

    [Fact]
    public void Redact_WithAnEmptyTerm_ShouldStillReplaceTheParameter()
    {
        // Act
        string redacted = SearchQueryRedaction.Redact("/api/v1/public/search?q=");

        // Assert
        redacted.Should().Be("/api/v1/public/search?q=[redacted]");
    }

    [Theory]
    [InlineData("/api/v1/public/faq?faq=fally-ipupa-secret")]
    [InlineData("/api/v1/public/articles?tag=fally-ipupa-secret")]
    [InlineData("/api/v1/public/articles?qq=fally-ipupa-secret")]
    public void Redact_WithAParameterThatMerelyEndsInQ_ShouldLeaveItAlone(string path)
    {
        // Act
        string redacted = SearchQueryRedaction.Redact(path);

        // Assert
        redacted.Should().Be(path, "only the search term parameter is redacted");
    }

    [Fact]
    public void Redact_WithNoQueryString_ShouldReturnThePathUnchanged()
    {
        // Act
        string redacted = SearchQueryRedaction.Redact("/api/v1/public/articles");

        // Assert
        redacted.Should().Be("/api/v1/public/articles");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Redact_WithNothingToRedact_ShouldReturnEmpty(string? path)
    {
        // Act
        string redacted = SearchQueryRedaction.Redact(path);

        // Assert
        redacted.Should().BeEmpty();
    }

    [Fact]
    public void Redact_WithAnEncodedTerm_ShouldRemoveTheWholeValue()
    {
        // Act
        string redacted = SearchQueryRedaction.Redact("/api/v1/public/search?q=r%C3%A9p%C3%A9tition%20secr%C3%A8te");

        // Assert
        redacted.Should().Be("/api/v1/public/search?q=[redacted]");
    }
}
