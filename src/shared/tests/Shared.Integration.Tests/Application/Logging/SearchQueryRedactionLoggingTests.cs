using Serilog.Events;

namespace _116.Shared.Integration.Tests.Application.Logging;

/// <summary>
/// Verifies that a reader's search term never reaches the log store. The request-logging
/// enrichment in <c>Program.cs</c> is the only thing standing between the query string and
/// Seq, so it is driven over real HTTP and asserted on the events the host actually wrote.
/// </summary>
/// <param name="db">The dedicated Testcontainer database and log-capturing application host.</param>
[Collection("RequestLogging")]
public class SearchQueryRedactionLoggingTests(RequestLogPostgresFixture db) : IDisposable
{
    private const string Term = "fally-ipupa-secret";

    private readonly HttpClient _client = db.Api.CreateClient();
    private readonly RequestLogApiFixture _api = (RequestLogApiFixture)db.Api;

    /// <inheritdoc />
    public void Dispose()
    {
        _client.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task SearchRequest_ShouldNotLeaveTheTermAnywhereInTheLog()
    {
        // Arrange
        _api.Sink.Reset();

        // Act
        using HttpResponseMessage response = await _client.GetAsync($"{ApiRoutes.Public.Search}?q={Term}&scope=All");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        IReadOnlyList<LogEvent> events = _api.Sink.Events;
        events.Should().NotBeEmpty("the request-logging middleware writes one event per request");

        string everythingLogged = string.Join(
            Environment.NewLine,
            events.Select(logEvent =>
                $"{logEvent.RenderMessage()} {string.Join(" ", logEvent.Properties.Select(p => $"{p.Key}={p.Value}"))}"
            )
        );

        everythingLogged.Should().NotContain(Term);
    }

    [Fact]
    public async Task SearchRequest_ShouldLogTheQueryWithTheTermReplaced()
    {
        // Arrange
        _api.Sink.Reset();

        // Act
        using HttpResponseMessage response = await _client.GetAsync(
            $"{ApiRoutes.Public.Search}?q={Term}&scope=Articles"
        );

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        IReadOnlyList<string> queries = _api.Sink.PropertyValues("RequestQuery");

        queries.Should().NotBeEmpty("the enricher sets RequestQuery on the completion event");
        queries.Should().Contain(query => query.Contains("q=[redacted]"));
        queries.Should().Contain(query => query.Contains("scope=Articles"), "only the term is redacted");
        queries.Should().AllSatisfy(query => query.Should().NotContain(Term));
    }

    [Fact]
    public async Task SearchRequest_ShouldKeepTheQueryOutOfTheRequestPath()
    {
        // Arrange
        _api.Sink.Reset();

        // Act
        using HttpResponseMessage response = await _client.GetAsync($"{ApiRoutes.Public.Search}?q={Term}");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        IReadOnlyList<string> paths = _api.Sink.PropertyValues("RequestPath");

        paths.Should().NotBeEmpty();
        paths
            .Should()
            .AllSatisfy(path =>
                path.Should().NotContain("?", "Serilog builds RequestPath and overwrites the enriched value")
            );
    }

    [Fact]
    public async Task RequestWithoutATerm_ShouldStillLogItsRoute()
    {
        // Arrange
        _api.Sink.Reset();

        // Act
        using HttpResponseMessage response = await _client.GetAsync(ApiRoutes.Public.Articles);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        _api.Sink.PropertyValues("RequestPath").Should().Contain(path => path.Contains(ApiRoutes.Public.Articles));
    }
}
