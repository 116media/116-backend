using _116.Content.Application.Editorial.UseCases.Admin.Queries.GetLyricsSubmissions.V1;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Queries.GetLyricsSubmissions.V1;

/// <summary>
/// Integration tests for the AdminGetLyricsSubmissions endpoint.
/// </summary>
[Collection("Database")]
public class AdminGetLyricsSubmissionsEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task GetLyricsSubmissions_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync(Routes.Admin.Lyrics.Submissions());

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetLyricsSubmissions_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync(Routes.Admin.Lyrics.Submissions());

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetLyricsSubmissions_AsAdmin_ReturnsPaginatedSubmissions()
    {
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ctx.LyricsSubmissions.AddRange(LyricsSubmissionFactory.Create(), LyricsSubmissionFactory.Create());
        });

        Client.AuthenticateAsAdmin();

        var response = await Client.GetAsync($"{Routes.Admin.Lyrics.Submissions()}?pageIndex=0&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<AdminGetLyricsSubmissionsResponse>();

        body.Submissions.Items.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task GetLyricsSubmissions_FilteredByStatus_ReturnsOnlyMatchingSubmissions()
    {
        (LyricsSubmissionEntity pending, LyricsSubmissionEntity rejected) = await SeedAsync<
            ContentDbContext,
            (LyricsSubmissionEntity, LyricsSubmissionEntity)
        >(ctx =>
        {
            LyricsSubmissionEntity pending = LyricsSubmissionFactory.Create();
            LyricsSubmissionEntity rejected = LyricsSubmissionFactory.CreateRejected(Guid.NewGuid());
            ctx.LyricsSubmissions.AddRange(pending, rejected);
            return (pending, rejected);
        });

        Client.AuthenticateAsAdmin();

        var response = await Client.GetAsync(
            $"{Routes.Admin.Lyrics.Submissions()}?pageIndex=0&pageSize=10&status={EnumSubmissionStatus.Pending}"
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<AdminGetLyricsSubmissionsResponse>();

        body.Submissions.Items.Should().Contain(s => s.Id == pending.Id);
        body.Submissions.Items.Should().NotContain(s => s.Id == rejected.Id);
    }
}
