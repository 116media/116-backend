using _116.Identity.Application.Auth.Validators;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Application.Shared.Errors.Facade;
using FluentValidation;

namespace _116.Identity.Application.Session.UseCases.Admin.Queries.ExportSessionData;

/// <summary>
/// Validator for <see cref="AdminExportSessionDataQuery" /> that ensures valid filter parameters.
/// </summary>
public class AdminExportSessionDataValidator : AbstractValidator<AdminExportSessionDataQuery>
{
    private static readonly string[] ValidColumns = typeof(SessionExportDto)
        .GetProperties()
        .Select(p => p.Name)
        .ToArray();

    /// <summary>
    /// Configure validation rules for the export query parameters.
    /// </summary>
    /// <param name="i18n">
    /// Identity module i18n facade for rule configuration.
    /// </param>
    public AdminExportSessionDataValidator(IdentityI18n i18n)
    {
        When(
            x => !string.IsNullOrWhiteSpace(value: x.Format),
            () => RuleFor(x => x.Format).ValidExportFormat(i18n.User.Validation)
        );

        When(
            x => !string.IsNullOrWhiteSpace(value: x.Columns),
            () => RuleFor(x => x.Columns).ValidExportColumns(ValidColumns, i18n.User.Validation)
        );

        When(
            x => !string.IsNullOrWhiteSpace(value: x.Status),
            () => RuleFor(x => x.Status).ValidExportStatus(i18n.User.Validation)
        );

        When(
            x => x.FromDate.HasValue && x.ToDate.HasValue,
            () =>
                RuleFor(x => x.ToDate)
                    .GreaterThanOrEqualTo(x => x.FromDate)
                    .WithMessage(i18n.User.Validation.ExportDateRangeInvalid())
        );
    }
}
