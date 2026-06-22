using _116.Identity.Application.Session.UseCases.Admin.Queries.ExportSessionData.V1;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Session.UseCases.Admin.Queries.ExportSessionData.V1;

/// <summary>
/// Integration tests for the AdminExportSessionData endpoint.
/// </summary>
[Collection("Database")]
public class AdminExportSessionDataEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task AdminExportSessions_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync(Routes.Admin.Sessions.Export());

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AdminExportSessions_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync(Routes.Admin.Sessions.Export());

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AdminExportSessions_AsSuperAdmin_Returns200()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync(Routes.Admin.Sessions.Export());

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminExportSessionDataResponse body = await response.ReadAsAsync<AdminExportSessionDataResponse>();
        body.SessionData.Should().NotBeNull();
    }

    [Fact]
    public async Task AdminExportSessions_AsSuperAdmin_WithSeededSessions_Returns200()
    {
        List<SessionEntity> sessions = SessionFactory.CreateMany(TestUser.SuperAdminId, 5);
        await SeedAsync<IdentityDbContext>(ctx =>
        {
            ctx.Sessions.AddRange(sessions);
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync(Routes.Admin.Sessions.Export());

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminExportSessionDataResponse body = await response.ReadAsAsync<AdminExportSessionDataResponse>();
        body.SessionData.Should().Contain(s => s.UserId == TestUser.SuperAdminId);
    }

    /// <summary>
    /// Verifies that requesting a session data export with an unsupported format
    /// returns a 400 Bad Request due to the export format validation rule.
    /// </summary>
    [Fact]
    public async Task AdminExportSessions_WithInvalidFormat_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{Routes.Admin.Sessions.Export()}?format=invalid_format");

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Verifies that requesting a session data export with CSV format returns
    /// a 200 OK response with the correct CSV content type and a non-empty body.
    /// This exercises the ExportFile branch of the endpoint.
    /// </summary>
    [Fact]
    public async Task AdminExportSessions_WithCsvFormat_ReturnsFileResponse()
    {
        List<SessionEntity> sessions = SessionFactory.CreateMany(TestUser.SuperAdminId, 2);
        await SeedAsync<IdentityDbContext>(ctx =>
        {
            ctx.Sessions.AddRange(sessions);
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{Routes.Admin.Sessions.Export()}?format=csv");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("text/csv");
        (await response.Content.ReadAsByteArrayAsync()).Should().NotBeEmpty();
    }

    /// <summary>
    /// Verifies that requesting a session data export with XLSX format returns
    /// a 200 OK response with the correct spreadsheet content type and a non-empty body.
    /// </summary>
    [Fact]
    public async Task AdminExportSessions_WithXlsxFormat_ReturnsFileResponse()
    {
        List<SessionEntity> sessions = SessionFactory.CreateMany(TestUser.SuperAdminId, 2);
        await SeedAsync<IdentityDbContext>(ctx =>
        {
            ctx.Sessions.AddRange(sessions);
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{Routes.Admin.Sessions.Export()}?format=xlsx");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response
            .Content.Headers.ContentType!.MediaType.Should()
            .Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        (await response.Content.ReadAsByteArrayAsync()).Should().NotBeEmpty();
    }

    /// <summary>
    /// Verifies that requesting a session data export with an invalid status
    /// returns a 400 Bad Request due to the status validation rule.
    /// </summary>
    [Fact]
    public async Task AdminExportSessions_WithInvalidStatus_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{Routes.Admin.Sessions.Export()}?status=invalid_status");

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);
    }
}
