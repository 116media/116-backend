using _116.Identity.Application.Session.Constants;
using _116.Identity.Application.Session.Services;
using _116.Identity.Application.Shared.Authorizations.Policies;
using _116.Identity.Domain.Constants;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.ValueObjects;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;

using Carter;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Identity.Application.Session.UseCases.Admin.Queries.ExportSessionData.V1;

/// <summary>
/// Response model for session data export (JSON format).
/// </summary>
/// <param name="SessionData">List of session export data.</param>
public record AdminExportSessionDataResponse(List<SessionExportDto> SessionData);

/// <summary>
/// Defines the admin export session data endpoint.
/// Handles retrieval of session data for export with optional filtering and format selection.
/// </summary>
public class AdminExportSessionDataEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the admin export session data route within the API pipeline.
    /// Maps the <c>/api/v1/admin/sessions/export</c> endpoint to handle export requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app
            .MapApiVersionGroup(1)
            .MapGroup($"{IdentityConstants.Admin}/{SessionRouteConstants.Endpoint}")
            .WithTags($"{IdentityConstants.Admin}::{IdentityConstants.SchemaName}");

        group.MapGet(pattern: SessionRouteConstants.Export, async (
                IDispatcher dispatcher,
                ISessionExportService exportService,
                string? status = null,
                DateTime? fromDate = null,
                DateTime? toDate = null,
                string? format = null,
                string? columns = null
            ) =>
            {
                var query = new AdminExportSessionDataQuery(
                    Status: status,
                    FromDate: fromDate,
                    ToDate: toDate,
                    Format: format,
                    Columns: columns
                );

                AdminExportSessionDataResult result = await dispatcher.Send(request: query);

                return string.IsNullOrWhiteSpace(value: format)
                    ? Results.Ok(new AdminExportSessionDataResponse(SessionData: result.SessionData))
                    : ExportFile(exportService: exportService, result: result, format: format, columns: columns);
            })
            .WithName(endpointName: AdminExportSessionDataMetaField.AdminExportSessionData.Name)
            .WithSummary(summary: AdminExportSessionDataMetaField.AdminExportSessionData.Summary)
            .WithDescription(description: AdminExportSessionDataMetaField.AdminExportSessionData.Description)
            .RequireAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireAuthorization(AccountStatusPolicies.RequireLoggedInUser)
            .ProducesValidationProblem()
            .Produces<AdminExportSessionDataResponse>()
            .Produces(statusCode: StatusCodes.Status200OK, contentType: SessionConstants.Export.CsvContentType)
            .Produces(statusCode: StatusCodes.Status200OK, contentType: SessionConstants.Export.XlsxContentType)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden);
    }

    /// <summary>
    /// Handles file export for CSV and XLSX formats.
    /// </summary>
    /// <param name="exportService">The export service.</param>
    /// <param name="result">The query result containing session data.</param>
    /// <param name="format">The export format string.</param>
    /// <param name="columns">The columns to export.</param>
    /// <returns>File result.</returns>
    private static IResult ExportFile(
        ISessionExportService exportService,
        AdminExportSessionDataResult result,
        string format,
        string? columns
    )
    {
        SessionExportFormat exportFormat = new ExportFormat(value: format).Value;

        List<string>? columnList = columns?
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        byte[] bytes = exportService.Export(sessions: result.SessionData, format: exportFormat, columns: columnList);

        return Results.File(
            fileContents: bytes,
            exportService.GetContentType(format: exportFormat),
            exportService.GenerateFileName(format: exportFormat)
        );
    }
}
