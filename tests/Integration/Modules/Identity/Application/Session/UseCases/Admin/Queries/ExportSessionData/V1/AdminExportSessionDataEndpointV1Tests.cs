using System.Text.Json;
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

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Sessions}/export");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AdminExportSessions_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Sessions}/export");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AdminExportSessions_AsSuperAdmin_Returns200()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Sessions}/export");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AdminExportSessions_AsSuperAdmin_WithSeededSessions_Returns200()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var sessions = SessionFactory.CreateMany(TestUser.SuperAdminId, 5);
        seedContext.Sessions.AddRange(sessions);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Sessions}/export");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Verifies that requesting a session data export with an unsupported format
    /// returns a 400 Bad Request due to the export format validation rule.
    /// </summary>
    [Fact]
    public async Task AdminExportSessions_WithInvalidFormat_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Sessions}/export?format=invalid_format");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Verifies that requesting a session data export with CSV format returns
    /// a 200 OK response with the correct CSV content type. This exercises
    /// the ExportFile branch of the endpoint.
    /// </summary>
    [Fact]
    public async Task AdminExportSessions_WithCsvFormat_ReturnsFileResponse()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var sessions = SessionFactory.CreateMany(TestUser.SuperAdminId, 2);
        seedContext.Sessions.AddRange(sessions);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Sessions}/export?format=csv");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/csv");
    }

    /// <summary>
    /// Verifies that requesting a session data export with XLSX format returns
    /// a 200 OK response with the correct spreadsheet content type.
    /// </summary>
    [Fact]
    public async Task AdminExportSessions_WithXlsxFormat_ReturnsFileResponse()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var sessions = SessionFactory.CreateMany(TestUser.SuperAdminId, 2);
        seedContext.Sessions.AddRange(sessions);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Sessions}/export?format=xlsx");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response
            .Content.Headers.ContentType?.MediaType.Should()
            .Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }

    /// <summary>
    /// Verifies that requesting a session data export with an invalid status
    /// returns a 400 Bad Request due to the status validation rule.
    /// </summary>
    [Fact]
    public async Task AdminExportSessions_WithInvalidStatus_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Sessions}/export?status=invalid_status");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
