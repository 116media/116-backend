using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.ResolveSingleStreamingLinks.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Integration.Tests.Common.Stubs;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Shared.Exceptions.Handlers;

/// <summary>
/// Covers the two strategies no mapped-exception test can reach: the fallback for an unexpected
/// fault and the client-cancellation handler. Both are driven through a real admin endpoint whose
/// stubbed resolution service is scripted to throw, so the assertion exercises the wiring — the
/// global middleware selecting the strategy and writing its ProblemDetails — rather than the
/// strategy in isolation. The host boots outside Development, so the fallback must withhold the raw
/// exception message.
/// </summary>
[Collection("Database")]
public class UnhandledExceptionPipelineTests(PostgresFixture db) : BaseApiTest(db)
{
    private const string SourceUrl = "https://open.spotify.com/track/xyz789";

    private StubStreamingLinkResolutionService StreamingStub =>
        Api.Services.GetRequiredService<StubStreamingLinkResolutionService>();

    private static string Url(Guid lyricsId) =>
        Routes.Admin.Editorial.ResolveStreamingLinks(EditorialRouteConstants.Lyrics, lyricsId);

    /// <summary>
    /// Seeds a standalone single so the resolve handler passes its own guards and reaches the
    /// stubbed resolution service, where the scripted exception is thrown.
    /// </summary>
    private async Task<Guid> SeedStandaloneLyricsAsync()
    {
        Guid categoryId = await SeedAsync<ContentDbContext, Guid>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            return category.Id;
        });

        LyricsEntity lyrics = await SeedAsync<ContentDbContext, LyricsEntity>(ctx =>
        {
            LyricsEntity entity = LyricsFactory.Create(categoryId);
            ctx.Lyrics.Add(entity);
            return entity;
        });

        return lyrics.Id;
    }

    [Fact]
    public async Task DefaultExceptionHandler_ShouldReturnSanitized500_WhenAnUnmappedExceptionEscapes()
    {
        const string leakedDetail = "connection host=db-primary password=hunter2 database=116";
        StreamingStub.NextUnhandledException = new InvalidOperationException(leakedDetail);

        Guid lyricsId = await SeedStandaloneLyricsAsync();
        Client.AuthenticateAsAdmin();

        var response = await Client.PostAsJsonAsync(
            Url(lyricsId),
            new AdminResolveSingleStreamingLinksRequest(SourceUrl)
        );

        // The fallback presents the sanitized envelope: InternalServerException title, generic detail.
        await response.ShouldBeProblem<InternalServerException>(
            HttpStatusCode.InternalServerError,
            Localized<SharedExceptionMessage>(m => m.UnexpectedError())
        );

        string raw = await response.Content.ReadAsStringAsync();
        raw.Should()
            .NotContain(leakedDetail, "the raw exception message must never reach the client outside Development");
        raw.Should().Contain("traceId", "the middleware enriches every problem with a correlation id");
        raw.Should().Contain("timestamp", "the middleware enriches every problem with a timestamp");
    }

    [Fact]
    public async Task OperationCanceledExceptionHandler_ShouldReturn499_WhenTheRequestIsCancelled()
    {
        StreamingStub.NextUnhandledException = new OperationCanceledException("The request was cancelled");

        Guid lyricsId = await SeedStandaloneLyricsAsync();
        Client.AuthenticateAsAdmin();

        var response = await Client.PostAsJsonAsync(
            Url(lyricsId),
            new AdminResolveSingleStreamingLinksRequest(SourceUrl)
        );

        await response.ShouldBeProblem<OperationCanceledException>(
            (HttpStatusCode)499,
            Localized<SharedExceptionMessage>(m => m.RequestCancelled())
        );
    }
}
